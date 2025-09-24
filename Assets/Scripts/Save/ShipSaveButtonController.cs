using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SaveButtonController : MonoBehaviour
{
    public SelectedShipContext selectedShipContext;

    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented
    };

    public void OnClickSave()
    {
        if (selectedShipContext == null)
        {
            Debug.LogError("[Save] SelectedShipContext가 지정되지 않았습니다.");
            return;
        }

        var all = selectedShipContext.GetAllSelections();
        if (all == null || all.Count == 0)
        {
            Debug.LogWarning("[Save] 저장할 선택 정보가 없습니다.");
            return;
        }

        var model = BuildModel(all);

        try
        {
            SavePath.EnsureDir();
            var json = JsonConvert.SerializeObject(model, JsonSettings);
            System.IO.File.WriteAllText(SavePath.LoadoutJsonPath, json);
            Debug.Log($"[Save] 저장 완료: {SavePath.LoadoutJsonPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Save] 저장 실패: {ex.Message}");
        }
    }

    private static LoadoutSaveModel BuildModel(
        Dictionary<int, Dictionary<ShipSlot, int?>> byShip)
    {
        var model = new LoadoutSaveModel();

        foreach (var shipKv in byShip)
        {
            var shipId = shipKv.Key;
            var parts = shipKv.Value;

            var entry = new ShipLoadout
            {
                ShipKey = shipId,
                Parts = new Dictionary<ShipSlot, int?>()
            };

            entry.Parts[ShipSlot.Up] = parts != null && parts.TryGetValue(ShipSlot.Up, out var up) ? up : null;
            entry.Parts[ShipSlot.Left] = parts != null && parts.TryGetValue(ShipSlot.Left, out var left) ? left : null;
            entry.Parts[ShipSlot.Right] = parts != null && parts.TryGetValue(ShipSlot.Right, out var right) ? right : null;
            entry.Parts[ShipSlot.Down] = parts != null && parts.TryGetValue(ShipSlot.Down, out var down) ? down : null;

            model.Ships[shipId] = entry;
        }

        return model;
    }
}
