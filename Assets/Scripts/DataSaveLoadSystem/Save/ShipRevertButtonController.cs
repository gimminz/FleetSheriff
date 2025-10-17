// Assets/Scripts/Save/RevertButtonController.cs
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class RevertButtonController : MonoBehaviour
{
    public SelectedShipContext selectedShipContext;
    public UiShipPartsController shipPartsController;

    public void OnClickRevert()
    {
        if (!System.IO.File.Exists(SavePath.LoadoutJsonPath))
        {
            Debug.LogWarning("[Revert] 저장본이 없습니다.");
            return;
        }

        LoadoutSaveModel model = null;
        try
        {
            var json = System.IO.File.ReadAllText(SavePath.LoadoutJsonPath);
            model = JsonConvert.DeserializeObject<LoadoutSaveModel>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Revert] 저장본 파싱 실패: {ex.Message}");
            return;
        }

        if (model == null || model.Ships == null || model.Ships.Count == 0)
        {
            Debug.LogWarning("[Revert] 저장 데이터가 비어 있습니다.");
            return;
        }

        if (selectedShipContext == null)
        {
            Debug.LogError("[Revert] SelectedShipContext가 설정되지 않았습니다.");
            return;
        }

        selectedShipContext.ClearAll();

        foreach (var shipEntry in model.Ships)
        {
            var shipId = shipEntry.Key;
            var loadout = shipEntry.Value;
            selectedShipContext.SetActiveShip(shipId);

            if (loadout?.Parts != null)
            {
                foreach (var kv in loadout.Parts)
                {
                    selectedShipContext.SetPart(shipId, kv.Key, kv.Value);
                }
            }
        }

        if (!selectedShipContext.ActiveShipId.HasValue)
        {
            var firstShipId = model.Ships.Keys.FirstOrDefault();
            selectedShipContext.SetActiveShip(firstShipId);
        }

        var activeShipId = selectedShipContext.ActiveShipId.Value;

        if (shipPartsController == null)
        {
            Debug.LogWarning("[Revert] UiShipPartsController가 연결되지 않아 UI 반영을 생략합니다.");
        }
        else
        {
            var parts = model.Ships.TryGetValue(activeShipId, out var activeLoadout)
                ? activeLoadout?.Parts
                : null;

            shipPartsController.ApplyPartsByIds(parts);
        }

        Debug.Log("[Revert] 마지막 저장본으로 되돌렸습니다.");
    }
}
