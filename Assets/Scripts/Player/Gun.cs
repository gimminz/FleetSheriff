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
    private bool _wasFiringLastFrame;
    private bool _currentlyFiring;

    private ObjectPool<PlayerProjectile> projectilePool;
    private ObjectPool<PlayerMissile> missilePool;

    void Start()
    {
        if (playerProjectilePrefab != null)
            projectilePool = new ObjectPool<PlayerProjectile>(playerProjectilePrefab, transform, 10, 50);

        if (playerMissilePrefab != null)
            missilePool = new ObjectPool<PlayerMissile>(playerMissilePrefab, transform, 5, 20);

        _missileInterval = (missileRPM > 0f) ? 60f / missileRPM : 9999f;

        Debug.Log($"미사일 설정:");
        Debug.Log($"  미사일 RPM: {missileRPM}");
        Debug.Log($"  미사일 간격: {_missileInterval}초");
        Debug.Log($"  초기 _nextMissileTime: {_nextMissileTime}");

        ApplyModeVisuals();
        UpdateCrosshairRange();
    }

    void Update()
    {
        CheckFireInput();

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Tab key pressed!");
            ChangeFireMode();
        }
#endif

        bool canFireMissile = Time.time >= _nextMissileTime; // 미사일 쿨다운 체크

        if (_currentlyFiring && canFireMissile)
        {
            switch (currentFireMode)
            {
                case FireMode.Hitscan:
                    if (Time.time >= _nextBulletTime)
                    {
                        FireHitscan();
                        _nextBulletTime = Time.time + fireDelay;
                    }
                    break;

                case FireMode.Projectile:
                    if (Time.time >= _nextBulletTime)
                    {
                        FireProjectile();
                        _nextBulletTime = Time.time + fireDelay;
                    }
                    break;

                case FireMode.HomingMissile:
                    Debug.Log($"미사일 발사 시도 - 쿨다운 완료: {canFireMissile}");
                    FireHomingBurstIfAny();
                    _nextMissileTime = Time.time + _missileInterval;
                    Debug.Log($"다음 미사일 시간 설정: {_nextMissileTime:F3} (간격: {_missileInterval}초)");
                    break;
            }
        }

        // 🔧 수정: 유도탄 모드에서는 항상 락온 시스템 활성화
        if (homingOverlay && currentFireMode == FireMode.HomingMissile)
        {
            homingOverlay.SetMissileReady(true); // 항상 락온 가능하도록 설정

            // 추가 디버깅: 현재 상태 확인
            if (_currentlyFiring && !canFireMissile)
            {
                float waitTime = _nextMissileTime - Time.time;
                Debug.Log($"미사일 쿨다운 대기 중: {waitTime:F2}초 남음");
            }
        }
    }

    private void CheckFireInput()
    {
        bool firePressed = _isFiringHeld;

        if (firePressed && !_wasFiringLastFrame)
        {
            Debug.Log("Fire Started");
        }
        else if (!firePressed && _wasFiringLastFrame)
        {
            Debug.Log("Fire Stopped");
        }

        _wasFiringLastFrame = firePressed;
        _currentlyFiring = firePressed;
    }

    public void OnFireButtonDown()
    {
        _isFiringHeld = true;
        Debug.Log("Button Fire Started");
    }

    public void OnFireButtonUp()
    {
        _isFiringHeld = false;
        Debug.Log("Button Fire Stopped");
    }

    public void ChangeFireMode()
    {
        currentFireMode = (FireMode)(((int)currentFireMode + 1) % 3);
        Debug.Log($"ChangeFireMode called! New mode: {currentFireMode}");
        ApplyModeVisuals();
        UpdateCrosshairRange();
    }

    private void ApplyModeVisuals()
    {
        bool useSingle = currentFireMode == Gun.FireMode.Hitscan || currentFireMode == Gun.FireMode.Projectile;
        bool useOverlay = currentFireMode == Gun.FireMode.HomingMissile;

        if (crosshairTracker)
        {
            crosshairTracker.gameObject.SetActive(useSingle);

            // Projectile이나 Hitscan 모드로 변경 시 crosshair를 중앙으로 리셋하고 lock 해제
            if (useSingle)
            {
                crosshairTracker.ResetToCenter();
                crosshairTracker.ReleaseLock();
            }

            // 같은 crosshair 오브젝트를 공유하는 경우를 위한 추가 제어
            if (crosshairTracker.crosshair)
            {
                crosshairTracker.crosshair.gameObject.SetActive(useSingle);
            }
        }

        if (homingOverlay)
        {
            homingOverlay.gameObject.SetActive(useOverlay);
            if (!useOverlay) homingOverlay.ClearAll();
            homingOverlay.SetMissileReady(false);

            // 유도탄 모드로 변경 시 missile range 동기화
            if (useOverlay)
            {
                homingOverlay.SetMissileRange(missileRange);
            }
        }

        Debug.Log($"모드 변경 시 시간 초기화:");
        Debug.Log($"  이전 _nextMissileTime: {_nextMissileTime:F3}");

        _nextBulletTime = Time.time;
        _nextMissileTime = Time.time;

        Debug.Log($"  새로운 _nextMissileTime: {_nextMissileTime:F3}");
        Debug.Log($"  현재 Time.time: {Time.time:F3}");
    }

    private void FireHitscan()
    {
        Debug.Log("FireHitscan called!");

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
                Debug.Log($"Hit {hit.collider.name}!");
            }
            else
            {
                Debug.Log("No hit detected");
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
        Debug.Log("=== FireHomingBurstIfAny 시작 ===");
        Debug.Log($"현재 시간: {Time.time}, 다음 미사일 시간: {_nextMissileTime}");
        Debug.Log($"미사일 준비됨: {Time.time >= _nextMissileTime}");

        if (missilePool == null)
        {
            Debug.LogError("❌ missilePool이 null입니다!");
            return;
        }
        Debug.Log("✅ missilePool 확인됨");

        if (homingOverlay == null)
        {
            Debug.LogError("❌ homingOverlay가 null입니다!");
            return;
        }
        Debug.Log("✅ homingOverlay 확인됨");

        Debug.Log($"HomingOverlay 활성화 상태: {homingOverlay.gameObject.activeInHierarchy}");
        Debug.Log($"HomingOverlay 컴포넌트 활성화: {homingOverlay.enabled}");

        List<Transform> targets = homingOverlay.GetTargetsSortedByDistance();

        if (targets == null)
        {
            Debug.LogError("❌ GetTargetsSortedByDistance()가 null을 반환했습니다!");
            return;
        }

        Debug.Log($"✅ 타겟 리스트 받음. 개수: {targets.Count}");

        if (targets.Count == 0)
        {
            Debug.LogWarning("⚠️ 타겟이 없습니다. HomingOverlay 상태를 확인하세요.");
            // HomingOverlay 내부 상태 디버깅을 위한 추가 정보 요청
            Debug.Log("HomingOverlay 디버그 정보 요청...");
            return;
        }

        // 각 타겟 정보 출력
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                float distance = Vector3.Distance(transform.position, targets[i].position);
                Debug.Log($"타겟 {i}: {targets[i].name}, 거리: {distance:F1}m");
            }
            else
            {
                Debug.LogWarning($"타겟 {i}이 null입니다!");
            }
        }

        int muzzleCount = (muzzles != null) ? muzzles.Length : 0;
        Debug.Log($"사용 가능한 머즐 수: {muzzleCount}");

        if (muzzleCount <= 0)
        {
            Debug.LogError("❌ 머즐이 없습니다!");
            return;
        }

        // 각 머즐 상태 확인
        for (int i = 0; i < muzzles.Length; i++)
        {
            if (muzzles[i] == null)
                Debug.LogWarning($"머즐 {i}이 null입니다!");
            else if (muzzles[i].muzzleTransform == null)
                Debug.LogWarning($"머즐 {i}의 Transform이 null입니다!");
            else
                Debug.Log($"✅ 머즐 {i} 정상: {muzzles[i].muzzleTransform.name}");
        }

        int fireCount = Mathf.Min(muzzleCount, targets.Count);
        Debug.Log($"🚀 {fireCount}개의 미사일을 발사합니다!");

        int actualFired = 0;
        for (int i = 0; i < fireCount; i++)
        {
            var m = muzzles[i];
            if (m?.muzzleTransform == null)
            {
                Debug.LogError($"❌ 머즐 {i} 사용 불가");
                continue;
            }

            var missile = missilePool.Get();
            if (!missile)
            {
                Debug.LogError($"❌ 미사일 풀에서 미사일을 가져올 수 없습니다 (머즐 {i})");
                continue;
            }

            Debug.Log($"✅ 미사일 {i} 풀에서 획득: {missile.name}");

            missile.Init(missilePool);
            missile.damage = damage;
            missile.speed = missileSpeed;

            Vector3 origin = m.muzzleTransform.position;
            Vector3 dir = m.muzzleTransform.forward;
            Transform target = targets[i];

            Debug.Log($"🎯 미사일 {i} 발사:");
            Debug.Log($"  - 발사 위치: {origin}");
            Debug.Log($"  - 발사 방향: {dir}");
            Debug.Log($"  - 타겟: {target.name} at {target.position}");
            Debug.Log($"  - 미사일 범위: {missileRange}");

            missile.Launch(origin, dir, target, transform, missileRange);
            actualFired++;

            Debug.Log($"🚀 미사일 {i} 발사 완료!");
        }

        Debug.Log($"=== 총 {actualFired}개 미사일 발사 완료 ===");
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

    private IEnumerator CoShotEffect(Muzzle m, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (m.lineRenderer != null)
        {
            m.lineRenderer.enabled = false;
        }
    }

    private Vector3 GetAimPoint(Transform tr)
    {
        var aim = tr.Find("AimPoint");
        return aim ? aim.position : tr.position;
    }

    private float DistanceTo(Transform tr, Vector3 from) => Vector3.Distance(from, GetAimPoint(tr));
}