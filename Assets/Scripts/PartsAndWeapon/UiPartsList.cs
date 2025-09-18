using System.Collections.Generic;
using UnityEngine;

public enum ShipSlot { Up, Left, Right, Down }

public class UiPartsList : MonoBehaviour
{
    public Transform listRoot;
    public PartInListPanel itemPrefab; 

    private readonly List<PartInListPanel> panels = new();

    private System.Action<PartData> onSelect;

    private void Awake()
    {
        if (listRoot == null) listRoot = transform;
        panels.AddRange(listRoot.GetComponentsInChildren<PartInListPanel>(true));
    }

    public void Show(ShipSlot slot, System.Action<PartData> onSelect)
    {
        this.onSelect = onSelect;

        int hardpoint = slot switch
        {
            ShipSlot.Up => 1,
            ShipSlot.Left => 2,
            ShipSlot.Right => 2,
            ShipSlot.Down => 3,
            _ => 1
        };

        var table = DataTableManager.PartTable;
        var list = table.GetByHardpoint(hardpoint, true, "ko-KR");

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
