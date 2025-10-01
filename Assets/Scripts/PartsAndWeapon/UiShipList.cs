using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

public class UiShipList : MonoBehaviour
{
    public Transform spaceShipListRoot;
    public UiShipExplain explain;                 // 설명 패널(텍스트)
    public UiShipCenterDisplay centerDisplay;    
    public SelectedShipContext selectedShipContext;
    public UiShipPartsController shipPartsController;

    private readonly List<ShipInSelectPanel> panels = new();

    private void Awake()
    {
        panels.Clear();
        if (spaceShipListRoot)
            panels.AddRange(spaceShipListRoot.GetComponentsInChildren<ShipInSelectPanel>(includeInactive: true));
    }

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        var table = DataTableManager.ShipTable;
        if (table == null)
        {
            Debug.LogError("ShipTable이 없습니다.");
            return;
        }

        var all = table.GetAll();
        var comp = System.StringComparer.Create(new CultureInfo("ko-KR"), ignoreCase: true);

        var sorted = all
            .OrderByDescending(s =>
                string.IsNullOrWhiteSpace(s.KoreanName) ? s.ShipName : s.KoreanName,
                comp)
            .ToList();

        int bindCount = Mathf.Min(sorted.Count, panels.Count);
        for (int i = 0; i < bindCount; i++)
        {
            var data = sorted[i];
            panels[i].gameObject.SetActive(true);
            panels[i].SetData(data, OnSelectShip);  
            panels[i].transform.SetSiblingIndex(i);
        }
        for (int i = bindCount; i < panels.Count; i++)
            panels[i].gameObject.SetActive(false);

        if (bindCount > 0) OnSelectShip(sorted[0]);
        else
        {
            explain?.Clear();
            centerDisplay?.Clear();
        }
    }

    private void OnSelectShip(ShipData data)
    {
        explain?.SetData(data);          
        centerDisplay?.SetData(data);    

        selectedShipContext?.SetActiveShip(data.Id);

        if (selectedShipContext != null && shipPartsController != null)
        {
            if (selectedShipContext.TryGetParts(data.Id, out var partsMap))
                shipPartsController.ApplyPartsByIds(partsMap);
            else
                shipPartsController.ApplyPartsByIds(null);
        }
    }
}
