using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("UI/UiWeaponExplain")]
public class UiWeaponExplain : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI hardpointText;
    public TextMeshProUGUI unlockLvText;
    public TextMeshProUGUI preconditionText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI addressText;  
    public TextMeshProUGUI keyText;   
    public void SetData(WeaponData d)
    {
        if (d == null) { Clear(); return; }

        if (nameText) nameText.text = string.IsNullOrWhiteSpace(d.DisplayName) ? (d.WeaponKey ?? $"-") : d.DisplayName;
        if (categoryText) categoryText.text = d.Category.ToString();
        if (hardpointText) hardpointText.text = d.Hardpoint.ToString();
        if (unlockLvText) unlockLvText.text = d.UnlockLv.HasValue ? d.UnlockLv.Value.ToString() : "-";
        if (preconditionText) preconditionText.text = string.IsNullOrWhiteSpace(d.UnlockPrecondition) ? "-" : d.UnlockPrecondition;
        if (costText) costText.text = d.Cost.HasValue ? d.Cost.Value.ToString("N0") : "-";
        if (descText) descText.text = string.IsNullOrWhiteSpace(d.ItemDescription) ? "-" : d.ItemDescription;
        if (addressText) addressText.text = string.IsNullOrWhiteSpace(d.WeaponAddress) ? "-" : d.WeaponAddress;
        if (keyText) keyText.text = string.IsNullOrWhiteSpace(d.WeaponKey) ? "-" : d.WeaponKey;
    }

    public void Clear()
    {
        if (nameText) nameText.text = "";
        if (categoryText) categoryText.text = "";
        if (hardpointText) hardpointText.text = "";
        if (unlockLvText) unlockLvText.text = "";
        if (preconditionText) preconditionText.text = "";
        if (costText) costText.text = "";
        if (descText) descText.text = "";

        if (addressText) addressText.text = "";
        if (keyText) keyText.text = "";
    }
}
