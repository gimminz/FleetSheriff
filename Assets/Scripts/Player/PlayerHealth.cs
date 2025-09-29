using UnityEngine;
using TMPro;

public class PlayerHealth : LivingEntity
{
    private PlaneController planeController;
    private Gun gun;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    // 마지막으로 표시한 값을 캐시해서 변경시에만 UI 갱신
    int _lastShownHP = int.MinValue;

    private void Awake()
    {
        planeController = GetComponent<PlaneController>();
        gun = GetComponent<Gun>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (planeController) planeController.enabled = true;
        if (gun) gun.enabled = true;

        ForceUpdateHealthUI();
    }

    private void Update()
    {
        // 매 프레임 현재 체력 확인 → 바뀌었으면 UI 갱신
        int hpNow = Mathf.CeilToInt(health);
        if (hpNow != _lastShownHP)
        {
            _lastShownHP = hpNow;
            if (healthText) healthText.text = $"HP: {hpNow}";
        }
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDead) return;
        base.OnDamage(damage, hitPoint, hitNormal);
        // Update()가 갱신하지만, 즉시 반영 원하면 강제 갱신 호출
        ForceUpdateHealthUI();
        Debug.Log($"[PlayerHealth] Player took {damage} damage. Current HP: {health}");
    }

    public override void OnDamage()
    {
        if (isDead) return;
        base.OnDamage();
        ForceUpdateHealthUI();
        Debug.Log($"[PlayerHealth] Player took default damage. Current HP: {health}");
    }

    protected override void Die()
    {
        base.Die();

        if (planeController) planeController.enabled = false;
        if (gun) gun.enabled = false;

        // 죽었을 때 표시를 바꾸고 싶으면 아래 라인 유지
        if (healthText) healthText.text = $"HP: 0";

        gameObject.SetActive(false);
        Debug.Log("[PlayerHealth] Player died.");
    }

    public float GetCurrentHealth() => health;

    private void ForceUpdateHealthUI()
    {
        _lastShownHP = int.MinValue; // 다음 Update()에서 강제로 갱신되도록 리셋
    }
}
