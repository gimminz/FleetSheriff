using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarController : MonoBehaviour
{
    public Transform player;                
    public RectTransform radarRect;          
    public RectTransform dotsContainer;   
    public RadarRegistry registry;            
    public RadarDotPool dotPool;           

    public float worldRadius = 500f;        

    private readonly List<Image> _activeDots = new List<Image>();
    private readonly List<Vector2> _visiblePositions = new List<Vector2>();

    void LateUpdate()
    {
        if (!player || !radarRect || !dotsContainer || !registry || !dotPool) return;

        float uiRadiusPx = radarRect.rect.width * 0.5f; 
        if (uiRadiusPx <= 0f || worldRadius <= 0f) return;

        float scale = uiRadiusPx / worldRadius;
        float yaw = player.eulerAngles.y;
        Quaternion invYaw = Quaternion.Euler(0f, -yaw, 0f);

        _visiblePositions.Clear();

        foreach (var t in registry.Targets)
        {
            if (!t) continue;
            Vector3 d = t.transform.position - player.position;
            d.y = 0f; 

            float dist = d.magnitude;
            if (dist > worldRadius) continue;

            Vector3 local = invYaw * d;              
            Vector2 ui = new Vector2(local.x, local.z) * scale;
            _visiblePositions.Add(ui);
        }

        int needed = _visiblePositions.Count;

        while (_activeDots.Count < needed)
        {
            var dot = dotPool.Rent();
            dot.transform.SetParent(dotsContainer, false);
            _activeDots.Add(dot);
        }

        for (int i = _activeDots.Count - 1; i >= needed; i--)
        {
            dotPool.Return(_activeDots[i]);
            _activeDots.RemoveAt(i);
        }

        for (int i = 0; i < needed; i++)
        {
            var rt = (RectTransform)_activeDots[i].transform;
            rt.anchoredPosition = _visiblePositions[i];
        }
    }
}
