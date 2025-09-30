using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public string mainMenuSceneName = "MainMenuScene";
    public OptionPanelController optionPanel;

    public GameObject failPanel;
    public GameObject successPanel;   // ★ 추가

    public PlayerHealth player;

    public MonoBehaviour[] pauseTargets; //Pause Scripts

    public PlayerInput playerInput;
    public string playerActionMapName = "Player";
    public string uiActionMapName = "UI";

    float _prevTimeScale = 1f;
    bool _isPaused = false;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _prevTimeScale = 1f;
        _isPaused = false;

        if (optionPanel) optionPanel.Hide();
        if (failPanel) failPanel.SetActive(false);
        if (successPanel) successPanel.SetActive(false);
    }

    private void Start()
    {
        if (player != null)
            player.OnDeath += ShowFailPanel;
    }

    private void OnDestroy()
    {
        if (player != null) player.OnDeath -= ShowFailPanel;
    }

    // ===== 옵션/일시정지 =====

    public void OpenOption()
    {
        PauseGame();
        if (optionPanel) optionPanel.Show();
    }

    public void CloseOption()
    {
        if (optionPanel) optionPanel.Hide();
        ResumeGame();
    }

    public void PauseGame()
    {
        if (_isPaused) return;

        _prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        _isPaused = true;

        SetPauseTargetsEnabled(false);

        if (playerInput && !string.IsNullOrEmpty(uiActionMapName))
        {
            var current = playerInput.currentActionMap?.name;
            if (current != uiActionMapName) playerInput.SwitchCurrentActionMap(uiActionMapName);
        }
    }

    public void ResumeGame()
    {
        if (!_isPaused) return;

        Time.timeScale = _prevTimeScale <= 0f ? 1f : _prevTimeScale;
        AudioListener.pause = false;
        _isPaused = false;

        SetPauseTargetsEnabled(true);

        if (playerInput && !string.IsNullOrEmpty(playerActionMapName))
        {
            var current = playerInput.currentActionMap?.name;
            if (current != playerActionMapName) playerInput.SwitchCurrentActionMap(playerActionMapName);
        }
    }

    void SetPauseTargetsEnabled(bool enabled)
    {
        if (pauseTargets == null) return;
        foreach (var t in pauseTargets)
        {
            if (t) t.enabled = enabled;
        }
    }

    // ===== 결과 패널 =====

    public void ShowFailPanel()
    {
        PauseGame();
        if (successPanel) successPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(true);
    }

    public void ShowSuccessPanel()
    {
        PauseGame();
        if (failPanel) failPanel.SetActive(false);
        if (successPanel) successPanel.SetActive(true);
    }

    // ===== 재시작/메뉴 =====

    public void RestartGame()
    {
        if (optionPanel) optionPanel.Hide();
        if (failPanel) failPanel.SetActive(false);
        if (successPanel) successPanel.SetActive(false);

        SetPauseTargetsEnabled(true);
        _isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (playerInput && !string.IsNullOrEmpty(playerActionMapName))
        {
            var current = playerInput.currentActionMap?.name;
            if (current != playerActionMapName) playerInput.SwitchCurrentActionMap(playerActionMapName);
        }

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        _isPaused = false;

        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }
}
