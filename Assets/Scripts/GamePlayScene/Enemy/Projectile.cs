using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float speed = 30f;
    public float lifeTime = 5f;

    private Rigidbody rb;
    private Collider col;
    private ObjectPool<Projectile> pool;

    private float lifeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Init(ObjectPool<Projectile> poolRef)
    {
        pool = poolRef;
    }

    public void Launch(Vector3 position, Vector3 direction)
    {
        lifeTimer = 0f;

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (col) col.enabled = true;

        rb.isKinematic = false;
        rb.angularVelocity = Vector3.zero;

        rb.linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime) ReturnToPool();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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
    }

    private void ReturnToPool()
    {
        if (col) col.enabled = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (pool != null) pool.Return(this);
        else gameObject.SetActive(false);
    }
}
