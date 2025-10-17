using UnityEngine;
using UnityEngine.UI;

public class MainMenuSelectPanel : GenericWindow
{
    public Button starShipButton;
    public Button missionButton;
    public GameObject infoPanel;

    private void Awake()
    {
        if (starShipButton != null)  starShipButton.onClick.AddListener(OnClickStartShip);
        if (missionButton != null) missionButton.onClick.AddListener(OnClickMission);
    }
    public override void Open()
    {
        if (infoPanel != null) infoPanel.SetActive(true);

        base.Open();
    }

    private void OnClickStartShip()
    {
        manager.Open(Menus.ManageStarShip);
    }

    private void OnClickMission()
    {
        manager.Open(Menus.MissionInfo);       
    }
}
