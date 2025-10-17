using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

    [Header("UI")]
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI ammoText;           // 🔹 현재 모드 잔탄 표시

    [Header("Weapon Names")]
    public string hitscanName = "레이저";
    public string projectileName = "투사체";
    public string homingMissileName = "유도탄";

    [Header("기본 탄수(선택 무장 없을 때 사용)")]
    public int defaultHitscanAmmo = 200;
    public int defaultProjectileAmmo = 120;
    public int defaultMissileAmmo = 12;

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
    private bool _currentlyFiring;

    private ObjectPool<PlayerProjectile> projectilePool;
    private ObjectPool<PlayerMissile> missilePool;

    // 🔹 무장/탄수 상태
    int? _hitscanWeaponId, _projectileWeaponId, _missileWeaponId;
    int? _hitscanMax, _projectileMax, _missileMax; // null이면 "null" 표기+무한
    int _hitscanCur, _projectileCur, _missileCur;

    void Start()
    {
        if (playerProjectilePrefab != null)
            projectilePool = new ObjectPool<PlayerProjectile>(playerProjectilePrefab, transform, 10, 50);

        if (playerMissilePrefab != null)
            missilePool = new ObjectPool<PlayerMissile>(playerMissilePrefab, transform, 5, 20);

        _missileInterval = (missileRPM > 0f) ? 60f / missileRPM : 9999f;

        InitializeAmmoFromLoadout();

        ApplyModeVisuals();
        UpdateCrosshairRange();
        UpdateWeaponNameUI();
        UpdateAmmoUI();
    }


    void Update()
    {
        CheckFireInput();

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ChangeFireMode();
        }
#endif

        bool canFireMissile = Time.time >= _nextMissileTime;

        if (_currentlyFiring)
        {
            switch (currentFireMode)
            {
                case FireMode.Hitscan:
                    if (Time.time >= _nextBulletTime && TryConsumeAmmo(FireMode.Hitscan, 1))
                    {
                        int fired = FireHitscan();
                        // 히트스캔은 1회 트리거당 1발로 간주(멀티 머즐도 1 소모로 단순화하려면 fired 대신 1 사용)
                        UpdateAmmoUI();
                        _nextBulletTime = Time.time + fireDelay;
                    }
                    break;

                case FireMode.Projectile:
                    if (Time.time >= _nextBulletTime && TryConsumeAmmo(FireMode.Projectile, 1))
                    {
                        int fired = FireProjectile();
                        UpdateAmmoUI();
                        _nextBulletTime = Time.time + fireDelay;
                    }
                    break;

                case FireMode.HomingMissile:
                    if (canFireMissile)
                    {
                        // 남은 탄수만큼만 발사 제한
                        int budget = GetRemaining(FireMode.HomingMissile);
                        if (!IsInfinite(FireMode.HomingMissile) && budget <= 0)
                        {
                            UpdateAmmoUI();
                            break;
                        }

                        int fired = FireHomingBurstIfAny(budget);
                        if (fired > 0 && !IsInfinite(FireMode.HomingMissile))
                            _missileCur = Mathf.Max(0, _missileCur - fired);

                        UpdateAmmoUI();
                        _nextMissileTime = Time.time + _missileInterval;
                    }
                    break;
            }
        }

        if (homingOverlay && currentFireMode == FireMode.HomingMissile)
        {
            // 잔탄 없으면 준비 False
            bool ready = IsInfinite(FireMode.HomingMissile) || GetRemaining(FireMode.HomingMissile) > 0;
            homingOverlay.SetMissileReady(ready);
        }
    }

    private void CheckFireInput()
    {
        bool firePressed = _isFiringHeld;
        _currentlyFiring = firePressed;
    }

    public void OnFireButtonDown() => _isFiringHeld = true;
    public void OnFireButtonUp() => _isFiringHeld = false;

    public void ChangeFireMode()
    {
        currentFireMode = (FireMode)(((int)currentFireMode + 1) % 3);
        ApplyModeVisuals();
        UpdateCrosshairRange();
        UpdateWeaponNameUI();
        UpdateAmmoUI();
    }

    // ========================= 발사 구현 =========================

    private int FireHitscan()
    {
        int shots = 0;
        foreach (var m in muzzles)
        {
            if (m?.muzzleTransform == null) continue;
            shots++;

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

            if (m.lineRenderer != null)
            {
                m.lineRenderer.enabled = true;
                m.lineRenderer.positionCount = 2;
                m.lineRenderer.SetPosition(0, origin);
                m.lineRenderer.SetPosition(1, hitPos);
                StartCoroutine(CoShotEffect(m, 0.05f));
            }
        }

        if (!IsInfinite(FireMode.Hitscan))
            _hitscanCur = Mathf.Max(0, _hitscanCur - 1); // 1회 트리거 = 1 소모(원하면 shots로 바꿔도 됨)

        return shots;
    }

    private int FireProjectile()
    {
        if (projectilePool == null) return 0;

        int shots = 0;
        foreach (var m in muzzles)
        {
            if (m?.muzzleTransform == null) continue;
            shots++;

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

        if (!IsInfinite(FireMode.Projectile))
            _projectileCur = Mathf.Max(0, _projectileCur - 1); // 1회 트리거 = 1 소모(멀티샷이면 shots로)

        return shots;
    }

    private int FireHomingBurstIfAny(int budget /* 잔탄 허용량 */ = int.MaxValue)
    {
        if (missilePool == null || homingOverlay == null) return 0;

        List<Transform> targets = homingOverlay.GetTargetsSortedByDistance();
        int muzzleCount = (muzzles != null) ? muzzles.Length : 0;
        int fireCount = Mathf.Min(muzzleCount, targets.Count);

        if (!IsInfinite(FireMode.HomingMissile))
            fireCount = Mathf.Min(fireCount, Mathf.Max(0, budget));

        int actualFired = 0;

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
            actualFired++;
        }

        return actualFired;
    }

    private void UpdateCrosshairRange()
    {
        if (!crosshairTracker) return;

        switch (currentFireMode)
        {
            case FireMode.Hitscan: crosshairTracker.range = hitscanRange; break;
            case FireMode.Projectile: crosshairTracker.range = projectileRange; break;
            case FireMode.HomingMissile: /* overlay가 처리 */                    break;
        }
    }

    private IEnumerator CoShotEffect(Muzzle m, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (m.lineRenderer != null) m.lineRenderer.enabled = false;
    }

    private Vector3 GetAimPoint(Transform tr)
    {
        var aim = tr.Find("AimPoint");
        return aim ? aim.position : tr.position;
    }

    private float DistanceTo(Transform tr, Vector3 from) => Vector3.Distance(from, GetAimPoint(tr));

    // ========================= 잔탄: 초기화/소모/표시 =========================

    void InitializeAmmoFromLoadout()
    {
        // 1) 선택 무장 분류 (모드별 대표 무장 1개씩만 사용)
        if (LaunchContext.SelectedWeapons != null && LaunchContext.SelectedWeapons.Count > 0)
        {
            foreach (var kv in LaunchContext.SelectedWeapons)
            {
                if (!kv.Value.HasValue) continue;
                var w = DataTableManager.WeaponTable?.Get(kv.Value.Value);
                if (w == null) continue;

                var kind = GuessKind(w);

                if (kind == FireMode.HomingMissile && _missileWeaponId == null) _missileWeaponId = w.Id;
                else if (kind == FireMode.Hitscan && _hitscanWeaponId == null) _hitscanWeaponId = w.Id;
                else if (kind == FireMode.Projectile && _projectileWeaponId == null) _projectileWeaponId = w.Id;
            }
        }

        ApplyCapacityForMode(FireMode.Hitscan, _hitscanWeaponId, defaultHitscanAmmo, out _hitscanMax, out _hitscanCur);
        ApplyCapacityForMode(FireMode.Projectile, _projectileWeaponId, defaultProjectileAmmo, out _projectileMax, out _projectileCur);
        ApplyCapacityForMode(FireMode.HomingMissile, _missileWeaponId, defaultMissileAmmo, out _missileMax, out _missileCur);

    }

    void ApplyCapacityForMode(FireMode mode, int? weaponId, int fallback, out int? maxCap, out int cur)
    {
        if (weaponId.HasValue)
        {
            var w = DataTableManager.WeaponTable?.Get(weaponId.Value);
            if (w != null)
            {
                int? cap = (mode == FireMode.Hitscan) ? w.NumberOfUses : w.AmmoCapacity;

                if (cap.HasValue)
                {
                    maxCap = Mathf.Max(0, cap.Value);
                    cur = maxCap.Value; 
                    return;
                }
                else
                {
                    maxCap = null;     
                    cur = int.MaxValue; 
                    return;
                }
            }
        }

        maxCap = Mathf.Max(0, fallback);
        cur = maxCap.Value;
    }


    string Fmt(int cur, int? max) => max.HasValue ? $"{cur}/{max.Value}" : "null";

    bool IsInfinite(FireMode mode)
    {
        return mode switch
        {
            FireMode.Hitscan => !_hitscanMax.HasValue,
            FireMode.Projectile => !_projectileMax.HasValue,
            FireMode.HomingMissile => !_missileMax.HasValue,
            _ => false
        };
    }

    int GetRemaining(FireMode mode)
    {
        return mode switch
        {
            FireMode.Hitscan => _hitscanCur,
            FireMode.Projectile => _projectileCur,
            FireMode.HomingMissile => _missileCur,
            _ => 0
        };
    }

    bool TryConsumeAmmo(FireMode mode, int amount)
    {
        if (amount <= 0) return true;
        if (IsInfinite(mode)) return true; 

        int remain = GetRemaining(mode);
        if (remain < amount) return false;

        switch (mode)
        {
            case FireMode.Hitscan: _hitscanCur = remain - amount; break;    
            case FireMode.Projectile: _projectileCur = remain - amount; break;
            case FireMode.HomingMissile: _missileCur = remain - amount; break;
        }
        return true;
    }

    void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        string label = currentFireMode == FireMode.Hitscan
            ? "남은 사용 횟수: "
            : "잔탄수: ";

        string value = currentFireMode switch
        {
            FireMode.Hitscan => _hitscanMax.HasValue ? _hitscanCur.ToString() : "null",
            FireMode.Projectile => _projectileMax.HasValue ? _projectileCur.ToString() : "null",
            FireMode.HomingMissile => _missileMax.HasValue ? _missileCur.ToString() : "null",
            _ => "-"
        };

        ammoText.text = label + value;
    }


    FireMode GuessKind(WeaponData w)
    {
        string s = ((w.WeaponKey ?? "") + " " + (w.DisplayName ?? "")).ToLowerInvariant();

        if (s.Contains("missile") || s.Contains("missilepod") || s.Contains("유도탄"))
            return FireMode.HomingMissile;

        if (s.Contains("laser") || s.Contains("레이저"))
            return FireMode.Hitscan;

        if (s.Contains("grenade") || s.Contains("rocket") || s.Contains("유탄") || s.Contains("로켓"))
            return FireMode.Projectile;

        return FireMode.Projectile;
    }

    private void ApplyModeVisuals()
    {
        bool useSingle = currentFireMode == FireMode.Hitscan || currentFireMode == FireMode.Projectile;
        bool useOverlay = currentFireMode == FireMode.HomingMissile;

        if (crosshairTracker)
        {
            crosshairTracker.gameObject.SetActive(useSingle);

            if (useSingle)
            {
                crosshairTracker.ResetToCenter();
                crosshairTracker.ReleaseLock();
            }

            if (crosshairTracker.crosshair)
                crosshairTracker.crosshair.gameObject.SetActive(useSingle);
        }

        if (homingOverlay)
        {
            homingOverlay.gameObject.SetActive(useOverlay);
            if (!useOverlay) homingOverlay.ClearAll();
            homingOverlay.SetMissileReady(false);

            if (useOverlay)
                homingOverlay.SetMissileRange(missileRange);
        }

        _nextBulletTime = Time.time;
        _nextMissileTime = Time.time;
    }

    private void UpdateWeaponNameUI()
    {
        if (weaponNameText == null) return;

        string weaponName = currentFireMode switch
        {
            FireMode.Hitscan => hitscanName,
            FireMode.Projectile => projectileName,
            FireMode.HomingMissile => homingMissileName,
            _ => "UNKNOWN"
        };

        weaponNameText.text = weaponName;
    }
}
