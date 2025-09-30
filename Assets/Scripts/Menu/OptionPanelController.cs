using UnityEngine;
using UnityEngine.UI;

public class OptionPanelController : MonoBehaviour
{
    public GameObject optionPanel;
    public Button mainMenuBtn;
    public Button resumeBtn;
    public Button restartBtn;

    void Awake()
    {
        Hide();
        WireButtons();
    }

    void WireButtons()
    {
        if (mainMenuBtn)
            mainMenuBtn.onClick.AddListener(() =>
            {
                Hide();
                GameFlowManager.Instance.GoToMainMenu();
            });

        if (resumeBtn)
            resumeBtn.onClick.AddListener(() =>
            {
                GameFlowManager.Instance.CloseOption();
            });

        if (restartBtn)
            restartBtn.onClick.AddListener(() =>
            {
                Hide();
                GameFlowManager.Instance.RestartGame();
            });
    }

    public void Show() { if (optionPanel) optionPanel.SetActive(true); }
    public void Hide() { if (optionPanel) optionPanel.SetActive(false); }
}
