using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ShipInSelectPanel : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Button button;

    private ShipData data;
    private Action<ShipData> onSelect;

    public void SetData(ShipData data, Action<ShipData> onSelect)
    {
        this.data = data;
        this.onSelect = onSelect;

        if (nameText) nameText.text = data.ShipName;

        if (button == null) button = GetComponent<Button>();
        if (button)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
    }
    private void HandleClick()
    {
        onSelect?.Invoke(data);
    }
}