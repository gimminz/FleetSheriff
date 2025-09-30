using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class LaunchContext
{
    public static int? SelectedShipId;

    public static void Clear()
    {
        SelectedShipId = null;
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
                Debug.Log("[BackButton] Clicked - Try to open MainMenuSelect");
                manager.Open(Menus.MainMenuSelect);
            });
        }
        else
        {
            Debug.LogError("[BackButton] is NULL!");
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
            LaunchContext.SelectedShipId = selectedShipContext.ActiveShipId.Value;
            Debug.Log($"[ReadyWindowPanel] Launch with ShipId={LaunchContext.SelectedShipId}");
        }
        else
        {
            Debug.LogWarning("[ReadyWindowPanel] ActiveShipId가 없어 기본값으로 진행합니다.");
            LaunchContext.SelectedShipId = null;
        }

        SceneManager.LoadScene("GamePlayScene");
    }
}
