using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [System.Serializable]
    public class Muzzle
    {
        public Transform muzzleTransform;  
        public LineRenderer lineRenderer;  
    }
    public Muzzle[] muzzles;  

    public float fireDistance = 50f;
    public float damage = 25f;
    public float fireDelay = 0.2f;

    private float lastFireTime;

    public void FireButton()
    {
        if (Time.time >= lastFireTime + fireDelay)
        {
            Fire();
            lastFireTime = Time.time;
        }
    }

    private void Fire()
    {
        foreach (var m in muzzles)
        {
            ShootFromMuzzle(m);
        }
    }

    private void ShootFromMuzzle(Muzzle m)
    {
        Vector3 hitPosition = m.muzzleTransform.position + m.muzzleTransform.forward * fireDistance;

        if (Physics.Raycast(m.muzzleTransform.position, m.muzzleTransform.forward, out RaycastHit hit, fireDistance))
        {
            hitPosition = hit.point;

            var target = hit.collider.GetComponent<IDamagable>();
            if (target != null)
            {
                target.OnDamage(damage, hit.point, hit.normal);
            }
        }

        StartCoroutine(CoShotEffect(m, hitPosition));
    }

    private IEnumerator CoShotEffect(Muzzle m, Vector3 hitPosition)
    {
        if (m.lineRenderer != null)
        {
            m.lineRenderer.enabled = true;
            m.lineRenderer.positionCount = 2;
            m.lineRenderer.SetPosition(0, m.muzzleTransform.position);
            m.lineRenderer.SetPosition(1, hitPosition);

            yield return new WaitForSeconds(0.1f);
            m.lineRenderer.enabled = false;
        }
    }
}
