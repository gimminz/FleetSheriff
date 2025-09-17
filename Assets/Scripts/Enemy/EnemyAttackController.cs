using UnityEngine;

public class EnemyAttackController : MonoBehaviour
{
    public Transform player;
    public Transform muzzle;
    public Projectile projectilePrefab;
    public Transform projectilePoolParent;

    public float attackRange = 80f;
    public float fireCooldown = 1.0f;
    private float lastFireTime = -999f;

    private ObjectPool<Projectile> projectilePool;
    public int initPoolSize = 10;
    public int maxPoolSize = 50;

    private void Awake()
    {
        projectilePool = new ObjectPool<Projectile>(
            projectilePrefab,
            projectilePoolParent,
            initPoolSize,
            maxPoolSize
        );
    }

    private void Update()
    {
        if (!player || !muzzle) return;

        float dist = Vector3.Distance(muzzle.position, player.position);
        if (dist > attackRange) return;

        if (Time.time < lastFireTime + fireCooldown) return;

        Fire();
        lastFireTime = Time.time;
    }

    private void Fire()
    {
        Projectile proj = projectilePool.Get();
        if (!proj) return;

        proj.Init(projectilePool);
        Vector3 dir = (player.position - muzzle.position).normalized;
        proj.Launch(muzzle.position, dir);
    }
}
