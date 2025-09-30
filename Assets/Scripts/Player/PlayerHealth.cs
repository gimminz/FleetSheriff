using TMPro;
using UnityEngine;

public class PlayerHealth : LivingEntity
{
    private PlaneController planeController;
    private Gun gun;

    [Header("UI (정수 %로 표시)")]
    public TextMeshProUGUI shieldPercentText; // 방어막 % 텍스트
    public TextMeshProUGUI hpPercentText;     // 내구도 % 텍스트

    [Header("재생 설정")]
    [Tooltip("방어막 초당 회복량(Ship CSV의 Shield-regen은 '피격 후 지연시간(초)'로 사용)")]
    public float shieldRegenPerSecond = 30f;

    // Ship 데이터로부터 설정되는 최대치들
    public float MaxHP { get; private set; }
    public float MaxShield { get; private set; }
    public float ShieldRegenDelaySeconds { get; private set; } // CSV Shield-regen(초)

    // 현재값
    public float HP { get; private set; }
    public float Shield { get; private set; }

    // 내부 상태
    float _lastHitTime = float.NegativeInfinity;
    bool _initialized;

    // UI 캐시(값이 바뀔 때만 갱신)
    int _lastShieldPct = int.MinValue;
    int _lastHpPct = int.MinValue;

    private void Awake()
    {
        planeController = GetComponent<PlaneController>();
        gun = GetComponent<Gun>();
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // 여기서 isDead=false, health=maxHealth 로 초기화됨
        if (planeController) planeController.enabled = true;
        if (gun) gun.enabled = true;
        ForceRefreshUI();
    }

    /// <summary>
    /// 씬 진입 시점에 선택된 shipId로 ShipTable을 읽어 초기화하세요.
    /// (출동 버튼 → 다음 씬에서 호출)
    /// </summary>
    public void InitializeFromShipId(int shipId)
    {
        var ship = DataTableManager.ShipTable?.Get(shipId);
        if (ship == null)
        {
            Debug.LogError($"[PlayerHealth] ShipTable에서 ID {shipId}를 찾지 못했습니다. 안전한 기본값으로 초기화합니다.");
            MaxHP = 100f;
            MaxShield = 0f;
            ShieldRegenDelaySeconds = 0f; // 지연 없음
        }
        else
        {
            MaxHP = Mathf.Max(0f, ship.ShipHP);
            MaxShield = Mathf.Max(0f, ship.ShipShield);
            // CSV의 Shield-regen 값을 '피격 후 재생 지연(초)'로 사용
            ShieldRegenDelaySeconds = Mathf.Max(0f, ship.ShieldRegen);
        }

        // LivingEntity와 값 일치(다른 시스템이 maxHealth/health를 볼 수 있음)
        maxHealth = MaxHP;
        HP = MaxHP;
        Shield = MaxShield;
        health = HP; // base.health 동기화

        _lastHitTime = float.NegativeInfinity;
        _initialized = true;

        UpdateUI(force: true);
    }

    /// <summary>
    /// 데미지 처리: 방어막 → 내구도 순서로 소모.
    /// LivingEntity의 base.OnDamage는 호출하지 않습니다(중복 차감 방지).
    /// </summary>
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        ApplyDamageInternal(damage);
        Debug.Log($"[PlayerHealth] Took {damage} dmg. HP:{HP}/{MaxHP}, Shield:{Shield}/{MaxShield}");
    }

    public override void OnDamage()
    {
        ApplyDamageInternal(1f);
        Debug.Log($"[PlayerHealth] Took 1 dmg. HP:{HP}/{MaxHP}, Shield:{Shield}/{MaxShield}");
    }

    void ApplyDamageInternal(float damage)
    {
        if (!_initialized || isDead || damage <= 0f) return;

        float remain = damage;

        // 방어막에서 먼저 흡수
        if (Shield > 0f)
        {
            float used = Mathf.Min(Shield, remain);
            Shield -= used;
            remain -= used;
        }

        // 남은 데미지가 있으면 HP 차감
        if (remain > 0f)
        {
            HP = Mathf.Max(0f, HP - remain);
            health = HP; // LivingEntity와 동기화
        }

        // 피격 시각 기록 → 재생 지연에 사용
        _lastHitTime = Time.time;

        UpdateUI(force: false);

        if (HP <= 0f && !isDead)
        {
            Die();
        }
    }

    private void Update()
    {
        if (!_initialized || isDead) return;

        // 피격 후 ShieldRegenDelaySeconds가 지나야 재생 시작
        bool canRegen = (Time.time - _lastHitTime) >= ShieldRegenDelaySeconds;

        if (canRegen && Shield < MaxShield && shieldRegenPerSecond > 0f)
        {
            Shield = Mathf.Min(MaxShield, Shield + shieldRegenPerSecond * Time.deltaTime);
            UpdateUI(force: false);
        }
    }

    protected override void Die()
    {
        base.Die(); // 여기서 isDead=true, OnDeath 이벤트 발생

        if (planeController) planeController.enabled = false;
        if (gun) gun.enabled = false;

        // UI 정리
        if (hpPercentText) hpPercentText.text = "0%";
        if (shieldPercentText) shieldPercentText.text = "0%";

        gameObject.SetActive(false);
        Debug.Log("[PlayerHealth] Player died.");
    }

    public float GetCurrentHealth() => HP;

    void ForceRefreshUI()
    {
        _lastShieldPct = int.MinValue;
        _lastHpPct = int.MinValue;
    }

    void UpdateUI(bool force)
    {
        int sp = (MaxShield > 0f) ? Mathf.Clamp(Mathf.RoundToInt(Shield / MaxShield * 100f), 0, 100) : 0;
        int hp = (MaxHP > 0f) ? Mathf.Clamp(Mathf.RoundToInt(HP / MaxHP * 100f), 0, 100) : 0;

        if (force || sp != _lastShieldPct)
        {
            if (shieldPercentText) shieldPercentText.text = sp.ToString() + "% 방어막";
            _lastShieldPct = sp;
        }

        if (force || hp != _lastHpPct)
        {
            if (hpPercentText) hpPercentText.text = hp.ToString() + "% 내구도";
            _lastHpPct = hp;
        }
    }
}
