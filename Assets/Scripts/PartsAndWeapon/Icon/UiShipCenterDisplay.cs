using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiShipCenterDisplay : MonoBehaviour
{
    [Header("Target")]
    public Image shipImage;                              // CenterPanel 하위 Image

    [Header("Resources")]
    public string resourcesPrefix = "ShipImage/";        // Resources/ShipImage/
    public Sprite defaultShipSprite;                     // 폴백(선택)

    // 간단 캐시
    private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (shipImage)
        {
            shipImage.raycastTarget = false;
            shipImage.preserveAspect = true;
            shipImage.gameObject.SetActive(true);
        }
    }

    public void Clear()
    {
        if (!shipImage) return;
        shipImage.sprite = defaultShipSprite ? defaultShipSprite : null;
        shipImage.gameObject.SetActive(shipImage.sprite != null);
    }

    public void SetData(ShipData data)
    {
        if (!shipImage) return;

        string key = data?.ShipName;
        Sprite s = LoadSprite(resourcesPrefix, key);
        if (!s) s = defaultShipSprite;

        shipImage.sprite = s;
        shipImage.gameObject.SetActive(s != null);
    }

    private static Sprite LoadSprite(string prefix, string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        string path = string.IsNullOrWhiteSpace(prefix) ? key : (prefix + key);

        if (_cache.TryGetValue(path, out var cached)) return cached;

        var loaded = Resources.Load<Sprite>(path); // 확장자 제외: ShipImage/ship_speed
        _cache[path] = loaded;                     // 실패(null)도 캐시
        return loaded;
    }
}
