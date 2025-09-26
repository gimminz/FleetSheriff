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

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color readyColor = Color.red;

    private ObjectPool<RectTransform> _pool;
    private readonly Dictionary<Transform, RectTransform> _map = new();
    private readonly List<Transform> _validTargets = new();
    private readonly List<Transform> _sortedTargets = new();

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
    }

    void OnEnable()
    {
        foreach (var kv in _map) _pool.Return(kv.Value);
        _map.Clear();
        _validTargets.Clear();
        _sortedTargets.Clear();
    }

    void LateUpdate()
    {
        if (!player || !playerCamera || !canvas || !scopeRect || registry == null) return;

        _validTargets.Clear();

        foreach (var rt in registry.Targets)
        {
            if (!rt) continue;
            var tr = rt.transform;

            var le = tr.GetComponent<LivingEntity>();
            if (le != null && le.isDead) { RemoveFor(tr); continue; }

            if (Vector3.Distance(player.position, GetAimPoint(tr)) > missileRange) { RemoveFor(tr); continue; }

            Vector3 sp = playerCamera.WorldToScreenPoint(GetAimPoint(tr));
            if (sp.z <= 0f) { RemoveFor(tr); continue; }

            if (!ScreenToLocal(scopeRect, new Vector2(sp.x, sp.y), out var inScopeLocal)
                || !scopeRect.rect.Contains(inScopeLocal))
            { RemoveFor(tr); continue; }

            _validTargets.Add(tr);

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

            SetUIColors(ui, _missileReadyVisual ? readyColor : normalColor);
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

    public void SetMissileReady(bool ready)
    {
        _missileReadyVisual = ready;
    }

    public List<Transform> GetTargetsSortedByDistance()
    {
        return _sortedTargets;
    }

    void RemoveFor(Transform tr)
    {
        if (_map.TryGetValue(tr, out var ui))
        {
            _pool.Return(ui);
            _map.Remove(tr);
        }
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
    }
    void OnDisable()
    {
        ClearAll(); 
    }
}
