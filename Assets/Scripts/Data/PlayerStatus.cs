using UnityEngine;
using TMPro;

public class PlayerStatus : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI goldText;

    private void OnEnable()
    {
        Refresh();
    }
    public void Refresh()
    {
        var table = DataTableManager.PlayerStatusTable;

        var d = table.data;
        levelText.text = $"Lv : {d.Level}";
        expText.text = $"EXP : {d.EXP}";
        goldText.text = $"GOLD : {d.Gold}";
    }
}
