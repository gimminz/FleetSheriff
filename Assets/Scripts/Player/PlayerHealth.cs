using UnityEngine;
using UnityEngine.Audio;

public class PlayerHealth : LivingEntity
{
    private PlaneController planeController;
    private Gun gun;

    private void Awake()
    {
        planeController = GetComponent<PlaneController>();
        gun = GetComponent<Gun>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (planeController) planeController.enabled = true;
        if (gun) gun.enabled = true;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDead) return;
        base.OnDamage(damage, hitPoint, hitNormal);
        Debug.Log($"[PlayerHealth] Player took {damage} damage. Current HP: {health}");
    }

    public override void OnDamage()
    {
        if (isDead) return;
        base.OnDamage();
        Debug.Log($"[PlayerHealth] Player took default damage. Current HP: {health}");
    }

    protected override void Die()
    {
        base.Die();

        if (planeController) planeController.enabled = false;
        if (gun) gun.enabled = false;

        gameObject.SetActive(false);

        Debug.Log("[PlayerHealth] Player died.");
    }
    public float GetCurrentHealth()
    {
        return health;
    }
}
