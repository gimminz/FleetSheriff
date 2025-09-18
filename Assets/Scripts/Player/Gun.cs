using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [System.Serializable]
    public class Muzzle
    {
        public Transform muzzleTransform;
        public LineRenderer lineRenderer;
    }

    public enum FireMode
    {
        Hitscan,  
        Projectile, 
        HomingMissile
    }

    public FireMode currentFireMode = FireMode.Hitscan;

    public LockOnManager lockOnManager;
    public Muzzle[] muzzles;
    public float fireDistance = 50f;
    public float damage = 25f;
    public float fireDelay = 0.2f;
    private float lastFireTime;

    public PlayerProjectile playerProjectilePrefab;
    public float projectileSpeed = 30f;
    private ObjectPool<PlayerProjectile> projectilePool;

    public PlayerMissile playerMissilePrefab;
    public float missileSpeed = 20f;
    private ObjectPool<PlayerMissile> missilePool;

    private void Start()
    {
        if (playerProjectilePrefab != null)
        {
            projectilePool = new ObjectPool<PlayerProjectile>(playerProjectilePrefab, transform, 10, 50);
        }

        if (playerMissilePrefab != null)
        {
            missilePool = new ObjectPool<PlayerMissile>(playerMissilePrefab, transform, 5, 20);
        }
    }

    public void ChangeFireMode()
    {
        currentFireMode = (FireMode)(((int)currentFireMode + 1) % 3);
        Debug.Log($"Fire mode changed to: {currentFireMode}");
    }

    public void FireButton()
    {
        if (Time.time >= lastFireTime + fireDelay)
        {
            Fire();
            lastFireTime = Time.time;
        }
    }

    private void Fire()
    {
        switch (currentFireMode)
        {
            case FireMode.Hitscan:
                FireHitscan();
                break;
            case FireMode.Projectile:
                FireProjectile();
                break;
            case FireMode.HomingMissile:
                FireHomingMissile();
                break;
        }
    }

    private void FireHitscan()
    {
        foreach (var m in muzzles)
        {
            ShootFromMuzzle(m);
        }
    }

    private void FireProjectile()
    {
        if (projectilePool == null) return;

        foreach (var m in muzzles)
        {
            PlayerProjectile projectile = projectilePool.Get();
            if (projectile != null)
            {
                projectile.Init(projectilePool);
                projectile.damage = damage;
                projectile.speed = projectileSpeed;
                projectile.Launch(m.muzzleTransform.position, m.muzzleTransform.forward);
            }
        }
    }

    private void FireHomingMissile()
    {
        if (missilePool == null) return;

        List<Transform> lockedEnemies = lockOnManager?.GetLockedOnEnemies();

        if (lockedEnemies != null && lockedEnemies.Count > 0)
        {
            int missilesToFire = Mathf.Min(muzzles.Length, lockedEnemies.Count);

            for (int i = 0; i < missilesToFire; i++)
            {
                PlayerMissile missile = missilePool.Get();
                if (missile != null)
                {
                    missile.Init(missilePool);
                    missile.damage = damage;
                    missile.speed = missileSpeed;
                    missile.Launch(muzzles[i].muzzleTransform.position,
                                 muzzles[i].muzzleTransform.forward,
                                 lockedEnemies[i]);
                }
            }
        }
        else
        {
            foreach (var m in muzzles)
            {
                PlayerMissile missile = missilePool.Get();
                if (missile != null)
                {
                    missile.Init(missilePool);
                    missile.damage = damage;
                    missile.speed = missileSpeed;
                    missile.Launch(m.muzzleTransform.position, m.muzzleTransform.forward, null);
                }
            }
        }
    }

    private void ShootFromMuzzle(Muzzle m)
    {
        Vector3 hitPosition = m.muzzleTransform.position + m.muzzleTransform.forward * fireDistance;

        if (Physics.Raycast(m.muzzleTransform.position, m.muzzleTransform.forward, out RaycastHit hit, fireDistance))
        {
            hitPosition = hit.point;

            List<Transform> lockedEnemies = lockOnManager?.GetLockedOnEnemies();

            if (lockedEnemies != null && lockedEnemies.Count > 0)
            {
                Transform enemyRoot = hit.collider.transform;
                if (lockedEnemies.Contains(enemyRoot))
                {
                    var target = hit.collider.GetComponent<IDamagable>();
                    if (target != null)
                    {
                        target.OnDamage(damage, hit.point, hit.normal);
                    }
                }
            }
            else
            {
                var target = hit.collider.GetComponent<IDamagable>();
                if (target != null)
                {
                    target.OnDamage(damage, hit.point, hit.normal);
                }
            }
        }

        StartCoroutine(CoShotEffect(m, hitPosition));
    }

    private IEnumerator CoShotEffect(Muzzle m, Vector3 hitPosition)
    {
        if (m.lineRenderer != null)
        {
            m.lineRenderer.enabled = true;
            m.lineRenderer.positionCount = 2;
            m.lineRenderer.SetPosition(0, m.muzzleTransform.position);
            m.lineRenderer.SetPosition(1, hitPosition);
            yield return new WaitForSeconds(0.1f);
            m.lineRenderer.enabled = false;
        }
    }
}