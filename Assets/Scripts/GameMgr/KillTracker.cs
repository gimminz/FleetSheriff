using UnityEngine;
using System;

public class KillTracker : MonoBehaviour
{
    public static KillTracker Instance { get; private set; }

    public int KillCount { get; private set; }
    public event Action<int> OnKillCountChanged;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        KillCount = 0;
    }

    public void ResetCount()
    {
        KillCount = 0;
        OnKillCountChanged?.Invoke(KillCount);
    }

    public void AddKill(int amount = 1)
    {
        KillCount = Mathf.Max(0, KillCount + amount);
        OnKillCountChanged?.Invoke(KillCount);
    }
}
