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
    public float spawnInterval = 20f;
    public float spawnRadius = 300f;
    public float altitudePitchDeg = 30f;
    public bool usePlayerCenteredRing = true;

    [Header("Test One-Enemy Mode")]
    [Tooltip("시작 시 플레이어 정면에 테스트용 적 1기를 소환합니다.")]
    public bool spawnTestEnemyOnStart = true;
    [Tooltip("테스트 적까지의 거리")]
    public float testEnemyDistance = 30f;
    [Tooltip("테스트 적의 높이 보정(플레이어 기준)")]
    public float testEnemyHeightOffset = 0f;

    [Header("Debug")]
    public bool drawGizmos = true;

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
            // 테스트 모드일 땐 무조건 '완전 정지'로 소환
            SpawnOneInFront(distance: testEnemyDistance, stationary: true, heightOffset: testEnemyHeightOffset);
            return; // 링 스폰 루틴 비활성
        }

        StartCoroutine(SpawnRoutine());
    }

    // ----------- 테스트용 1기 소환 -----------
    public GameObject SpawnOneInFront(float distance = 30f, bool stationary = true, float heightOffset = 0f)
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

        var e = go.GetComponent<Enemy>();
        if (e)
        {
            e.OnDeath += () =>
            {
                if (!successShown)
                {
                    successShown = true;
                    missionSuccessUI?.Show();
                }
            };
        }

        // 무조건 정지시킴(테스트 모드 확실)
        if (stationary) MakeCompletelyStationary(go, alsoStopAttacks: true);

        return go;
    }
    // --------------------------------------

    // 적을 '완전 정지' 상태로 만드는 유틸
    void MakeCompletelyStationary(GameObject go, bool alsoStopAttacks)
    {
        // 1) 커스텀 이동 컴포넌트
        var move = go.GetComponent<EnemyMovement>();
        if (move) move.enabled = false;

        // 2) NavMeshAgent(있다면)
#if UNITY_AI_NAVIGATION
        var agent = go.GetComponent<NavMeshAgent>();
        if (agent)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.enabled = false;
        }
#endif

        // 3) 리지드바디 정지/고정
        var rb = go.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // 외력/중력 영향 제거
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // 4) 애니메이터 루트모션/재생 속도 중단(있다면)
        var animator = go.GetComponentInChildren<Animator>();
        if (animator)
        {
            animator.applyRootMotion = false;
            // 필요하면 특정 정지 포즈를 위해 speed=0
            // animator.speed = 0f; // (정지 모션이 없으면 주석 해제)
        }

        // 5) 공격/사격 AI 끄기(원하면 계속 켜둘 수도 있음)
        if (alsoStopAttacks)
        {
            var attack1 = go.GetComponent<EnemyAttackController>();
            if (attack1) attack1.enabled = false;

            // 다른 커스텀 AI 스크립트도 여기서 비활성화 가능
            // var otherAI = go.GetComponent<SomeEnemyAI>(); if (otherAI) otherAI.enabled = false;
        }

        // 6) 아이들 흔들림/패트롤 등 기타 스크립트 비활성화 필요 시 여기에 추가
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawnTwo();
        }
    }

    void TrySpawnTwo()
    {
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

        if (spawned > 0) _nextOrbitPlusX = !_nextOrbitPlusX;
    }

    GameObject InstantiateEnemyInNumber(int number, bool orbitPlusX)
    {
        if (!enemy || !player) return null;

        int sectorIndex = NumberToSectorIndex(number);
        float centerDeg = sectorIndex * 45f + 22.5f;

        Vector3 center = usePlayerCenteredRing ? player.position : Vector3.zero;
        Vector3 dirXZ = AngleToDirXZ(centerDeg);
        Vector3 spawnPos = center + dirXZ * spawnRadius;

        bool isUpGroup = (number == 1 || number == 2 || number == 8 || number == 7);
        float pitch = isUpGroup ? altitudePitchDeg : -altitudePitchDeg;

        Quaternion look = Quaternion.LookRotation((player.position - spawnPos).normalized, Vector3.up) * Quaternion.Euler(pitch, 0f, 0f);

        var go = Instantiate(enemy, spawnPos, look);

        if (enemyIndicator) enemyIndicator.AddEnemy(go.transform);

        var e = go.GetComponent<Enemy>();
        if (e)
        {
            e.OnDeath += () => HandleEnemyDeath(number);
            e.SetSpawnerOccupyRelease(() => { _occupied[number] = Mathf.Max(0, _occupied[number] - 1); });
        }

        var move = go.GetComponent<EnemyMovement>();
        if (move)
        {
            move.player = player;
            move.initialOrbitPlusX = orbitPlusX;
            move.speed = 60f;
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, spawnRadius);

        if (spawnTestEnemyOnStart)
        {
            Gizmos.color = Color.yellow;
            Vector3 p0 = player.position;
            Vector3 p1 = p0 + player.forward * Mathf.Max(0.01f, testEnemyDistance);
            p1.y += testEnemyHeightOffset;
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawSphere(p1, 1.0f);
        }
    }
}
