using System.Collections.Generic;
using UnityEngine;

public class SelectedShipContext : MonoBehaviour
{
    private readonly Dictionary<int, Dictionary<ShipSlot, int?>> _byShip = new();

    public int? ActiveShipId { get; private set; }

    public void SetActiveShip(int shipId)
    {
        ActiveShipId = shipId;
        if (!_byShip.ContainsKey(shipId))
            _byShip[shipId] = new Dictionary<ShipSlot, int?>();
    }

    public void SetPart(int shipId, ShipSlot slot, int? partId)
    {
        if (!_byShip.ContainsKey(shipId))
            _byShip[shipId] = new Dictionary<ShipSlot, int?>();
        _byShip[shipId][slot] = partId;
    }

    public Dictionary<int, Dictionary<ShipSlot, int?>> GetAllSelections()
    {
        var copy = new Dictionary<int, Dictionary<ShipSlot, int?>>();
        foreach (var kv in _byShip)
        {
            var inner = new Dictionary<ShipSlot, int?>();
            foreach (var sv in kv.Value)
                inner[sv.Key] = sv.Value;
            copy[kv.Key] = inner;
        }
        return copy;
    }
    public void ClearAll()
    {
        _byShip.Clear();
        ActiveShipId = null;
    }
    public bool TryGetParts(int shipId, out Dictionary<ShipSlot, int?> parts)
    {
        if (_byShip.TryGetValue(shipId, out var map))
        {
            parts = new Dictionary<ShipSlot, int?>(map);
            return true;
        }
        parts = null;
        return false;
    }
}