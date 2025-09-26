using UnityEngine;

public class PlayerMissile : MonoBehaviour
{
    public float damage = 25f;
    public float speed = 20f;
    public float lifeTime = 8f;
    public float homingStrength = 2f;

    private Transform launcher;
    private float maxTargetDistance = 500f;

    private Rigidbody rb;
    private Collider col;
    private ObjectPool<PlayerMissile> pool;
    private float lifeTimer;
    private Transform target;
    private Vector3 velocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Init(ObjectPool<PlayerMissile> poolRef)
    {
        pool = poolRef;
    }

    public void Launch(Vector3 position, Vector3 direction, Transform targetTransform, Transform launcherTransform, float maxDistance)
    {
        lifeTimer = 0f;
        target = targetTransform;

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (col) col.enabled = true;
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.angularVelocity = Vector3.zero;

        launcher = launcherTransform;
        maxTargetDistance = maxDistance;

        velocity = direction.normalized * speed;
        rb.linearVelocity = velocity;
    }

    private void FixedUpdate()
    {
        UpdateHoming();
        rb.linearVelocity = velocity;

        if (velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(velocity);
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime) { ReturnToPool(); return; }

        if (target == null) { ReturnToPool(); return; }

        if (launcher != null)
        {
            float d = Vector3.Distance(launcher.position, target.position);
            if (d > maxTargetDistance) { ReturnToPool(); return; }
        }
    }

    private void UpdateHoming()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        velocity = Vector3.Slerp(velocity.normalized, dir, homingStrength * Time.fixedDeltaTime) * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var dmg = other.GetComponent<IDamagable>();
            if (dmg != null)
            {
                Vector3 hitPoint = transform.position;
                Vector3 hitNormal = -transform.forward;
                dmg.OnDamage(damage, hitPoint, hitNormal);
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
        target = null;
        if (col) col.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (pool != null) pool.Return(this);
        else gameObject.SetActive(false);
    }
}
