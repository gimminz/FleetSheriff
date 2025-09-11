using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public float spawnInterval = 5f; 
    public float spawnRadius = 500f; 
    public Transform player;

    void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            Vector3 spawnPos = player.position + Random.insideUnitSphere * spawnRadius;
            spawnPos.y = player.position.y + Random.Range(-20f, 20f);
            Instantiate(enemy, spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
