using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairAutoTracker : MonoBehaviour
{
    public Transform player;                  
    public Camera playerCamera;              
    public Canvas canvas;                    
    public RectTransform parentRect;            
    public RectTransform scopeRect;    
    public RectTransform crosshair;  
    public Image crosshairImage;                
    public RadarRegistry registry;              

    public float range = 300f;           
    public float moveSpeed = 12f;                
    public float lockPixelThreshold = 8f;  
    public Color normalColor = Color.white;
    public Color lockedColor = Color.red;

    private Graphic[] _crosshairGraphics;

    public bool IsLocked { get; private set; }
    public Transform CurrentTarget { get; private set; }


    static readonly List<Transform> _candidate = new List<Transform>();

    void Reset()
    {
        crosshairImage = crosshair ? crosshair.GetComponent<Image>() : null;
        _crosshairGraphics = crosshair ? crosshair.GetComponentsInChildren<Graphic>(true) : null;
    }

    void LateUpdate()
    {
        if (!player || !playerCamera || !canvas || !parentRect || !scopeRect || !crosshair || !registry) return;

        _candidate.Clear();
        bool keepCurrent = IsTargetValid(CurrentTarget);
        if (!keepCurrent) CurrentTarget = null;

        float closestDist = float.MaxValue;
        Transform best = keepCurrent ? CurrentTarget : null;

        foreach (var t in registry.Targets)
        {
            if (!t) continue;

            var tr = t.transform;
            if (!IsTargetValid(tr)) continue;

            float d = Vector3.SqrMagnitude(tr.position - player.position); 
            if (keepCurrent) continue; 

            if (d < closestDist)
            {
                closestDist = d;
                best = tr;
            }
        }

        if (!keepCurrent) CurrentTarget = best;

        Vector2 targetLocal = GetScopeCenterInParent();
        if (CurrentTarget)
        {
            if (TryGetScreenPoint(CurrentTarget, out Vector2 screen))
            {
                if (IsInsideScope(screen))
                {
                    if (ScreenToLocal(parentRect, screen, out Vector2 local))
                        targetLocal = local;
                }
                else
                {
                    CurrentTarget = null;
                    IsLocked = false;
                }
            }
            else
            {
                CurrentTarget = null;
                IsLocked = false;
            }
        }

        var cur = crosshair.anchoredPosition;
        var next = Vector2.MoveTowards(cur, targetLocal, moveSpeed * 60f * Time.deltaTime); 
        crosshair.anchoredPosition = next;

        if (CurrentTarget && TryGetScreenPoint(CurrentTarget, out Vector2 screen2))
        {
            if (ScreenToLocal(parentRect, screen2, out Vector2 local2))
            {
                float px = Vector2.Distance(local2, crosshair.anchoredPosition);
                bool locked = px <= lockPixelThreshold;
                if (locked != IsLocked)
                {
                    IsLocked = locked;
                    SetCrosshairColor(IsLocked ? lockedColor : normalColor); // ← 여기만 교체
                }
            }
            else ReleaseLock();
        }
        else
        {
            ReleaseLock();
        }
    }

    void ReleaseLock()
    {
        if (IsLocked)
        {
            IsLocked = false;
            SetCrosshairColor(normalColor);
        }
    }
    void SetCrosshairColor(Color c)
    {
        if ((_crosshairGraphics == null || _crosshairGraphics.Length == 0) && crosshair)
            _crosshairGraphics = crosshair.GetComponentsInChildren<Graphic>(true);

        if (_crosshairGraphics != null)
        {
            for (int i = 0; i < _crosshairGraphics.Length; i++)
            {
                var g = _crosshairGraphics[i];
                if (g) g.color = c;
            }
        }
        else if (crosshairImage)
        {
            crosshairImage.color = c;
        }
    }

    bool IsTargetValid(Transform tr)
    {
        if (!tr) return false;

        float dist = Vector3.Distance(player.position, tr.position);
        if (dist > range) return false;

        Vector3 sp = playerCamera.WorldToScreenPoint(GetTargetWorldPos(tr));
        if (sp.z <= 0f) return false;

        return IsInsideScope(new Vector2(sp.x, sp.y));
    }

    bool TryGetScreenPoint(Transform tr, out Vector2 screen)
    {
        Vector3 sp = playerCamera.WorldToScreenPoint(GetTargetWorldPos(tr));
        if (sp.z <= 0f)
        {
            screen = default; return false;
        }
        screen = new Vector2(sp.x, sp.y);
        return true;
    }

    bool IsInsideScope(Vector2 screen)
    {

        if (ScreenToLocal(scopeRect, screen, out Vector2 localInScope))
            return scopeRect.rect.Contains(localInScope);
        return false;
    }

    Vector2 GetScopeCenterInParent()
    {
        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, scopeRect.position);
        if (ScreenToLocal(parentRect, screenCenter, out Vector2 local))
            return local;
        return Vector2.zero;
    }

    bool ScreenToLocal(RectTransform targetRect, Vector2 screen, out Vector2 local)
    {
        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screen, cam, out local);
    }

    Vector3 GetTargetWorldPos(Transform tr)
    {
        var aim = tr.Find("AimPoint");
        return aim ? aim.position : tr.position;
    }
}
