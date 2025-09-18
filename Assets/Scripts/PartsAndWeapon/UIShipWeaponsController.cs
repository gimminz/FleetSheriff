using TMPro;
using UnityEngine;

public class UiShipWeaponsController : MonoBehaviour
{
    public GameObject spaceShipListContainer;
    public GameObject weaponListContainer;

    public UiWeaponsList weaponsList;
    public UiPartExplain explain;

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

        if (explain)
        {
            explain.SetData(new PartData
            {
                ItemDisplayName = w.DisplayName,
                ItemDescription = w.ItemDescription
            });
        }
        var target = GetAddressText(currentSlot);
        if (target)
            target.text = string.IsNullOrWhiteSpace(w.WeaponAddress) ? "-" : w.WeaponAddress;
    }

    private void SetModeShipList()
    {
        if (spaceShipListContainer) spaceShipListContainer.SetActive(true);
        if (weaponListContainer) weaponListContainer.SetActive(false);
        Debug.Log($"ShipList ON ({spaceShipListContainer.name}), WeaponList OFF ({weaponListContainer.name})");
    }

    private void SetModeWeaponList()
    {
        if (spaceShipListContainer) spaceShipListContainer.SetActive(false);
        if (weaponListContainer) weaponListContainer.SetActive(true);
        Debug.Log($"ShipList OFF ({spaceShipListContainer.name}), WeaponList ON ({weaponListContainer.name})");
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