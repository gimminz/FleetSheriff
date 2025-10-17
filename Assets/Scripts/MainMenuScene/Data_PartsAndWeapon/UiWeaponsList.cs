using System.Collections.Generic;
using UnityEngine;

public class UiWeaponsList : MonoBehaviour
{
    public Transform listRoot;  
    public WeaponInListPanel itemPrefab; 

    private readonly List<WeaponInListPanel> panels = new();
    private System.Action<WeaponData> onSelect;

    private void Awake()
    {
        if (!listRoot) listRoot = transform;
        panels.AddRange(listRoot.GetComponentsInChildren<WeaponInListPanel>(true));
    }

    public void Show(ShipSlot slot, System.Action<WeaponData> onSelect)
    {
        this.onSelect = onSelect;

        int hardpoint = slot switch
        {
            ShipSlot.Up => 1,
            ShipSlot.Left => 2,
            ShipSlot.Right => 2,
            _ => 1
        };

        var table = DataTableManager.WeaponTable;
        var list = table.GetByCategoryAndHardpoint(category: 0, hardpoint: hardpoint, ascending: true, locale: "ko-KR");

        EnsurePanelCount(list.Count);

        for (int i = 0; i < panels.Count; i++)
        {
            if (i < list.Count)
            {
                panels[i].gameObject.SetActive(true);
                panels[i].SetData(list[i], this.onSelect);
                panels[i].transform.SetSiblingIndex(i);
            }
            else
            {
                panels[i].SetEmpty();
                panels[i].gameObject.SetActive(false);
            }
        }
    }

    private void EnsurePanelCount(int count)
    {
        if (itemPrefab == null) return;
        while (panels.Count < count)
        {
            var p = GameObject.Instantiate(itemPrefab, listRoot);
            p.gameObject.SetActive(false);
            panels.Add(p);
        }
    }
}
