using TMPro;
using UnityEngine;

public class UiShipWeaponsController : MonoBehaviour
{
    public SelectedShipContext selectedShipContext; 

    public GameObject spaceShipListContainer;
    public GameObject weaponListContainer;

    public UiWeaponsList weaponsList;
    public UiWeaponExplain explain;

    public TextMeshProUGUI upAddressText;
    public TextMeshProUGUI leftAddressText;
    public TextMeshProUGUI rightAddressText;

    private ShipSlot currentSlot = ShipSlot.Up;

    private void Start()
    {
        SetModeShipList();
    }

    public void OnClickUpPanel() => OpenWeaponsForSlot(ShipSlot.Up);
    public void OnClickLeftPanel() => OpenWeaponsForSlot(ShipSlot.Left);
    public void OnClickRightPanel() => OpenWeaponsForSlot(ShipSlot.Right);

    private void OpenWeaponsForSlot(ShipSlot slot)
    {
        currentSlot = slot;
        SetModeWeaponList();
        weaponsList.Show(currentSlot, OnSelectWeapon);
        explain?.Clear();
    }

    private void OnSelectWeapon(WeaponData w)
    {
        if (w == null) return;

        explain?.SetData(w);

        var target = GetAddressText(currentSlot);
        if (target)
            target.text = string.IsNullOrWhiteSpace(w.WeaponAddress) ? "-" : w.WeaponAddress;

        if (selectedShipContext != null && selectedShipContext.ActiveShipId.HasValue)
            selectedShipContext.SetWeapon(selectedShipContext.ActiveShipId.Value, currentSlot, w.Id);
    }

    public void OnClickShipPanel()
    {
        SetModeShipList();
        if (spaceShipListContainer)
        {
            var shipList = spaceShipListContainer.GetComponentInChildren<UiShipList>(true);
            shipList?.Refresh();
        }
    }


    public void ApplyWeaponToSlot(ShipSlot slot, WeaponData weapon)
    {
        if (weapon != null) explain?.SetData(weapon);

        var target = GetAddressText(slot);
        if (target)
            target.text = string.IsNullOrWhiteSpace(weapon?.WeaponAddress) ? "-" : weapon.WeaponAddress;
    }

    public void ApplyWeaponsByIds(System.Collections.Generic.Dictionary<ShipSlot, int?> weapons)
    {
        var weaponTable = DataTableManager.WeaponTable;
        if (weapons == null || weaponTable == null) return;

        foreach (var kv in weapons)
        {
            var slot = kv.Key;
            if (slot == ShipSlot.Down) continue;

            var id = kv.Value;
            WeaponData w = (id.HasValue) ? weaponTable.Get(id.Value) : null;
            ApplyWeaponToSlot(slot, w);
        }
    }

    private void SetModeShipList()
    {
        if (spaceShipListContainer) spaceShipListContainer.SetActive(true);
        if (weaponListContainer) weaponListContainer.SetActive(false);
    }

    private void SetModeWeaponList()
    {
        if (spaceShipListContainer) spaceShipListContainer.SetActive(false);
        if (weaponListContainer) weaponListContainer.SetActive(true);
    }

    private TextMeshProUGUI GetAddressText(ShipSlot slot)
    {
        return slot switch
        {
            ShipSlot.Up => upAddressText,
            ShipSlot.Left => leftAddressText,
            ShipSlot.Right => rightAddressText,
            _ => null
        };
    }
}
