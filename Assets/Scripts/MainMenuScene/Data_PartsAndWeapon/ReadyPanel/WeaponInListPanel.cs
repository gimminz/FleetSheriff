using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInListPanel : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Button button;

    private WeaponData data;
    private Action<WeaponData> onSelect;

    public void SetData(WeaponData d, Action<WeaponData> onSelect)
    {
        data = d;
        this.onSelect = onSelect;

        if (nameText) nameText.text = d?.DisplayName ?? "-";

        if (!button) button = GetComponent<Button>();
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => this.onSelect?.Invoke(data));
        }
    }

    public void SetEmpty()
    {
        data = null;
        if (nameText) nameText.text = "";
        if (button) button.onClick.RemoveAllListeners();
    }
}
