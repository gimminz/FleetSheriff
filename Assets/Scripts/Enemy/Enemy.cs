using System;
using UnityEngine;

public class Enemy : LivingEntity
{
    public LockOnManager lockOnManager;
    public EnemyIndicator enemyIndicator;
    private Action _releaseOccupy;

    public void SetSpawnerOccupyRelease(Action release)
    {
        _releaseOccupy = release;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.OnDamage(damage, hitPoint, hitNormal);

        var move = GetComponent<EnemyMovement>();
        if (move && health < maxHealth * 0.5f)
        {
            move.EnterFlee();
        }
    }

    protected override void Die()
    {
        base.Die();

        var rt = GetComponent<RadarTarget>();
        if (rt) rt.enabled = false;

        if (KillTracker.Instance) KillTracker.Instance.AddKill(1);

        _releaseOccupy?.Invoke();

        Destroy(gameObject, 0.2f);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Enemy")) return;

        if (other.collider.CompareTag("Player"))
        {
            var player = other.collider.GetComponent<LivingEntity>();
            if (player && !player.isDead)
                player.OnDamage(10f, other.contacts[0].point, -transform.forward);

            if (!isDead) OnDamage(10f, other.contacts[0].point, other.contacts[0].normal);
            return;
        }

        if (other.collider.gameObject.layer == LayerMask.NameToLayer("Default") ||
            other.collider.gameObject.isStatic)
        {
            var eul = transform.eulerAngles;
            eul.x = (eul.x + 180f) % 360f;
            eul.y = (eul.y - 180f + 360f) % 360f;
            transform.eulerAngles = eul;

            var move = GetComponent<EnemyMovement>();
            if (move) move.RecoverAfterBounce();
        }
    }
}
