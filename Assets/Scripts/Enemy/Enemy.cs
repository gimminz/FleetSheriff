using UnityEngine;

public class Enemy : LivingEntity
{
    public LockOnManager lockOnManager;
    public EnemyIndicator enemyIndicator;

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
         base.OnDamage(damage, hitPoint, hitNormal);
    }

    protected override void Die()
    {
        base.Die(); 

        if (lockOnManager != null) lockOnManager.RemoveLockOn(transform);
        if (enemyIndicator != null) enemyIndicator.RemoveEnemy(transform);

        Destroy(gameObject, 0.2f);
    }
}