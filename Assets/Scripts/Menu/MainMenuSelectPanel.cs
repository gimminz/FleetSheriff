using UnityEngine;
using UnityEngine.UI;

public class MainMenuSelectPanel : GenericWindow
{
    public Button startShipButton;
    public Button missionButton;

    private void Awake()
    {
        if (startShipButton != null)  startShipButton.onClick.AddListener(OnClickStartShip);
        if (missionButton != null) missionButton.onClick.AddListener(OnClickMission);
    }

    private void OnClickStartShip()
    {
        manager.Open(Menus.ManageStartShip); 
    }

    private void OnClickMission()
    {
        manager.Open(Menus.MissionInfo);       
    }
}
