using System.Collections.Generic;
using UnityEngine;

public class UiShipList : MonoBehaviour
{
    public Transform spaceShipListRoot;
    public UiShipExplain explain;

    private readonly List<ShipInSelectPanel> panels = new();

    private void Awake()
    {
        panels.Clear();
        panels.AddRange(spaceShipListRoot.GetComponentsInChildren<ShipInSelectPanel>(includeInactive: true));
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var table = DataTableManager.ShipTable;
        if (table == null)
        {
            Debug.LogError("ShipTable이 없습니다.");
            return;
        }

        var sorted = table.GetAllSortedByName(ascending: true, locale: "ko-KR");

        int bindCount = Mathf.Min(sorted.Count, panels.Count);
        for (int i = 0; i < bindCount; i++)
        {
            var data = sorted[i];
            panels[i].gameObject.SetActive(true);
            panels[i].SetData(data, OnSelectShip);
            panels[i].transform.SetSiblingIndex(i);
        }
        for (int i = bindCount; i < panels.Count; i++)
        {
            panels[i].gameObject.SetActive(false);
        }

        if (bindCount > 0) OnSelectShip(sorted[0]);
        else if (explain) explain.Clear();
    }

    private void OnSelectShip(ShipData data)
    {
        if (explain) explain.SetData(data);
    }
}