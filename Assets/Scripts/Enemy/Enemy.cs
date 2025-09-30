using System;
using UnityEngine;

public class Enemy : LivingEntity
{
    public LockOnManager lockOnManager;
    public EnemyIndicator enemyIndicator;

    // 스포너에 구간 점유 해제를 알리기 위한 콜백
    private Action _releaseOccupy;

    public void SetSpawnerOccupyRelease(Action release)
    {
        _releaseOccupy = release;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.OnDamage(damage, hitPoint, hitNormal);

        // HP 50% 미만이면 도주 상태로 전환
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

        // 구간 점유 해제
        _releaseOccupy?.Invoke();

        Destroy(gameObject, 0.2f);
    }

    void OnCollisionEnter(Collision other)
    {
        // 적 vs 적: 데미지 없음 — 아무 것도 안 함
        if (other.collider.CompareTag("Enemy")) return;

        // 적 vs 플레이어: 서로 -10
        if (other.collider.CompareTag("Player"))
        {
            var player = other.collider.GetComponent<LivingEntity>();
            if (player && !player.isDead)
                player.OnDamage(10f, other.contacts[0].point, -transform.forward);

            if (!isDead) OnDamage(10f, other.contacts[0].point, other.contacts[0].normal);
            return;
        }

        // 지형 충돌: x+180, y-180 반전 후 다시 플레이어를 향해 전진
        // (오일러를 직접 다룰 때 짧게 처리)
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
