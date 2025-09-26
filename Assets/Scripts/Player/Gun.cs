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
    public float missileRPM = 5f;

    public PlayerProjectile playerProjectilePrefab;
    public float projectileSpeed = 30f;

    public PlayerMissile playerMissilePrefab;
    public float missileSpeed = 20f;

    private float _nextBulletTime;       
    private float _nextMissileTime;       
    private float _missileInterval;      
    private bool _isFiringHeld;         

    private ObjectPool<PlayerProjectile> projectilePool;
    private ObjectPool<PlayerMissile> missilePool;

    void Start()
    {
        if (playerProjectilePrefab != null)
            projectilePool = new ObjectPool<PlayerProjectile>(playerProjectilePrefab, transform, 10, 50);

        if (playerMissilePrefab != null)
            missilePool = new ObjectPool<PlayerMissile>(playerMissilePrefab, transform, 5, 20);

        _missileInterval = (missileRPM > 0f) ? 60f / missileRPM : 9999f;

        ApplyModeVisuals();
        UpdateCrosshairRange();
    }

    void Update()
    {
        if (_isFiringHeld)
        {
            switch (currentFireMode)
            {
                case FireMode.Hitscan:
                case FireMode.Projectile:
                    if (Time.time >= _nextBulletTime)
                    {
                        FireNonHomingBurst();
                        _nextBulletTime = Time.time + fireDelay;
                    }
                    break;

                case FireMode.HomingMissile:
                    if (Time.time >= _nextMissileTime)
                    {
                        FireHomingBurstIfAny();
                        _nextMissileTime = Time.time + _missileInterval;
                    }
                    break;
            }
        }

        if (homingOverlay)
        {
            bool ready = Time.time >= _nextMissileTime - 0.0001f; // 약간의 여유
            homingOverlay.SetMissileReady(ready && currentFireMode == FireMode.HomingMissile);
        }
    }

    public void OnFireButtonDown()
    {
        _isFiringHeld = true;

        switch (currentFireMode)
        {
            case FireMode.Hitscan:
            case FireMode.Projectile:
                if (Time.time >= _nextBulletTime)
                {
                    FireNonHomingBurst();
                    _nextBulletTime = Time.time + fireDelay;
                }
                break;

            case FireMode.HomingMissile:
                if (Time.time >= _nextMissileTime)
                {
                    FireHomingBurstIfAny();
                    _nextMissileTime = Time.time + _missileInterval;
                }
                break;
        }
    }

    public void OnFireButtonUp()
    {
        _isFiringHeld = false;
    }

    public void ChangeFireMode()
    {
        currentFireMode = (FireMode)(((int)currentFireMode + 1) % 3);
        ApplyModeVisuals();
        UpdateCrosshairRange();
        Debug.Log($"Fire mode changed to: {currentFireMode}");
    }

    private void ApplyModeVisuals()
    {
        bool useSingle = currentFireMode == Gun.FireMode.Hitscan || currentFireMode == Gun.FireMode.Projectile;
        if (crosshairTracker) crosshairTracker.gameObject.SetActive(useSingle);

        bool useOverlay = currentFireMode == Gun.FireMode.HomingMissile;
        if (homingOverlay)
        {
            homingOverlay.gameObject.SetActive(useOverlay);
            if (!useOverlay) homingOverlay.ClearAll();  
            homingOverlay.SetMissileReady(false);       
        }

        _nextBulletTime = Time.time;
        _nextMissileTime = Time.time;
    }

    private void FireNonHomingBurst()
    {
        switch (currentFireMode)
        {
            case FireMode.Hitscan:
                FireHitscan();
                break;
            case FireMode.Projectile:
                FireProjectile();
                break;
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
    private void FireHomingBurstIfAny()
    {
        if (missilePool == null || homingOverlay == null) return;

        List<Transform> targets = homingOverlay.GetTargetsSortedByDistance();
        if (targets == null || targets.Count == 0) return;

        int muzzleCount = (muzzles != null) ? muzzles.Length : 0;
        if (muzzleCount <= 0) return;

        int fireCount = Mathf.Min(muzzleCount, targets.Count);

        for (int i = 0; i < fireCount; i++)
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

    private void UpdateCrosshairRange()
    {
        if (!crosshairTracker) return;

        switch (currentFireMode)
        {
            case FireMode.Hitscan:
                crosshairTracker.range = hitscanRange;
                break;
            case FireMode.Projectile:
                crosshairTracker.range = projectileRange;
                break;
            case FireMode.HomingMissile:
                break;
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

    private Vector3 GetAimPoint(Transform tr)
    {
        var aim = tr.Find("AimPoint");
        return aim ? aim.position : tr.position;
    }
    private float DistanceTo(Transform tr, Vector3 from) => Vector3.Distance(from, GetAimPoint(tr));
}