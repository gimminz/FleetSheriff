using UnityEngine;

public class MissionSuccessUI : MonoBehaviour
{
    public GameObject panel;

    public void Show()
    {
        if (!panel) return;
        panel.SetActive(true);
        Time.timeScale = 0f;
    }
}