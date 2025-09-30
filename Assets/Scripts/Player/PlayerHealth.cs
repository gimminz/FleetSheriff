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

    [Header("사망 VFX")]
    [Tooltip("플레이어 사망 시 폭발 이펙트를 띄울 Y 오프셋")]
    public float deathExplosionYOffset = 1.0f;   // ★ 추가: 자리 ‘위’로 살짝 띄우기

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
        base.OnEnable(); // isDead=false, health=maxHealth
        if (planeController) planeController.enabled = true;
        if (gun) gun.enabled = true;
        ForceRefreshUI();
    }

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
            ShieldRegenDelaySeconds = Mathf.Max(0f, ship.ShieldRegen);
        }

        maxHealth = MaxHP;   // LivingEntity와 동기화
        HP = MaxHP;
        Shield = MaxShield;
        health = HP;

        _lastHitTime = Time.time; // 초기엔 바로 재생되지 않게 하고 싶다면 -Infinity 유지도 가능
        _initialized = true;

        UpdateUI(force: true);
    }

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

        if (Shield > 0f)
        {
            float used = Mathf.Min(Shield, remain);
            Shield -= used;
            remain -= used;
        }

        if (remain > 0f)
        {
            HP = Mathf.Max(0f, HP - remain);
            health = HP; // LivingEntity와 동기화
        }

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

        bool canRegen = (Time.time - _lastHitTime) >= ShieldRegenDelaySeconds;

        if (canRegen && Shield < MaxShield && shieldRegenPerSecond > 0f)
        {
            Shield = Mathf.Min(MaxShield, Shield + shieldRegenPerSecond * Time.deltaTime);
            UpdateUI(force: false);
        }
    }

    protected override void Die()
    {
        // ★ 중복 방지 가드
        if (isDead) return;

        // 먼저 base로 isDead=true, OnDeath 이벤트 발행
        base.Die();

        // ★ 폭발 VFX (풀링, VfxManager 사용)
        if (VfxManager.Instance)
        {
            VfxManager.Instance.SpawnExplosion(transform.position, deathExplosionYOffset);
        }

        // 조작/무기 비활성화 및 UI 마무리
        if (planeController) planeController.enabled = false;
        if (gun) gun.enabled = false;

        if (hpPercentText) hpPercentText.text = "0%";
        if (shieldPercentText) shieldPercentText.text = "0%";

        // 정책에 따라 유지/비활성/파괴 택1
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
            if (shieldPercentText) shieldPercentText.text = sp + "% 방어막";
            _lastShieldPct = sp;
        }

        if (force || hp != _lastHpPct)
        {
            if (hpPercentText) hpPercentText.text = hp + "% 내구도";
            _lastHpPct = hp;
        }
    }
}
