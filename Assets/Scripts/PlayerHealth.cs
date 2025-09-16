using UnityEngine;
using UnityEngine.Audio;

public class PlayerHealth : LivingEntity
{
    private PlayerMovement playerMove;
    //  private Gun playerGun;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMovement>();
        // playerGun = GetComponentInChildren<Gun>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        playerMove.enabled = true;
        //playerGun.enabled = true;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDead) return;
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    public override void OnDamage()
    {
        base.OnDamage();
    }

    protected override void Die()
    {
        base.Die();
        playerMove.enabled = false;
        // playerGun.enabled = false;
    }
}
