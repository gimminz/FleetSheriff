using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_AI_NAVIGATION
using UnityEngine.AI;
#endif

public class EnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    public GameObject enemy;
    public Transform player;
    public EnemyIndicator enemyIndicator;
    public MissionSuccessUI missionSuccessUI;

    [Header("Spawn Settings")]
    [Tooltip("스폰 간격 (초) - 이 시간마다 2기씩 생성")]
    public float spawnInterval = 90f;  // ✅ 20 → 90초로 대폭 증가

    [Tooltip("스폰 반경 (유닛) - 플레이어로부터 이 거리에서 생성")]
    public float spawnRadius = 3000f;  // ✅ 300 → 3000으로 대폭 증가 (플레이어 속도 200 고려)

    [Tooltip("고도 각도 (상/하 그룹)")]
    public float altitudePitchDeg = 30f;

    [Tooltip("플레이어 중심 링 방식 사용 (false 권장: 플레이어가 빠르게 이동할 때 뒤에서 스폰 방지)")]
    public bool usePlayerCenteredRing = false;  // ✅ true → false (고정 위치 스폰)

    [Header("Fixed Spawn Zone")]
    [Tooltip("고정 스폰 존 중심 (usePlayerCenteredRing=false일 때)")]
    public Vector3 fixedSpawnCenter = Vector3.zero;

    [Tooltip("플레이어 진행 방향 앞쪽에만 스폰 (전방 반구)")]
    public bool spawnOnlyInFront = true;  // ✅ 새로 추가: 전방에만 스폰

    [Header("Wave Control")]
    [Tooltip("동시에 존재 가능한 최대 적 수")]
    public int maxConcurrentEnemies = 4;  // ✅ 6 → 4기로 감소

    private int currentEnemyCount = 0;

    [Header("Test One-Enemy Mode")]
    [Tooltip("시작 시 플레이어 정면에 테스트용 적 1기를 소환합니다.")]
    public bool spawnTestEnemyOnStart = true;
    [Tooltip("테스트 적까지의 거리")]
    public float testEnemyDistance = 500f;  // ✅ 30 → 500으로 대폭 증가
    [Tooltip("테스트 적의 높이 보정(플레이어 기준)")]
    public float testEnemyHeightOffset = 0f;

    [Header("Debug")]
    public bool drawGizmos = true;
    public bool showDebugLogs = false;  // ✅ 디버그 로그 토글

    private readonly Dictionary<int, int> _occupied = new Dictionary<int, int>();
    private readonly int[][] _pairs = new int[][] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5, 6 }, new[] { 7, 8 } };
    private int _pairIndex = 0;
    private bool _nextOrbitPlusX = true;
    private bool successShown = false;

    void Awake()
    {
        for (int n = 1; n <= 8; n++) _occupied[n] = 0;
    }

    void Start()
    {
        if (spawnTestEnemyOnStart)
        {
            SpawnOneInFront(distance: testEnemyDistance, stationary: true, heightOffset: testEnemyHeightOffset);
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    // ----------- 테스트용 1기 소환 -----------
    public GameObject SpawnOneInFront(float distance = 100f, bool stationary = true, float heightOffset = 0f)
    {
        if (!enemy || !player)
        {
            Debug.LogWarning("[EnemySpawner] enemy 또는 player가 비어 있어 테스트 적을 소환할 수 없습니다.");
            return null;
        }

        Vector3 spawnPos = player.position + player.forward * Mathf.Max(0.01f, distance);
        spawnPos.y += heightOffset;

        Quaternion look = Quaternion.LookRotation((player.position - spawnPos).normalized, Vector3.up);

        var go = Instantiate(enemy, spawnPos, look);

        if (enemyIndicator) enemyIndicator.AddEnemy(go.transform);

        currentEnemyCount++;  // ✅ 카운트 증가

        var e = go.GetComponent<Enemy>();
        if (e)
        {
            e.OnDeath += () =>
            {
                currentEnemyCount--;  // ✅ 사망 시 카운트 감소
                if (!successShown)
                {
                    successShown = true;
                    missionSuccessUI?.Show();
                }
            };
        }

        if (stationary) MakeCompletelyStationary(go, alsoStopAttacks: true);

        return go;
    }

    void MakeCompletelyStationary(GameObject go, bool alsoStopAttacks)
    {
        var move = go.GetComponent<EnemyMovement>();
        if (move) move.enabled = false;

#if UNITY_AI_NAVIGATION
        var agent = go.GetComponent<NavMeshAgent>();
        if (agent)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.enabled = false;
        }
#endif

        var rb = go.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        var animator = go.GetComponentInChildren<Animator>();
        if (animator)
        {
            animator.applyRootMotion = false;
        }

        if (alsoStopAttacks)
        {
            var attack1 = go.GetComponent<EnemyAttackController>();
            if (attack1) attack1.enabled = false;
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // ✅ 최대 개체 수 체크
            if (currentEnemyCount >= maxConcurrentEnemies)
            {
                if (showDebugLogs)
                    Debug.Log($"[EnemySpawner] 최대 적 수 도달 ({currentEnemyCount}/{maxConcurrentEnemies}). 스폰 스킵.");
                continue;
            }

            TrySpawnTwo();
        }
    }

    void TrySpawnTwo()
    {
        // ✅ 최대 개체 수 체크 (2기 스폰 가능한지)
        if (currentEnemyCount + 2 > maxConcurrentEnemies)
        {
            if (showDebugLogs)
                Debug.Log($"[EnemySpawner] 2기 스폰 불가. 현재: {currentEnemyCount}, 최대: {maxConcurrentEnemies}");
            return;
        }

        bool anyFree = false;
        for (int n = 1; n <= 8; n++) if (_occupied[n] == 0) { anyFree = true; break; }
        if (!anyFree) return;

        int spawned = 0;
        int attempts = 0;

        while (spawned < 2 && attempts < 8)
        {
            int[] pair = _pairs[_pairIndex];
            foreach (var num in pair)
            {
                if (spawned >= 2) break;
                if (_occupied[num] > 0) continue;

                var go = InstantiateEnemyInNumber(num, spawned == 0 ? _nextOrbitPlusX : !_nextOrbitPlusX);
                if (go)
                {
                    _occupied[num]++;
                    spawned++;
                }
            }
            _pairIndex = (_pairIndex + 1) % _pairs.Length;
            attempts += 2;
        }

        if (spawned > 0)
        {
            _nextOrbitPlusX = !_nextOrbitPlusX;
            if (showDebugLogs)
                Debug.Log($"[EnemySpawner] {spawned}기 스폰 완료. 총 적: {currentEnemyCount}");
        }
    }

    GameObject InstantiateEnemyInNumber(int number, bool orbitPlusX)
    {
        if (!enemy || !player) return null;

        int sectorIndex = NumberToSectorIndex(number);
        float centerDeg = sectorIndex * 45f + 22.5f;

        // ✅ 스폰 중심 결정
        Vector3 center = usePlayerCenteredRing ? player.position : fixedSpawnCenter;

        Vector3 dirXZ = AngleToDirXZ(centerDeg);
        Vector3 spawnPos = center + dirXZ * spawnRadius;

        // ✅ 전방에만 스폰하는 옵션 (플레이어 속도가 빠를 때 권장)
        if (spawnOnlyInFront && player)
        {
            // 스폰 위치가 플레이어 진행 방향 기준 앞쪽인지 체크
            Vector3 toSpawn = spawnPos - player.position;
            toSpawn.y = 0f;
            Vector3 playerForwardFlat = player.forward;
            playerForwardFlat.y = 0f;

            float dot = Vector3.Dot(playerForwardFlat.normalized, toSpawn.normalized);

            // 뒤쪽이면 앞쪽으로 반사
            if (dot < 0f)
            {
                // 스폰 위치를 플레이어 전방으로 재배치
                Vector3 reflected = Vector3.Reflect(toSpawn, playerForwardFlat.normalized);
                spawnPos = player.position + reflected.normalized * spawnRadius;

                if (showDebugLogs)
                    Debug.Log($"[EnemySpawner] 후방 스폰 감지 → 전방으로 재배치 (섹터 {number})");
            }
        }

        bool isUpGroup = (number == 1 || number == 2 || number == 8 || number == 7);
        float pitch = isUpGroup ? altitudePitchDeg : -altitudePitchDeg;

        Quaternion look = Quaternion.LookRotation((player.position - spawnPos).normalized, Vector3.up) * Quaternion.Euler(pitch, 0f, 0f);

        var go = Instantiate(enemy, spawnPos, look);

        if (enemyIndicator) enemyIndicator.AddEnemy(go.transform);

        currentEnemyCount++;  // ✅ 카운트 증가

        var e = go.GetComponent<Enemy>();
        if (e)
        {
            e.OnDeath += () =>
            {
                currentEnemyCount--;  // ✅ 사망 시 카운트 감소
                HandleEnemyDeath(number);
            };
            e.SetSpawnerOccupyRelease(() =>
            {
                _occupied[number] = Mathf.Max(0, _occupied[number] - 1);
            });
        }

        var move = go.GetComponent<EnemyMovement>();
        if (move)
        {
            move.player = player;
            move.initialOrbitPlusX = orbitPlusX;
            move.speed = 80f;  // ✅ 60 → 80 (플레이어 속도 200에 비해 느리게)
            // ✅ Orbit 진입 거리 및 반경 증가 (장거리 전투)
            move.orbitEnterDistance = 400f;  // 60 → 400 (훨씬 멀리서 궤도 진입)
            move.orbitRadius = 250f;         // 50 → 250 (넓은 궤도)
        }

        return go;
    }

    void HandleEnemyDeath(int number)
    {
        if (!successShown)
        {
            successShown = true;
            missionSuccessUI?.Show();
        }
    }

    static int NumberToSectorIndex(int number)
    {
        switch (number)
        {
            case 1: return 0;
            case 2: return 1;
            case 8: return 2;
            case 7: return 3;
            case 4: return 4;
            case 3: return 5;
            case 5: return 6;
            case 6: return 7;
            default: return 0;
        }
    }

    static Vector3 AngleToDirXZ(float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || !player) return;

        // ✅ 스폰 존 시각화
        Vector3 center = usePlayerCenteredRing ? player.position : fixedSpawnCenter;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, spawnRadius);

        // ✅ 스폰 포인트 시각화 개선
        for (int i = 0; i < 8; i++)
        {
            float deg = i * 45f + 22.5f;
            Vector3 dir = AngleToDirXZ(deg);
            Vector3 pos = center + dir * spawnRadius;

            // 전방/후방 구분 색상
            if (spawnOnlyInFront && player)
            {
                Vector3 toSpawn = pos - player.position;
                toSpawn.y = 0f;
                Vector3 playerForwardFlat = player.forward;
                playerForwardFlat.y = 0f;
                float dot = Vector3.Dot(playerForwardFlat.normalized, toSpawn.normalized);

                Gizmos.color = dot >= 0f ? Color.green : Color.red;  // 전방=초록, 후방=빨강
            }
            else
            {
                Gizmos.color = Color.yellow;
            }

            Gizmos.DrawWireSphere(pos, 20f);
            Gizmos.DrawLine(center, pos);
        }

        // ✅ 플레이어 전방 방향 표시
        if (spawnOnlyInFront)
        {
            Gizmos.color = Color.blue;
            Vector3 forwardLine = player.position + player.forward * spawnRadius;
            Gizmos.DrawLine(player.position, forwardLine);
            Gizmos.DrawWireSphere(forwardLine, 50f);
        }

        if (spawnTestEnemyOnStart)
        {
            Gizmos.color = Color.magenta;
            Vector3 p0 = player.position;
            Vector3 p1 = p0 + player.forward * Mathf.Max(0.01f, testEnemyDistance);
            p1.y += testEnemyHeightOffset;
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawSphere(p1, 10.0f);
        }
    }
}