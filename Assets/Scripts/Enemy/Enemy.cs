using System;
using UnityEngine;

public class Enemy : LivingEntity
{
    public LockOnManager lockOnManager;
    public EnemyIndicator enemyIndicator;
    private Action _releaseOccupy;

    [Header("VFX")]
    public float explosionYOffset = 1.0f;

    private bool _dying = false;
    private bool _explosionSpawned = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        _dying = false;
        _explosionSpawned = false;
    }

    public void SetSpawnerOccupyRelease(Action release)
    {
        _releaseOccupy = release;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (_dying || isDead) return;

        base.OnDamage(damage, hitPoint, hitNormal);

        var move = GetComponent<EnemyMovement>();
        if (move && health < maxHealth * 0.5f)
        {
            move.EnterFlee();
        }
    }

    protected override void Die()
    {
        if (_dying || isDead) return;
        _dying = true;

        var cols = GetComponentsInChildren<Collider>(true);
        foreach (var c in cols) c.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (!_explosionSpawned && VfxManager.Instance)
        {
            VfxManager.Instance.SpawnExplosion(transform.position, explosionYOffset);
            _explosionSpawned = true;
        }

        var rt = GetComponent<RadarTarget>();
        if (rt) rt.enabled = false;

        if (KillTracker.Instance) KillTracker.Instance.AddKill(1);
        _releaseOccupy?.Invoke();

        base.Die();

        Destroy(gameObject, 0.2f);
    }

    void OnCollisionEnter(Collision other)
    {
        if (_dying || isDead) return;

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
