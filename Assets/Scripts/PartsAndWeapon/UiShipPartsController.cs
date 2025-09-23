// Assets/Scripts/Parts/UiShipPartsController.cs
using TMPro;
using UnityEngine;

public class UiShipPartsController : MonoBehaviour
{
    public GameObject shipListContainer;   
    public GameObject partsListContainer;  
        
    public UiPartsList partsList;
    public UiPartExplain explain;         

    public TextMeshProUGUI upAddressText;
    public TextMeshProUGUI leftAddressText;
    public TextMeshProUGUI rightAddressText;
    public TextMeshProUGUI downAddressText;

    private ShipSlot currentSlot = ShipSlot.Up;

    private void Start()
    {
        SetModeShipList();
    }
    public void OnClickUpPanel() => OpenPartsForSlot(ShipSlot.Up);
    public void OnClickLeftPanel() => OpenPartsForSlot(ShipSlot.Left);
    public void OnClickRightPanel() => OpenPartsForSlot(ShipSlot.Right);
    public void OnClickDownPanel() => OpenPartsForSlot(ShipSlot.Down);

    private void OpenPartsForSlot(ShipSlot slot)
    {
        currentSlot = slot;
        SetModePartsList();
        partsList.Show(currentSlot, OnSelectPart);
        explain?.Clear();
    }

    private void OnSelectPart(PartData part)
    {
        if (part == null) return;

        explain?.SetData(part);

        var target = GetAddressText(currentSlot);
        if (target)
        {
            target.text = string.IsNullOrWhiteSpace(part.ItemAddress) ? "-" : part.ItemAddress;
        }
    }

    public void SetModeShipList()
    {
        if (shipListContainer) shipListContainer.SetActive(true);
        if (partsListContainer) partsListContainer.SetActive(false);
        explain?.Clear();
    }

    private void SetModePartsList()
    {
        if (shipListContainer) shipListContainer.SetActive(false);
        if (partsListContainer) partsListContainer.SetActive(true);
    }

    private TextMeshProUGUI GetAddressText(ShipSlot slot)
    {
        return slot switch
        {
            ShipSlot.Up => upAddressText,
            ShipSlot.Left => leftAddressText,
            ShipSlot.Right => rightAddressText,
            ShipSlot.Down => downAddressText,
            _ => null
        };
    }
}
