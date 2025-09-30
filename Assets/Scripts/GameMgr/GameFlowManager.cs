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

    public void MarkTimerElapsed()
    {
        _timerElapsed = true;
        Debug.Log("[GameFlowManager] Timer elapsed marked.");
    }

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

    /// <summary>
    /// 타이머 종료 시 성공 조건 체크
    /// </summary>
    bool ShouldForceSuccessAtTimeUp()
    {
        int kills = KillTracker.Instance ? KillTracker.Instance.KillCount : 0;

        // 타이머가 종료되었고, 킬 수가 기준 이상이면 성공
        if (_timerElapsed)
            return kills >= successKillThresholdOnTimeUp;

        // 경합 프레임 보강: SurvivalTimer에서 직접 남은 시간 확인
        if (survivalTimer && survivalTimer.GetRemaining() <= 0.001f)
            return kills >= successKillThresholdOnTimeUp;

        return false;
    }

    /// <summary>
    /// Fail Panel 표시 (플레이어 사망 시 호출됨)
    /// ★ 타이머 종료 후 성공 조건을 만족하면 Success Panel로 전환
    /// </summary>
    public void ShowFailPanel()
    {
        if (_resultShown)
        {
            Debug.Log("[GameFlowManager] Result already shown. Ignoring ShowFailPanel.");
            return;
        }

        // ★ 중요: 타이머가 종료되었고 킬 수 조건을 만족하면 성공으로 전환
        if (ShouldForceSuccessAtTimeUp())
        {
            Debug.Log("[GameFlowManager] Timer elapsed with sufficient kills. Switching to Success.");
            ShowSuccessPanel();
            return;
        }

        // 일반 실패 처리
        Debug.Log("[GameFlowManager] Showing Fail Panel.");
        PauseGame();
        if (successPanel) successPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(true);
        _resultShown = true;
    }

    /// <summary>
    /// Success Panel 표시 (타이머 종료 시 호출됨)
    /// </summary>
    public void ShowSuccessPanel()
    {
        if (_resultShown)
        {
            Debug.Log("[GameFlowManager] Result already shown. Ignoring ShowSuccessPanel.");
            return;
        }

        // ★ 타이머 종료 체크를 완화: 경합 프레임을 고려하여 남은 시간도 확인
        bool timerReady = _timerElapsed ||
                         (survivalTimer && survivalTimer.GetRemaining() <= 0.001f);

        if (!timerReady)
        {
            Debug.LogWarning("[GameFlowManager] ShowSuccessPanel ignored because timer has not elapsed yet.");
            return;
        }

        // 성공 처리
        Debug.Log("[GameFlowManager] Showing Success Panel.");
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