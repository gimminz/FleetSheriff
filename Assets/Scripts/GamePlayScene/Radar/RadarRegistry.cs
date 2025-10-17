using System.Collections.Generic;
using UnityEngine;

public class RadarRegistry : MonoBehaviour
{
    public static RadarRegistry Instance { get; private set; }

    private readonly HashSet<RadarTarget> _targets = new HashSet<RadarTarget>();
    public IReadOnlyCollection<RadarTarget> Targets => _targets;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[RadarRegistry] Duplicate instance detected. Destroying this.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(RadarTarget t)
    {
        if (t != null) _targets.Add(t);
    }

    public void Unregister(RadarTarget t)
    {
        if (t != null) _targets.Remove(t);
    }
}
