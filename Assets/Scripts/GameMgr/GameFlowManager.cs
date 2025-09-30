using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public string mainMenuSceneName = "MainMenuScene";
    public OptionPanelController optionPanel;

    public GameObject failPanel;
    public GameObject successPanel;

    public PlayerHealth player;

    public MonoBehaviour[] pauseTargets;

    public PlayerInput playerInput;
    public string playerActionMapName = "Player";
    public string uiActionMapName = "UI";

    [Header("Result Rules")]
    [Tooltip("00:00 시 격추 수가 이 값 이상이면 무조건 성공")]
    public int successKillThresholdOnTimeUp = 1;

    [Header("Optional (경합 보강)")]
    public SurvivalTimer survivalTimer; 

    float _prevTimeScale = 1f;
    bool _isPaused = false;

    bool _timerElapsed = false;
    public bool IsTimerElapsed => _timerElapsed;

    bool _resultShown = false;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _prevTimeScale = 1f;
        _isPaused = false;

        if (optionPanel) optionPanel.Hide();
        if (failPanel) failPanel.SetActive(false);
        if (successPanel) successPanel.SetActive(false);
        _timerElapsed = false;
        _resultShown = false;
    }

    void Start()
    {
        if (player != null)
            player.OnDeath += ShowFailPanel; 
    }

    void OnDestroy()
    {
        if (player != null) player.OnDeath -= ShowFailPanel;
    }

    public void MarkTimerElapsed() { _timerElapsed = true; }

    public void ResetResults()
    {
        _timerElapsed = false;
        _resultShown = false;
        if (failPanel) failPanel.SetActive(false);
        if (successPanel) successPanel.SetActive(false);
    }

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

    bool ShouldForceSuccessAtTimeUp()
    {
        int kills = KillTracker.Instance ? KillTracker.Instance.KillCount : 0;

        if (_timerElapsed)                     
            return kills >= successKillThresholdOnTimeUp;

        if (survivalTimer && survivalTimer.GetRemaining() <= 0.001f)
            return kills >= successKillThresholdOnTimeUp;

        return false;
    }

    public void ShowFailPanel()
    {
        if (_resultShown) return;

        if (ShouldForceSuccessAtTimeUp())
        {
            ShowSuccessPanel(); 
            return;
        }

        PauseGame();
        if (successPanel) successPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(true);
        _resultShown = true;
    }

    public void ShowSuccessPanel()
    {
        if (_resultShown) return;

        if (!_timerElapsed)
        {
            Debug.LogWarning("[GameFlowManager] ShowSuccessPanel ignored because timer has not elapsed yet.");
            return;
        }

        PauseGame();
        if (failPanel) failPanel.SetActive(false);
        if (successPanel) successPanel.SetActive(true);
        _resultShown = true;
    }

    public void RestartGame()
    {
        if (optionPanel) optionPanel.Hide();
        ResetResults();

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
