using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public float spawnInterval = 5f; 
    public float spawnRadius = 100f; 
    public Transform player;
    public EnemyIndicator enemyIndicator;

    //debug -> fire test
    public bool spawnTestEnemyInFront = true; 
    public float forwardDistance = 1000f;       
    public float heightOffset = 0f;


    void Start()
    {
        if (spawnTestEnemyInFront)
        {
            SpawnEnemyInFront();
        }

        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        int initialSpawnCount = 3;
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnEnemy();
        }

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPos = player.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = player.position.y + Random.Range(-20f, 20f);
        GameObject spawnedEnemy = Instantiate(enemy, spawnPos, Quaternion.identity);
        if (enemyIndicator != null)
        {
            enemyIndicator.AddEnemy(spawnedEnemy.transform);
        }
    }
    private void SpawnEnemyInFront()
    {
        Vector3 spawnPos = player.position + player.forward * forwardDistance;
        spawnPos.y += heightOffset;

        GameObject spawnedEnemy = Instantiate(enemy, spawnPos, Quaternion.identity);
        if (enemyIndicator != null)
        {
            enemyIndicator.AddEnemy(spawnedEnemy.transform);
        }
    }
}
