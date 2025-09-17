using UnityEngine;
using UnityEngine.UI;

public class MissionPanel : GenericWindow
{
    public GameObject infoPanel;
    public Button ReadyButton;
    public Button backButton;

    private void Awake()
    {
        if (ReadyButton != null) ReadyButton.onClick.AddListener(() => manager.Open(Menus.ReadyWindowPanel));
        if (backButton != null) backButton.onClick.AddListener(() => manager.Open(Menus.MainMenuSelect));
    }

    public override void Open()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
        if (backButton != null) firstSelected = backButton.gameObject;

        base.Open();
    }
}
