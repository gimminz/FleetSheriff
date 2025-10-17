using UnityEngine;

public class RadarTarget : MonoBehaviour
{
    private void OnEnable()
    {
        if (RadarRegistry.Instance != null)
            RadarRegistry.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (RadarRegistry.Instance != null)
            RadarRegistry.Instance.Unregister(this);
    }
}
