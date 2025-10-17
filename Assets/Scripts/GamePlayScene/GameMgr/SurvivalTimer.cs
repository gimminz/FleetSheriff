using TMPro;
using UnityEngine;

public class SurvivalTimer : MonoBehaviour
{
    [Header("Refs")]
    public PlayerHealth player;
    public TextMeshProUGUI timerText;

    [Header("Settings")]
    public float durationSeconds = 120f;
    public bool autoStart = true;
    public bool useUnscaledTime = false;

    [Header("Rule")]
    [Tooltip("시간 00:00 시, 이 값 이상 격추면 무조건 성공")]
    public int successKillThreshold = 1;

    float timeLeft;
    bool running;
    bool _fired; // 종료 이벤트 단 1회

    void Start()
    {
        if (autoStart) StartTimer();
        else { timeLeft = Mathf.Max(0f, durationSeconds); UpdateText(timeLeft); }
    }

    public void StartTimer()
    {
        timeLeft = Mathf.Max(0f, durationSeconds);
        running = true;
        _fired = false;
        UpdateText(timeLeft);
    }

    public void StopTimer() => running = false;

    void Update()
    {
        if (!running || _fired) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        timeLeft -= dt;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running = false;
            if (_fired) return;
            _fired = true;

            // 00:00 표시 보장
            UpdateText(timeLeft);

            // ★ 타이머 종료 플래그를 먼저 올린다
            if (GameFlowManager.Instance)
                GameFlowManager.Instance.MarkTimerElapsed();

            // ★ 이후 성공/실패 판정 - 성공 우선!
            int kills = KillTracker.Instance ? KillTracker.Instance.KillCount : 0;

            Debug.Log($"[SurvivalTimer] Timer ended. Kills: {kills}, Threshold: {successKillThreshold}");

            if (kills >= successKillThreshold)
            {
                Debug.Log("[SurvivalTimer] Success condition met! Calling ShowSuccessPanel.");
                if (GameFlowManager.Instance)
                    GameFlowManager.Instance.ShowSuccessPanel();
            }
            else
            {
                Debug.Log("[SurvivalTimer] Success condition not met. Calling ShowFailPanel.");
                if (GameFlowManager.Instance)
                    GameFlowManager.Instance.ShowFailPanel();
            }

            return;
        }

        UpdateText(timeLeft);
    }

    void UpdateText(float t)
    {
        int total = Mathf.CeilToInt(t);
        int m = total / 60;
        int s = total % 60;
        if (timerText) timerText.text = $"{m:00}:{s:00}";
    }

    // ★ 경합 프레임 보강용: GFM에서 남은 시간을 직접 확인
    public float GetRemaining() => timeLeft;
}
