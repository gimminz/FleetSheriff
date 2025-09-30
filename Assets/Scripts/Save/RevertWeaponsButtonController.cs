using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class RevertWeaponsButtonController : MonoBehaviour
{
    public SelectedShipContext selectedShipContext;
    public UiShipWeaponsController weaponsUI; 

    public void OnClickRevertWeapons()
    {
        if (selectedShipContext == null)
        {
            Debug.LogError("[RevertWeapons] SelectedShipContext가 없습니다.");
            return;
        }
        if (!selectedShipContext.ActiveShipId.HasValue)
        {
            Debug.LogWarning("[RevertWeapons] ActiveShipId가 없습니다. 우선 우주선을 선택하세요.");
            return;
        }

        var model = TryLoadExisting();
        if (model == null)
        {
            Debug.LogWarning("[RevertWeapons] 저장 파일이 없습니다.");
            return;
        }

        var shipId = selectedShipContext.ActiveShipId.Value;
        if (!model.Ships.TryGetValue(shipId, out var entry) || entry.Weapons == null)
        {
            Debug.Log("[RevertWeapons] 해당 우주선의 무장 저장 정보가 없습니다.");
            return;
        }

        foreach (var kv in entry.Weapons)
        {
            if (kv.Key == ShipSlot.Down) continue;
            selectedShipContext.SetWeapon(shipId, kv.Key, kv.Value);
        }

        // 2) UI 반영
        weaponsUI?.ApplyWeaponsByIds(entry.Weapons);
        Debug.Log($"[RevertWeapons] ship {shipId} 무장 되돌리기 완료.");
    }

    private static LoadoutSaveModel TryLoadExisting()
    {
        try
        {
            if (!System.IO.File.Exists(SavePath.LoadoutJsonPath)) return null;
            var json = System.IO.File.ReadAllText(SavePath.LoadoutJsonPath);
            return JsonConvert.DeserializeObject<LoadoutSaveModel>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[RevertWeapons] 파일 로드 실패: {ex.Message}");
            return null;
        }
    }
}
