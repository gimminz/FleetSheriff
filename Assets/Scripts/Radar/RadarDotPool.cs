using UnityEngine;
using UnityEngine.UI;

public class RadarDotPool : MonoBehaviour
{
    public Image dotPrefab;              
    public Transform parent;              
    public int initialSize = 64;
    public int maxSize = 128;

    private ObjectPool<Image> _pool;

    private void Awake()
    {
        if (dotPrefab == null || parent == null)
        {
            Debug.LogError("[RadarDotPool] dotPrefab/parent not set.");
            enabled = false;
            return;
        }
        _pool = new ObjectPool<Image>(dotPrefab, parent, initialSize, maxSize);
    }

    public Image Rent()
    {
        return _pool.Get();
    }

    public void Return(Image img)
    {
        _pool.Return(img);
    }
}
