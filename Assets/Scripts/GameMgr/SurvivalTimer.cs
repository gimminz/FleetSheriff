using TMPro;
using UnityEngine;

public class SurvivalTimer : MonoBehaviour
{
    public PlayerHealth player;
    public TextMeshProUGUI timerText;

    public float durationSeconds = 120f;

    private float timeLeft;
    private bool running;

    private void Start()
    {
        StartTimer();
    }

    public void StartTimer()
    {
        timeLeft = durationSeconds;
        running = true;
        UpdateText(timeLeft);
    }

    public void StopTimer() => running = false;

    private void Update()
    {
        if (!running) return;

        if (player != null && player.isDead)
        {
            running = false;
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running = false;

            bool alive = (player != null && !player.isDead);
            int kills = KillTracker.Instance ? KillTracker.Instance.KillCount : 0;

            if (alive && kills >= 1)
            {
                if (GameFlowManager.Instance) GameFlowManager.Instance.ShowSuccessPanel();
            }
            else
            {
                if (alive && player != null)
                {
                    player.OnDamage(player.GetCurrentHealth(), Vector3.zero, Vector3.zero);
                }
                else
                {
                    if (GameFlowManager.Instance) GameFlowManager.Instance.ShowFailPanel();
                }
            }

            UpdateText(timeLeft);
            return;
        }

        UpdateText(timeLeft);
    }

    private void UpdateText(float t)
    {
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        if (timerText) timerText.text = $"{m:00}:{s:00}";
    }
}
