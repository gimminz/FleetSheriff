using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartInListPanel : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Button button;

    private PartData data;
    private Action<PartData> onSelect;

    public void SetData(PartData d, Action<PartData> onSelect)
    {
        data = d;
        this.onSelect = onSelect;

        if (nameText) nameText.text = d?.ItemDisplayName ?? "-";

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
