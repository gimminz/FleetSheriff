using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LaunchContext
{
    public static int? SelectedShipId;
    public static Dictionary<ShipSlot, int?> SelectedWeapons;

    public static void Clear()
    {
        SelectedShipId = null;
        SelectedWeapons = null;
    }
}

public class ReadyWindowPanel : GenericWindow
{
    public Button goButton;
    public Button backButton;
    public GameObject infoPanel;
    public SelectedShipContext selectedShipContext;

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                manager.Open(Menus.MainMenuSelect);
            });
        }

        if (goButton != null) goButton.onClick.AddListener(OnClickGo);
    }

    public override void Open()
    {
        if (backButton != null) firstSelected = backButton.gameObject;
        if (infoPanel != null) infoPanel.SetActive(false);
        base.Open();
    }

    private void OnClickGo()
    {
        if (selectedShipContext != null && selectedShipContext.ActiveShipId.HasValue)
        {
            var shipId = selectedShipContext.ActiveShipId.Value;
            LaunchContext.SelectedShipId = shipId;

            if (selectedShipContext.TryGetWeapons(shipId, out var weapons))
                LaunchContext.SelectedWeapons = new Dictionary<ShipSlot, int?>(weapons);
            else
                LaunchContext.SelectedWeapons = null;
        }
        else
        {
            LaunchContext.SelectedShipId = null;
            LaunchContext.SelectedWeapons = null;
        }

        SceneManager.LoadScene("GamePlayScene");
    }

}
