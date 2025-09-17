using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReadyWindowPanel : GenericWindow
{
    public Button goButton;
    public Button backButton;
    public GameObject infoPanel;  

    private void Awake()
    {
        if (backButton != null) backButton.onClick.AddListener(() => manager.Open(Menus.ReadyWindowPanel));
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
        SceneManager.LoadScene("GamePlayScene");
    }
}
