using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [Header("Spec")]
    public float damage = 25f;
    public float speed = 0.08f;
    public float lifeTime = 5f;

    [Header("Trail (옵션)")]
    [Tooltip("프로젝타일 이동 궤적을 그릴 TrailRenderer (자식에 두는 것을 권장).")]
    [SerializeField] private TrailRenderer trail;
    [Tooltip("발사 시 Trail.Clear() 수행 (권장)")]
    [SerializeField] private bool clearTrailOnLaunch = true;
    [Tooltip("풀 복귀 직전에 Trail를 꺼서(Emitting=false) 잔상 생성 방지")]
    [SerializeField] private bool stopTrailOnReturn = true;
    [Tooltip("Trail을 월드 좌표로 그릴지 여부 (빠른 탄은 보통 true가 보기 좋음)")]
    [SerializeField] private bool trailWorldSpace = true;

    private Rigidbody rb;
    private Collider col;
    private ObjectPool<PlayerProjectile> pool;
    private float lifeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // 자식에 TrailRenderer가 있다면 자동 캐치
        if (!trail) trail = GetComponentInChildren<TrailRenderer>(true);

        if (trail)
        {
            trail.autodestruct = false;     // 풀링이므로 자동 파괴 X
            trail.emitting = false;      // 시작 전엔 끔
            trail.alignment = LineAlignment.View;
            trail.Clear();
        }
    }

    public void Init(ObjectPool<PlayerProjectile> poolRef)
    {
        pool = poolRef;
    }

    /// <summary>
    /// 풀에서 꺼내 발사할 때 호출
    /// </summary>
    public void Launch(Vector3 position, Vector3 direction)
    {
        lifeTimer = 0f;

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (col) col.enabled = true;

        rb.isKinematic = false;
        rb.angularVelocity = Vector3.zero;
        // 프로젝트 전반에서 쓰던 확장 프로퍼티를 유지합니다.
        rb.linearVelocity = direction.normalized * speed;

        // ── Trail on ─────────────────────────────────────────
        if (trail)
        {
            if (clearTrailOnLaunch) trail.Clear();
            trail.emitting = true;
            trail.alignment = LineAlignment.View; // 계속 카메라 정렬 유지
        }
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var damagable = other.GetComponent<IDamagable>();
            if (damagable != null)
            {
                Vector3 hitPoint = transform.position;
                Vector3 hitNormal = -transform.forward;
                damagable.OnDamage(damage, hitPoint, hitNormal);
            }
            ReturnToPool();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.CompareTag("Obstacle"))
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        // ── Trail off ────────────────────────────────────────
        if (trail)
        {
            if (stopTrailOnReturn) trail.emitting = false;
            trail.Clear();
        }

        if (col) col.enabled = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (pool != null)
            pool.Return(this);
        else
            gameObject.SetActive(false);
    }
}
