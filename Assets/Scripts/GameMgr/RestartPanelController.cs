using TMPro;
using UnityEngine;

public class RestartPanelController : MonoBehaviour
{
    public GameObject root;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public GameObject resumeButton;

    void Awake()
    {
        if (root) root.SetActive(false);
    }

    public void Show(string title, string desc, bool isPause)
    {
        if (titleText) titleText.text = title;
        if (descText) descText.text = desc;
        if (resumeButton) resumeButton.SetActive(isPause);

        if (root) root.SetActive(true);
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
    }
}
