using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    public GameObject enemy;
    public Transform player;
    public EnemyIndicator enemyIndicator;
    public MissionSuccessUI missionSuccessUI;

    [Header("Spawn Settings")]
    public float spawnInterval = 20f;       // 20초마다 시도
    public float spawnRadius = 300f;        // 플레이어 기준 반경
    public float altitudePitchDeg = 30f;    // 고도 ±30°
    public bool usePlayerCenteredRing = true;

    [Header("Debug")]
    public bool drawGizmos = true;

    // 구간 점유 (번호 1..8) -> 살아있는 적 수
    private readonly Dictionary<int, int> _occupied = new Dictionary<int, int>();
    // 스폰 순서 페어 (1,2)->(3,4)->(5,6)->(7,8)
    private readonly int[][] _pairs = new int[][]
    {
        new []{1,2}, new []{3,4}, new []{5,6}, new []{7,8}
    };
    private int _pairIndex = 0;

    // 원형 선회 방향: 한 사이클(+X), 다음 사이클(−X) 번갈이
    private bool _nextOrbitPlusX = true;

    private bool successShown = false;

    void Awake()
    {
        for (int n = 1; n <= 8; n++) _occupied[n] = 0;
    }

    void Start()
    {
        StartCoroutine(SpawnRoutine());
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
        // 모든 구간이 차 있으면 스킵
        bool anyFree = false;
        for (int n = 1; n <= 8; n++) if (_occupied[n] == 0) { anyFree = true; break; }
        if (!anyFree) return;

        int spawned = 0;
        int attempts = 0;

        // 현재 페어부터 시작하여 8개 구간을 최대 훑어보며 빈 자리에 1기씩, 최대 2기
        while (spawned < 2 && attempts < 8)
        {
            int[] pair = _pairs[_pairIndex];
            foreach (var num in pair)
            {
                if (spawned >= 2) break;
                if (_occupied[num] > 0) continue; // 차 있으면 패스

                // 스폰
                var go = InstantiateEnemyInNumber(num, spawned == 0 ? _nextOrbitPlusX : !_nextOrbitPlusX);
                if (go)
                {
                    _occupied[num]++;
                    spawned++;
                }
            }

            // 다음 페어로
            _pairIndex = (_pairIndex + 1) % _pairs.Length;
            attempts += 2; // 페어당 2구간
        }

        if (spawned > 0)
        {
            // 사이클 끝: 다음 사이클은 반대 선회부터
            _nextOrbitPlusX = !_nextOrbitPlusX;
        }
    }

    GameObject InstantiateEnemyInNumber(int number, bool orbitPlusX)
    {
        if (!enemy || !player) return null;

        // 번호→섹터 인덱스(0..7) 매핑
        // 0:0~45→1, 1:45~90→2, 2:90~135→8, 3:135~180→7, 4:180~225→4, 5:225~270→3, 6:270~315→5, 7:315~360→6
        int sectorIndex = NumberToSectorIndex(number);
        float centerDeg = sectorIndex * 45f + 22.5f;

        // 원 위 스폰 위치
        Vector3 center = usePlayerCenteredRing ? player.position : Vector3.zero;
        Vector3 dirXZ = AngleToDirXZ(centerDeg); // XZ 평면 단위
        Vector3 spawnPos = center + dirXZ * spawnRadius;

        // 고도(피치) 부여: +y 그룹(1,2,8,7)은 +pitch, -y 그룹(4,3,5,6)은 -pitch
        bool isUpGroup = (number == 1 || number == 2 || number == 8 || number == 7);
        float pitch = isUpGroup ? altitudePitchDeg : -altitudePitchDeg;

        Quaternion look = Quaternion.LookRotation((player.position - spawnPos).normalized, Vector3.up) * Quaternion.Euler(pitch, 0f, 0f);

        var go = Instantiate(enemy, spawnPos, look);

        if (enemyIndicator) enemyIndicator.AddEnemy(go.transform);

        // 적 세팅
        var e = go.GetComponent<Enemy>();
        if (e)
        {
            e.OnDeath += () => HandleEnemyDeath(number);
            e.SetSpawnerOccupyRelease(() => { _occupied[number] = Mathf.Max(0, _occupied[number] - 1); });
        }

        // 무브먼트 초기화: 속도 60, 초기 선회 방향(+X/−X)
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
        // UI 성공 패널(한 번만)
        if (!successShown)
        {
            successShown = true;
            missionSuccessUI?.Show();
        }

        // 점유 해제는 Enemy에서 SetSpawnerOccupyRelease 통해 호출됨(중복 안전)
    }

    // ---------- Helpers ----------

    static int NumberToSectorIndex(int number)
    {
        // number -> sectorIndex(0..7)
        // [1,2,8,7,4,3,5,6] ←→ [0,1,2,3,4,5,6,7]
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
        // 0°=+Z, 90°=+X 기준 → dir = (sin, 0, cos)
        return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || !player) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, spawnRadius);
    }
}
