using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [System.Serializable]
    public class Muzzle
    {
        public Transform muzzleTransform;
        public LineRenderer lineRenderer;
    }

    public enum FireMode { Hitscan, Projectile, HomingMissile }
    public FireMode currentFireMode = FireMode.Projectile;  

    public Muzzle[] muzzles;
    public CrosshairAutoTracker crosshairTracker;            

    public float projectileRange = 300f;              
    public float missileRange = 500f;                  
    public float hitscanRange = 450f;               

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
            projectilePool = new ObjectPool<PlayerProjectile>(playerProjectilePrefab, transform, 10, 50);

        if (playerMissilePrefab != null)
            missilePool = new ObjectPool<PlayerMissile>(playerMissilePrefab, transform, 5, 20);
    }

    public void ChangeFireMode()
    {
        currentFireMode = (FireMode)(((int)currentFireMode + 1) % 3);
        Debug.Log($"Fire mode changed to: {currentFireMode}");
    }

    public void FireButton()
    {
        if (Time.time < lastFireTime + fireDelay) return;
        Fire();
        lastFireTime = Time.time;
    }

    private void Fire()
    {
        switch (currentFireMode)
        {
            case FireMode.Hitscan: FireHitscan(); break;
            case FireMode.Projectile: FireProjectile(); break;
            case FireMode.HomingMissile: FireHomingMissile(); break;
        }
    }
    private void FireProjectile()
    {
        if (projectilePool == null) return;

        foreach (var m in muzzles)
        {
            if (m?.muzzleTransform == null) continue;

            Vector3 origin = m.muzzleTransform.position;
            Vector3 dir = m.muzzleTransform.forward;

            Transform target = crosshairTracker ? crosshairTracker.CurrentTarget : null;
            if (target && crosshairTracker.IsLocked && DistanceTo(target, origin) <= projectileRange)
            {
                dir = (GetTargetAimPoint(target) - origin).normalized;
            }

            PlayerProjectile p = projectilePool.Get();
            if (!p) continue;

            p.Init(projectilePool);
            p.damage = damage;
            p.speed = projectileSpeed;
            p.Launch(origin, dir);
        }
    }

    private void FireHitscan()
    {
        foreach (var m in muzzles)
        {
            if (m?.muzzleTransform == null) continue;

            Vector3 origin = m.muzzleTransform.position;
            Vector3 dir = m.muzzleTransform.forward;

            Transform target = crosshairTracker ? crosshairTracker.CurrentTarget : null;
            if (target && crosshairTracker.IsLocked && DistanceTo(target, origin) <= hitscanRange)
            {
                dir = (GetTargetAimPoint(target) - origin).normalized;
            }

            Vector3 hitPos = origin + dir * hitscanRange;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, hitscanRange))
            {
                hitPos = hit.point;
                var dmg = hit.collider.GetComponent<IDamagable>();
                if (dmg != null) dmg.OnDamage(damage, hit.point, hit.normal);
            }

            StartCoroutine(CoShotEffect(m, hitPos));
        }
    }

    private void FireHomingMissile()
    {
        if (missilePool == null) return;

        Transform target = (crosshairTracker && crosshairTracker.IsLocked) ? crosshairTracker.CurrentTarget : null;
        if (!target) return;

        Transform firstMuzzle = muzzles != null && muzzles.Length > 0 ? muzzles[0].muzzleTransform : transform;
        if (DistanceTo(target, firstMuzzle.position) > missileRange) return;

        foreach (var m in muzzles)
        {
            if (m?.muzzleTransform == null) continue;

            PlayerMissile missile = missilePool.Get();
            if (!missile) continue;

            missile.Init(missilePool);
            missile.damage = damage;
            missile.speed = missileSpeed;

            Vector3 origin = m.muzzleTransform.position;
            Vector3 dir = m.muzzleTransform.forward;

            missile.Launch(origin, dir, target);
        }
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
        yield break;
    }

    private Vector3 GetTargetAimPoint(Transform tr)
    {
        var aim = tr.Find("AimPoint");
        return aim ? aim.position : tr.position;
    }

    private float DistanceTo(Transform tr, Vector3 from) => Vector3.Distance(from, GetTargetAimPoint(tr));
}