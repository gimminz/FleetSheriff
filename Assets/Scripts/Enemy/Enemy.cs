using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public LockOnManager lockOnManager;

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"Enemy {gameObject.name} took {damage} damage. Health: {health}");

        if (health <= 0)
        {
            DestroyEnemy();
        }
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}
