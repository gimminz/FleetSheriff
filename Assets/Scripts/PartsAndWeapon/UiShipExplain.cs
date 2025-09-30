using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiShipExplain : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI shieldText;
    public TextMeshProUGUI shieldRegenText;
    public TextMeshProUGUI maxSpeedText;
    public TextMeshProUGUI searchRangeText;
    public TextMeshProUGUI maxTargetingText;
    public TextMeshProUGUI unlockLvText;
    public TextMeshProUGUI preconditionText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descText;

    public void SetData(ShipData d)
    {
        if (d == null) { Clear(); return; }

        if (nameText) nameText.text = d.ShipName;
        if (hpText) hpText.text = d.ShipHP.ToString("N0");
        if (shieldText) shieldText.text = d.ShipShield.ToString("N0");
        if (shieldRegenText) shieldRegenText.text = d.ShieldRegen.ToString("N1");
        if (maxSpeedText) maxSpeedText.text = d.ShipMaxSpeed.ToString("N0");
        if (searchRangeText) searchRangeText.text = d.MaxSearchRange.ToString("N0");
        if (maxTargetingText) maxTargetingText.text = d.MaxTargeting.ToString();
        if (unlockLvText) unlockLvText.text = d.UnlockLv.HasValue ? d.UnlockLv.Value.ToString() : "-";
        if (preconditionText) preconditionText.text = string.IsNullOrWhiteSpace(d.UnlockPrecondition) ? "-" : d.UnlockPrecondition;
        if (costText) costText.text = d.BuyCost.HasValue ? d.BuyCost.Value.ToString("N0") : "-";
        if (descText) descText.text = string.IsNullOrWhiteSpace(d.ShipDescription) ? "-" : d.ShipDescription;
    }

    public void Clear()
    {
        if (nameText) nameText.text = "";
        if (hpText) hpText.text = "";
        if (shieldText) shieldText.text = "";
        if (shieldRegenText) shieldRegenText.text = "";
        if (maxSpeedText) maxSpeedText.text = "";
        if (searchRangeText) searchRangeText.text = "";
        if (maxTargetingText) maxTargetingText.text = "";
        if (unlockLvText) unlockLvText.text = "";
        if (preconditionText) preconditionText.text = "";
        if (costText) costText.text = "";
        if (descText) descText.text = "";
    }
}
