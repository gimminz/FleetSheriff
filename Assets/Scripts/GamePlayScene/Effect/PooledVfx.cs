using UnityEngine;
using System.Collections.Generic;

public class PooledVfx : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> systems = new List<ParticleSystem>();
    private ObjectPool<PooledVfx> _pool;
    void Awake()
    {
        if (systems == null || systems.Count == 0)
            GetComponentsInChildren(true, systems);

        foreach (var ps in systems)
        {
            if (!ps) continue;
            var main = ps.main; main.loop = false;
            var emission = ps.emission; emission.rateOverTime = 0f;
        }
    }

    public void Init(ObjectPool<PooledVfx> pool) => _pool = pool;

    public void PlayAt(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);

        foreach (var ps in systems)
        {
            if (!ps) continue;
            ps.Clear(true);
            ps.Play(true);
        }

        StopAllCoroutines();
        StartCoroutine(ReturnWhenFinished());
    }

    System.Collections.IEnumerator ReturnWhenFinished()
    {
        bool alive = true;
        while (alive)
        {
            alive = false;
            foreach (var ps in systems)
            {
                if (ps && ps.IsAlive(true)) { alive = true; break; }
            }
            yield return null;
        }

        if (_pool != null) _pool.Return(this);
        else gameObject.SetActive(false);
    }
}
