using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomingTargetOverlay : MonoBehaviour
{
    public Transform player;
    public Camera playerCamera;
    public Canvas canvas;
    public RectTransform parentRect;
    public RectTransform overlayRoot;
    public RectTransform scopeRect;
    public RadarRegistry registry;
    public RectTransform crosshairPrefab;

    public float missileRange = 500f;
    public int initialPool = 16;
    public int maxPool = 64;

    [Header("Lock-on Settings")]
    public float lockOnTime = 1.0f; // 락온까지 걸리는 시간
    public float lockOnRange = 500f; // 락온 가능 거리

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color lockedColor = Color.red;      // 락온 완료 색상
    public Color readyColor = Color.red;

    private ObjectPool<RectTransform> _pool;
    private readonly Dictionary<Transform, RectTransform> _map = new();
    private readonly Dictionary<Transform, float> _lockOnProgress = new(); // 각 타겟별 락온 진행도
    private readonly List<Transform> _validTargets = new();
    private readonly List<Transform> _sortedTargets = new();
    private readonly HashSet<Transform> _lockedTargets = new(); // 락온된 타겟들

    private bool _missileReadyVisual;

    void Awake()
    {
        if (!crosshairPrefab || !parentRect)
        {
            Debug.LogError("[HomingTargetOverlay] crosshairPrefab/parentRect not set.");
            enabled = false; return;
        }
        _pool = new ObjectPool<RectTransform>(crosshairPrefab, parentRect, initialPool, maxPool);

        if (!overlayRoot)
        {
            var go = new GameObject("HomingOverlay_Instances", typeof(RectTransform));
            overlayRoot = go.GetComponent<RectTransform>();
            overlayRoot.SetParent(parentRect, false);
            overlayRoot.anchorMin = overlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
            overlayRoot.sizeDelta = Vector2.zero;
            overlayRoot.anchoredPosition = Vector2.zero;
        }

        // 락온 사거리를 미사일 사거리와 동일하게 설정
        lockOnRange = missileRange;
    }

    void OnEnable()
    {
        foreach (var kv in _map) _pool.Return(kv.Value);
        _map.Clear();
        _validTargets.Clear();
        _sortedTargets.Clear();
        _lockOnProgress.Clear();
        _lockedTargets.Clear();
    }

    void LateUpdate()
    {
        if (!player || !playerCamera || !canvas || !scopeRect || registry == null) return;

        _validTargets.Clear();
        var targetsToRemove = new List<Transform>();

        foreach (var rt in registry.Targets)
        {
            if (!rt) continue;
            var tr = rt.transform;

            var le = tr.GetComponent<LivingEntity>();
            if (le != null && le.isDead)
            {
                RemoveFor(tr);
                targetsToRemove.Add(tr);
                continue;
            }

            float distance = Vector3.Distance(player.position, GetAimPoint(tr));
            if (distance > missileRange)
            {
                RemoveFor(tr);
                targetsToRemove.Add(tr);
                continue;
            }

            Vector3 sp = playerCamera.WorldToScreenPoint(GetAimPoint(tr));
            if (sp.z <= 0f)
            {
                RemoveFor(tr);
                targetsToRemove.Add(tr);
                continue;
            }

            if (!ScreenToLocal(scopeRect, new Vector2(sp.x, sp.y), out var inScopeLocal)
                || !scopeRect.rect.Contains(inScopeLocal))
            {
                RemoveFor(tr);
                targetsToRemove.Add(tr);
                continue;
            }

            _validTargets.Add(tr);

            // 락온 진행도 업데이트
            UpdateLockOnProgress(tr, distance);

            if (!_map.TryGetValue(tr, out var ui))
            {
                ui = _pool.Get();
                ui.SetParent(overlayRoot, false);

                foreach (var g in ui.GetComponentsInChildren<Graphic>(true))
                    g.raycastTarget = false;

                _map[tr] = ui;
            }

            if (ScreenToLocal(parentRect, new Vector2(sp.x, sp.y), out var parentLocal))
                ui.anchoredPosition = parentLocal;

            // 락온 상태에 따른 색상 설정
            Color targetColor = GetTargetColor(tr);
            SetUIColors(ui, targetColor);
        }

        // 제거된 타겟들의 락온 정보 정리
        foreach (var tr in targetsToRemove)
        {
            _lockOnProgress.Remove(tr);
            _lockedTargets.Remove(tr);
        }

        CollectRemovals();

        _sortedTargets.Clear();
        _sortedTargets.AddRange(_validTargets);
        _sortedTargets.Sort((a, b) =>
        {
            float da = (GetAimPoint(a) - player.position).sqrMagnitude;
            float db = (GetAimPoint(b) - player.position).sqrMagnitude;
            return da.CompareTo(db);
        });
    }

    private void UpdateLockOnProgress(Transform target, float distance)
    {
        // 락온 가능 거리 내에 있는지 확인
        if (distance <= lockOnRange)
        {
            // 락온 진행도 증가
            if (!_lockOnProgress.ContainsKey(target))
                _lockOnProgress[target] = 0f;

            _lockOnProgress[target] += Time.deltaTime;

            // 락온 완료 체크
            if (_lockOnProgress[target] >= lockOnTime && !_lockedTargets.Contains(target))
            {
                _lockedTargets.Add(target);
                Debug.Log($"🎯 타겟 락온 완료: {target.name}");
            }
        }
        else
        {
            // 락온 사거리 밖이면 락온 진행도 리셋
            if (_lockOnProgress.ContainsKey(target))
            {
                _lockOnProgress.Remove(target);
                _lockedTargets.Remove(target);
            }
        }
    }

    private Color GetTargetColor(Transform target)
    {
        if (_lockedTargets.Contains(target))
            return lockedColor; 

        return normalColor; 
    }

    public void SetMissileReady(bool ready)
    {
        _missileReadyVisual = ready;
        Debug.Log($"미사일 준비 상태 변경: {ready} (락온 상태 유지)");
    }

    public void SetMissileRange(float range)
    {
        missileRange = range;
        lockOnRange = range; // 락온 사거리를 미사일 사거리와 동일하게 설정
    }

    public List<Transform> GetTargetsSortedByDistance()
    {
        // 락온된 타겟들만 반환
        var lockedSortedTargets = new List<Transform>();

        foreach (var target in _sortedTargets)
        {
            if (_lockedTargets.Contains(target))
                lockedSortedTargets.Add(target);
        }

        Debug.Log($"락온된 타겟 수: {lockedSortedTargets.Count} / 전체 타겟: {_sortedTargets.Count}");
        return lockedSortedTargets;
    }

    public bool IsTargetLocked(Transform target)
    {
        return _lockedTargets.Contains(target);
    }

    public float GetLockOnProgress(Transform target)
    {
        if (!_lockOnProgress.ContainsKey(target))
            return 0f;
        return Mathf.Clamp01(_lockOnProgress[target] / lockOnTime);
    }

    void RemoveFor(Transform tr)
    {
        if (_map.TryGetValue(tr, out var ui))
        {
            _pool.Return(ui);
            _map.Remove(tr);
        }
        _lockOnProgress.Remove(tr);
        _lockedTargets.Remove(tr);
    }

    void CollectRemovals()
    {
        var toRemove = new List<Transform>();
        foreach (var kv in _map)
            if (!_validTargets.Contains(kv.Key)) toRemove.Add(kv.Key);

        foreach (var tr in toRemove)
        {
            _pool.Return(_map[tr]);
            _map.Remove(tr);
            _lockOnProgress.Remove(tr);
            _lockedTargets.Remove(tr);
        }
    }

    void SetUIColors(RectTransform ui, Color c)
    {
        foreach (var g in ui.GetComponentsInChildren<Graphic>(true))
            g.color = c;
    }

    bool ScreenToLocal(RectTransform targetRect, Vector2 screen, out Vector2 local)
    {
        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screen, cam, out local);
    }

    Vector3 GetAimPoint(Transform tr)
    {
        var aim = tr.Find("AimPoint");
        return aim ? aim.position : tr.position;
    }

    public void ClearAll()
    {
        foreach (var kv in _map) _pool.Return(kv.Value);
        _map.Clear();
        _validTargets.Clear();
        _sortedTargets.Clear();
        _lockOnProgress.Clear();
        _lockedTargets.Clear();
    }

    void OnDisable()
    {
        ClearAll();
    }
}