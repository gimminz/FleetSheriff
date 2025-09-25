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

    public enum FireMode { Hitscan, Projectile, HomingMissile }

    public FireMode currentFireMode = FireMode.Projectile;  

    public Muzzle[] muzzles;
    public CrosshairAutoTracker crosshairTracker;            
    public HomingTargetOverlay homingOverlay;                

    public float projectileRange = 300f;                     
    public float missileRange = 500f;                       
    public float hitscanRange = 450f;                     

    public float damage = 25f;
    public float fireDelay = 0.2f;                           
    private float _lastFireTime;

    [Tooltip("분당 발사 속도(RPM). 예: 5 → 12초 간격")]
    public float missileRPM = 5f;                           
    private float _missileInterval;                          
    private bool _isHomingHeld;                           
    private float _lastMissileTime;

    public PlayerProjectile playerProjectilePrefab;
    public float projectileSpeed = 30f;
    private ObjectPool<PlayerProjectile> projectilePool;

    public PlayerMissile playerMissilePrefab;
    public float missileSpeed = 20f;
    private ObjectPool<PlayerMissile> missilePool;

    void Start()
    {
        if (playerProjectilePrefab != null)
            projectilePool = new ObjectPool<PlayerProjectile>(playerProjectilePrefab, transform, 10, 50);

        if (playerMissilePrefab != null)
            missilePool = new ObjectPool<PlayerMissile>(playerMissilePrefab, transform, 5, 20);

        _missileInterval = (missileRPM > 0f) ? 60f / missileRPM : 9999f;
    }

    void Update()
    {
        if (currentFireMode == FireMode.HomingMissile && _isHomingHeld)
        {
            if (Time.time >= _lastMissileTime + _missileInterval)
            {
                FireHomingMissile();
                _lastMissileTime = Time.time;
            }
        }
    }

    public void ChangeFireMode()
    {
        currentFireMode = (FireMode)(((int)currentFireMode + 1) % 3);
        Debug.Log($"Fire mode changed to: {currentFireMode}");
    }

    public void FireButton()
    {
        if (currentFireMode == FireMode.HomingMissile)
        {
            if (Time.time >= _lastMissileTime + _missileInterval)
            {
                FireHomingMissile();
                _lastMissileTime = Time.time;
            }
            return;
        }

        if (Time.time < _lastFireTime + fireDelay) return;
        Fire();
        _lastFireTime = Time.time;
    }

    public void SetHomingHeld(bool held)
    {
        _isHomingHeld = held;
        if (held && Time.time >= _lastMissileTime + _missileInterval)
        {
            FireHomingMissile();
            _lastMissileTime = Time.time;
        }
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

    private void FireHitscan()
    {
        foreach (var m in muzzles)
        {
            if (m?.muzzleTransform == null) continue;

            Vector3 origin = m.muzzleTransform.position;
            Vector3 dir = m.muzzleTransform.forward;

            Transform target = crosshairTracker ? crosshairTracker.CurrentTarget : null;
            if (target && crosshairTracker.IsLocked && DistanceTo(target, origin) <= hitscanRange)
                dir = (GetAimPoint(target) - origin).normalized;

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
                dir = (GetAimPoint(target) - origin).normalized;

            var p = projectilePool.Get();
            if (!p) continue;

            p.Init(projectilePool);
            p.damage = damage;
            p.speed = projectileSpeed;
            p.Launch(origin, dir); 
        }
    }

    private void FireHomingMissile()
    {
        if (missilePool == null || homingOverlay == null) return;

        List<Transform> targets = homingOverlay.GetTargetsSortedByDistance();
        if (targets == null || targets.Count == 0) return;

        int count = Mathf.Min(muzzles != null ? muzzles.Length : 0, targets.Count);
        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            var m = muzzles[i];
            if (m?.muzzleTransform == null) continue;

            var missile = missilePool.Get();
            if (!missile) continue;

            missile.Init(missilePool);
            missile.damage = damage;
            missile.speed = missileSpeed;

            Vector3 origin = m.muzzleTransform.position;
            Vector3 dir = m.muzzleTransform.forward;

            Transform target = targets[i];
            missile.Launch(origin, dir, target, transform, missileRange);
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

    public void OnFireButtonDown()
{
    if (currentFireMode == FireMode.HomingMissile)
    {
        SetHomingHeld(true);
    }
    else
    {
        FireButton();
    }
}

public void OnFireButtonUp()
{
    if (currentFireMode == FireMode.HomingMissile)
    {
        SetHomingHeld(false);
    }
}

    private Vector3 GetAimPoint(Transform tr)
    {
        var aim = tr.Find("AimPoint");
        return aim ? aim.position : tr.position;
    }
    private float DistanceTo(Transform tr, Vector3 from) => Vector3.Distance(from, GetAimPoint(tr));
}
