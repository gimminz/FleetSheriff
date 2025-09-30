using UnityEngine;

public class MissionSuccessUI : MonoBehaviour
{
    public GameObject panel;

    public void Show()
    {
        if (!panel) return;
        panel.SetActive(true);
    }

    public void Hide()
    {
        if (!panel) return;
        panel.SetActive(false);
    }
}