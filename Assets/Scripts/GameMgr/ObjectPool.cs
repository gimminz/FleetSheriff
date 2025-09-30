using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T: Component
{
    private readonly Queue<T> pool = new Queue<T>();
    private readonly T prefab;
    private readonly Transform parent;
    private readonly int maxSize;
    public ObjectPool(T prefab, Transform parent, int initialSize = 10, int maxSize = 100)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.maxSize = maxSize;

        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    public T Get()
    {
        if (pool.Count == 0)
        {
            CreateNewObject();
        }

        T obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        if (obj == null) return;
        obj.gameObject.SetActive(false);

        if (pool.Count < maxSize)
        {
            pool.Enqueue(obj);
        }
        else
        {
            Object.Destroy(obj.gameObject);
        }
    }

    private void CreateNewObject()
    {
        T newObj = Object.Instantiate(prefab, parent);
        newObj.gameObject.SetActive(false);
        pool.Enqueue(newObj);
    }

    public int PoolSize => pool.Count;
}
