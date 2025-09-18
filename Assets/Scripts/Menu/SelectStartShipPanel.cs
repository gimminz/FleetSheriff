using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectStartShipPanel : GenericWindow
{
    public Button backButton;
    private void Awake()
    {
        if (backButton != null) backButton.onClick.AddListener(() => manager.Open(Menus.MainMenuSelect));
    }

    public override void Open()
    {
        if (backButton != null) firstSelected = backButton.gameObject;

        base.Open();
    }
}
