using UnityEngine;
public interface IDamagable
{
    void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal);
    void OnDamage();
}