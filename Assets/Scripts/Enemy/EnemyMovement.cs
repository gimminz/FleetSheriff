using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    public float speed = 60f;

    // Orbit 세팅
    public float orbitRadius = 50f;
    public float orbitAngularSpeedDeg = 90f; // 초당 90도
    public float orbitEnterDistance = 60f;   // Orbit 진입 임계

    public float rearChaseDuration = 2.0f;   // 뒤쪽 직진 유지 시간
    public float despawnDistance = 800f;

    // 선회 방향(+X/−X) 초기값 — 스폰 시 지정
    public bool initialOrbitPlusX = true;

    // 내부 상태
    private enum State { Homing, Orbit, RearChase, Flee }
    private State _state = State.Homing;
    private bool _orbitPlusXThisEnemy;   // 이 적의 현재 Orbit 방향(+X/−X)
    private float _stateTimer = 0f;

    public bool IsFleeing => _state == State.Flee;

    void OnEnable()
    {
        _state = State.Homing;
        _stateTimer = 0f;
        _orbitPlusXThisEnemy = initialOrbitPlusX;
    }

    void Update()
    {
        if (!player) return;

        _stateTimer += Time.deltaTime;

        // 멀어지면 소멸
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist >= despawnDistance)
        {
            Destroy(gameObject);
            return;
        }

        switch (_state)
        {
            case State.Homing:
                TickHoming();
                if (dist <= orbitEnterDistance)
                    EnterOrbit();
                break;

            case State.Orbit:
                TickOrbit();
                // 플레이어 뒤쪽 영역에 들어오면 RearChase
                if (IsBehindPlayer())
                    EnterRearChase();
                break;

            case State.RearChase:
                TickRearChase();
                if (_stateTimer >= rearChaseDuration)
                    EnterHoming(); // 재교전
                break;

            case State.Flee:
                TickFlee();
                break;
        }
    }

    // ----- State ticks -----

    void TickHoming()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        MoveTowards(dir);
    }

    void TickOrbit()
    {
        // 플레이어 기준 반지름 방향
        Vector3 r = transform.position - player.position;
        r.y = 0f;
        if (r.sqrMagnitude < 1e-4f) r = transform.right; // 안전장치
        r.Normalize();

        // 접선: cross(Up, r) 또는 그 반대
        Vector3 t = Vector3.Cross(Vector3.up, r);
        // 이번 Orbit이 +X여야 하면 t.x>0인 접선 선택, 아니면 t.x<0 선택
        if (_orbitPlusXThisEnemy)
        {
            if (t.x < 0f) t = -t;
        }
        else
        {
            if (t.x > 0f) t = -t;
        }

        // 원주 방향으로 이동 + 반지름 유지 보정
        Vector3 targetPos = player.position + r * orbitRadius;
        Vector3 toRing = (targetPos - transform.position);
        Vector3 tangentMove = t * (orbitAngularSpeedDeg * Mathf.Deg2Rad * orbitRadius); // 각속도*반지름 ≈ 선속도
        Vector3 desiredVel = tangentMove + toRing; // 원으로 붙는 보정

        MoveTowards(desiredVel.normalized);
    }

    void TickRearChase()
    {
        // 플레이어 뒤 방향(플레이어 forward 반대)으로 직진
        Vector3 dir = -player.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-4f) dir = -transform.forward;
        MoveTowards(dir.normalized);
    }

    void TickFlee()
    {
        // 플레이어로부터 멀어지는 방향
        Vector3 dir = (transform.position - player.position).normalized;
        MoveTowards(dir);
    }

    // ----- State transitions -----

    void EnterHoming()
    {
        _state = State.Homing;
        _stateTimer = 0f;
    }

    void EnterOrbit()
    {
        _state = State.Orbit;
        _stateTimer = 0f;

        // Orbit 진입 시마다 개체 토글(+X↔−X)
        _orbitPlusXThisEnemy = !_orbitPlusXThisEnemy;
    }

    void EnterRearChase()
    {
        _state = State.RearChase;
        _stateTimer = 0f;
    }

    public void EnterFlee()
    {
        _state = State.Flee;
        _stateTimer = 0f;
    }

    public void RecoverAfterBounce()
    {
        // 지형 반사 후 즉시 Homing으로 복귀해 플레이어를 향해 진행
        EnterHoming();
    }

    // ----- Helpers -----

    void MoveTowards(Vector3 dir)
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir, Vector3.up),
            Time.deltaTime * 5f
        );
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    bool IsBehindPlayer()
    {
        // 플레이어 기준으로 적이 뒤쪽에 있으면 true
        Vector3 toEnemy = transform.position - player.position;
        toEnemy.y = 0f;
        toEnemy.Normalize();
        float d = Vector3.Dot(player.forward, toEnemy);
        return d < -0.5f; // 임계: 충분히 뒤쪽
    }
}
