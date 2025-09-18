using TMPro;
using UnityEngine;

public class UiPartExplain : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    public void SetData(PartData d)
    {
        if (nameText) nameText.text = d?.ItemDisplayName ?? "";
        if (descText) descText.text = d == null ? "" : (string.IsNullOrWhiteSpace(d.ItemDescription) ? "-" : d.ItemDescription);
    }

    public void Clear()
    {
        if (nameText) nameText.text = "";
        if (descText) descText.text = "";
    }
}
