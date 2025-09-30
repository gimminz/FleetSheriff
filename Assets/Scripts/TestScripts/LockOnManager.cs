using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class LockOnManager : MonoBehaviour
{
    public Image lockOnImagePrefab;
    public Transform lockOnUIParent;
    public Camera playerCamera;

    public float maxLockOnDistance = 1000f;

    private ObjectPool<Image> lockOnPool;
    public int initPoolSize = 5;
    public int maxPoolSize = 20;

    private Dictionary<Transform, Image> activeLockOns = new Dictionary<Transform, Image>();

    private void Start()
    {
        if (lockOnImagePrefab != null && lockOnUIParent != null)
        {
            lockOnPool=new ObjectPool<Image>(
                lockOnImagePrefab,
                lockOnUIParent,
                initPoolSize,
                maxPoolSize
                );
        }
    }

    private void Update()
    {
        UpdateLockOnPositions();
        RemoveInvalidLockOns();
    }

    public void TouchLockOn(Vector2 screenPosition)
    {

        Ray ray = playerCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxLockOnDistance))
        {
            if (hit.collider.CompareTag("Enemy") || hit.collider.GetComponent<Enemy>() != null)
            {
                Transform enemyTransform = hit.collider.transform;
                if (activeLockOns.ContainsKey(enemyTransform))
                {
                    RemoveLockOn(enemyTransform);
                    Debug.Log("remove LockOn");
                }
                else
                {
                    AddLockOn(enemyTransform);
                    Debug.Log("add LockOn");
                }
            }
        }
    }

    private void AddLockOn(Transform enemy)
    {
        if (lockOnPool == null || enemy==null) return;
        
        Image lockOnImage = lockOnPool.Get();
        if (lockOnImage != null)
        {
            activeLockOns[enemy] = lockOnImage;
            UpdateLockOnPosition(enemy, lockOnImage);
        }
    }

    public void RemoveLockOn(Transform enemy)
    {
        if (activeLockOns.TryGetValue(enemy, out Image lockOnImage))
        {
            activeLockOns.Remove(enemy);
            if (lockOnPool != null && lockOnImage != null)
            {
                lockOnPool.Return(lockOnImage);
            }
        }
    }

    private void UpdateLockOnPosition(Transform enemy, Image lockOnImage)
    {
        if (enemy == null || lockOnImage == null || playerCamera == null) return;

        Vector3 enemyScreenPos = playerCamera.WorldToScreenPoint(enemy.position);

        if (enemyScreenPos.z <= 0 || Vector3.Distance(playerCamera.transform.position, enemy.position) > maxLockOnDistance)
        {
            lockOnImage.gameObject.SetActive(false);
            return;
        }
        lockOnImage.gameObject.SetActive(true);

        RectTransform rectTransform = lockOnImage.rectTransform;
        rectTransform.position = enemyScreenPos;

    }

    private void UpdateLockOnPositions()
    {
        foreach (var alo in activeLockOns)
        {
            Transform enemy = alo.Key;
            Image lockOnImage = alo.Value;
            if (enemy != null && lockOnImage != null)
            {
                UpdateLockOnPosition(enemy, lockOnImage);
            }
        }
    }

    private void RemoveInvalidLockOns()
    {
        var enemiesToRemove = new List<Transform>();
        
        foreach (var alo in activeLockOns)
        {
            Transform enemy = alo.Key;

            if(enemy==null) 
            {
                enemiesToRemove.Add(enemy);
                continue;
            }

            if (Vector3.Distance(playerCamera.transform.position, enemy.position)>maxLockOnDistance)
            {
                enemiesToRemove.Add(enemy);
            }
        }

        foreach (var enemy in enemiesToRemove)
        {
            RemoveLockOn(enemy);
        }
    }

    public void ClearAllLockOns()
    {
        var allEnemies = new List<Transform>(activeLockOns.Keys);
        foreach (var enemy in allEnemies)
        {
            RemoveLockOn(enemy);
        }
    }

    public List<Transform> GetLockedOnEnemies()
    {
        var lockedEnemies = new List<Transform>();
        foreach (var alo in activeLockOns)
        {
            if (alo.Key != null) lockedEnemies.Add(alo.Key);
        }
        return lockedEnemies;
    }
    private void OnDestroy()
    {
        ClearAllLockOns();
    }
}
