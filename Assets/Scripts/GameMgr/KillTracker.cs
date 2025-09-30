using UnityEngine;
using System;

public class KillTracker : MonoBehaviour
{
    public static KillTracker Instance { get; private set; }

    public int KillCount { get; private set; }

    public event Action<int> OnKillCountChanged;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Debug.LogWarning("[KillTracker] Duplicate instance destroyed.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        KillCount = 0;

        Debug.Log("[KillTracker] Instance created and initialized.");
    }

    public void ResetCount()
    {
        Debug.Log($"[KillTracker] Resetting count from {KillCount} to 0");
        KillCount = 0;
        OnKillCountChanged?.Invoke(KillCount);
    }

    public void AddKill(int amount = 1)
    {
        int before = KillCount;
        KillCount = Mathf.Max(0, KillCount + amount);

        Debug.Log($"[KillTracker] AddKill({amount}). Before: {before}, After: {KillCount}");

        OnKillCountChanged?.Invoke(KillCount);
    }
}