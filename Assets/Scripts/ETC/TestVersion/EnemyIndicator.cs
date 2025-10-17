using UnityEngine;
using System.Collections.Generic;

public class EnemyIndicator : MonoBehaviour
{
    public Transform player;
    public RectTransform canvas;
    public RectTransform arrowPrefab;

    public float radius = 100f;
    public int poolSize = 20;

    private ObjectPool<RectTransform> arrowPool;
    private Dictionary<Transform, RectTransform> enemyArrowMap=new Dictionary<Transform,RectTransform>();
    public List<Transform> enemies = new List<Transform>();

    private void Start()
    {
        arrowPool = new ObjectPool<RectTransform>(arrowPrefab, canvas, poolSize * 2);
    }

    private void Update()
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null)
            {
                Transform nullEnemy = enemies[i];
                if(enemyArrowMap.ContainsKey(nullEnemy))
                {
                    arrowPool.Return(enemyArrowMap[nullEnemy]);
                    enemyArrowMap.Remove(nullEnemy);
                }
                enemies.RemoveAt(i);
            }
        }

        foreach (Transform enemy in enemies)
        {
            if (!enemyArrowMap.ContainsKey(enemy))
            {
                RectTransform arrow = arrowPool.Get();
                enemyArrowMap[enemy] = arrow;
            }
            UpdateArrow(enemyArrowMap[enemy], enemy);
        }
    }
    private void UpdateArrow(RectTransform arrow, Transform enemy)
    {
        Vector3 enemyDir = player.InverseTransformDirection(enemy.position - player.position);
        Vector2 arrowPlane = new Vector2(enemyDir.x, enemyDir.y);
        arrowPlane.Normalize();
        Vector2 arrowPos = arrowPlane * radius;
        arrow.anchoredPosition = arrowPos;

        float angle = Mathf.Atan2(arrowPlane.y, arrowPlane.x) * Mathf.Rad2Deg - 90f;
        arrow.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void AddEnemy(Transform enemy)
    {
        if (!enemies.Contains(enemy)) enemies.Add(enemy);
    }

    public void RemoveEnemy(Transform enemy)
    {
        if (enemies.Contains(enemy)) enemies.Remove(enemy);

        if (enemyArrowMap.ContainsKey(enemy))
        {
            arrowPool.Return(enemyArrowMap[enemy]);
            enemyArrowMap.Remove(enemy);
        }
    }
}
