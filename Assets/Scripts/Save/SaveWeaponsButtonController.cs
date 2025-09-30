using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SaveWeaponsButtonController : MonoBehaviour
{
    public SelectedShipContext selectedShipContext;

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.Indented
    };

    public void OnClickSaveWeapons()
    {
        if (selectedShipContext == null)
        {
            Debug.LogError("[SaveWeapons] SelectedShipContext가 없습니다.");
            return;
        }

        var wAll = selectedShipContext.GetAllWeaponSelections();
        if (wAll == null || wAll.Count == 0)
        {
            Debug.LogWarning("[SaveWeapons] 저장할 무장 선택이 없습니다.");
            return;
        }

        var model = TryLoadExisting() ?? new LoadoutSaveModel();

        MergeWeapons(model, wAll);

        model.Version = Mathf.Max(model.Version, 2);

        try
        {
            SavePath.EnsureDir();
            var json = JsonConvert.SerializeObject(model, JsonSettings);
            System.IO.File.WriteAllText(SavePath.LoadoutJsonPath, json);
            Debug.Log($"[SaveWeapons] 저장 완료: {SavePath.LoadoutJsonPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveWeapons] 저장 실패: {ex.Message}");
        }
    }

    private static LoadoutSaveModel TryLoadExisting()
    {
        try
        {
            if (!System.IO.File.Exists(SavePath.LoadoutJsonPath)) return null;
            var json = System.IO.File.ReadAllText(SavePath.LoadoutJsonPath);
            var m = JsonConvert.DeserializeObject<LoadoutSaveModel>(json) ?? new LoadoutSaveModel();
            return m;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SaveWeapons] 기존 파일 로드 실패(무시하고 신규 생성): {ex.Message}");
            return null;
        }
    }

    private static void MergeWeapons(LoadoutSaveModel model, Dictionary<int, Dictionary<ShipSlot, int?>> byShipWeapons)
    {
        foreach (var shipKv in byShipWeapons)
        {
            var shipId = shipKv.Key;
            var map = shipKv.Value;

            if (!model.Ships.TryGetValue(shipId, out var entry))
            {
                entry = new ShipLoadout { ShipKey = shipId };
                model.Ships[shipId] = entry;
            }

            entry.Weapons ??= new Dictionary<ShipSlot, int?>();

            foreach (var kv in map)
            {
                var slot = kv.Key;
                if (slot == ShipSlot.Down) continue; 
                entry.Weapons[slot] = kv.Value;    
            }
        }
    }
}
