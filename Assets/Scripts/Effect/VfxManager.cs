using UnityEngine;

public class VfxManager : MonoBehaviour
{
    public static VfxManager Instance { get; private set; }
    public PooledVfx explosionPrefab;
    public Transform vfxRoot;
    public int initialPoolSize = 10;
    public int maxPoolSize = 50;

    private ObjectPool<PooledVfx> _explosionPool;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _explosionPool = new ObjectPool<PooledVfx>(
            explosionPrefab,
            vfxRoot,
            initialPoolSize,
            maxPoolSize
        );

        WarmupInit();
    }

    void WarmupInit()
    {
        int warmCount = Mathf.Min(initialPoolSize, 20);
        for (int i = 0; i < warmCount; i++)
        {
            var vfx = _explosionPool.Get();
            if (vfx) vfx.Init(_explosionPool);
            _explosionPool.Return(vfx);
        }
    }

    public void SpawnExplosion(Vector3 position, float yOffset = 1.0f, Quaternion? rotation = null)
    {
        if (_explosionPool == null) return;

        var vfx = _explosionPool.Get();
        if (!vfx) return;

        vfx.Init(_explosionPool);

        Vector3 pos = position + Vector3.up * yOffset;
        Quaternion rot = rotation ?? Quaternion.identity;

        vfx.PlayAt(pos, rot);
    }
}
