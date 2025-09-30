using System;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagable
{
    public float maxHealth = 100f;
    public float health { get; protected set; }
    public bool isDead { get; private set; }

    public event Action OnDeath;

    protected virtual void OnEnable()
    {
        isDead = false;
        health = maxHealth;
    }
    public virtual void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        health -= damage;
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    public virtual void OnDamage()
    {
        health -= 50;
        Debug.Log(health); //debug
    }

    protected virtual void Die()
    {
        if (OnDeath != null)
        {
            OnDeath();
        }
        isDead = true;
    }
}
