using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SurvivalTimer : MonoBehaviour
{
    public PlayerHealth player;            
    public TextMeshProUGUI timerText;  

    public float durationSeconds = 120f;   

    public UnityEvent OnSurvived;       
    public UnityEvent OnFailed;        

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
            OnFailed?.Invoke();
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running = false;

            if (player != null && !player.isDead)
            {
                player.OnDamage(player.GetCurrentHealth(), Vector3.zero, Vector3.zero);
            }

            OnSurvived?.Invoke();
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
