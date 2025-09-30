using System.Collections.Generic;
using UnityEngine;

public class SelectedShipContext : MonoBehaviour
{
    private readonly Dictionary<int, Dictionary<ShipSlot, int?>> _partsByShip = new();
    private readonly Dictionary<int, Dictionary<ShipSlot, int?>> _weaponsByShip = new();

    public int? ActiveShipId { get; private set; }

    public void SetActiveShip(int shipId)
    {
        ActiveShipId = shipId;
        _partsByShip.TryAdd(shipId, new Dictionary<ShipSlot, int?>());
        _weaponsByShip.TryAdd(shipId, new Dictionary<ShipSlot, int?>());
    }

    public void SetPart(int shipId, ShipSlot slot, int? partId)
    {
        if (!_partsByShip.ContainsKey(shipId))
            _partsByShip[shipId] = new Dictionary<ShipSlot, int?>();
        _partsByShip[shipId][slot] = partId;
    }

    public bool TryGetParts(int shipId, out Dictionary<ShipSlot, int?> parts)
    {
        if (_partsByShip.TryGetValue(shipId, out var map))
        {
            parts = new(map);
            return true;
        }
        parts = null;
        return false;
    }

    public Dictionary<int, Dictionary<ShipSlot, int?>> GetAllSelections() => GetAllPartSelections();

    public Dictionary<int, Dictionary<ShipSlot, int?>> GetAllPartSelections()
    {
        var copy = new Dictionary<int, Dictionary<ShipSlot, int?>>();
        foreach (var (shipId, map) in _partsByShip)
            copy[shipId] = new Dictionary<ShipSlot, int?>(map);
        return copy;
    }

    public void SetWeapon(int shipId, ShipSlot slot, int? weaponId)
    {
        if (slot == ShipSlot.Down) return; 
        if (!_weaponsByShip.ContainsKey(shipId))
            _weaponsByShip[shipId] = new Dictionary<ShipSlot, int?>();
        _weaponsByShip[shipId][slot] = weaponId;
    }

    public bool TryGetWeapons(int shipId, out Dictionary<ShipSlot, int?> weapons)
    {
        if (_weaponsByShip.TryGetValue(shipId, out var map))
        {
            weapons = new(map);
            return true;
        }
        weapons = null;
        return false;
    }

    public Dictionary<int, Dictionary<ShipSlot, int?>> GetAllWeaponSelections()
    {
        var copy = new Dictionary<int, Dictionary<ShipSlot, int?>>();
        foreach (var (shipId, map) in _weaponsByShip)
            copy[shipId] = new Dictionary<ShipSlot, int?>(map);
        return copy;
    }

    public void ClearAll()
    {
        _partsByShip.Clear();
        _weaponsByShip.Clear();
        ActiveShipId = null;
    }
}
