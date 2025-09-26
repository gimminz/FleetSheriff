using UnityEngine;
using UnityEngine.EventSystems;

public class FireHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ICancelHandler
{
    public Gun gun;

    private int activePointerId = -1;
    private bool holding;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != -1) return;         
        activePointerId = eventData.pointerId;
        holding = true;
        if (gun) gun.OnFireButtonDown();           
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        holding = false;
        activePointerId = -1;
        if (gun) gun.OnFireButtonUp();              
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (holding && eventData.pointerId == activePointerId)
        {
            holding = false;
            activePointerId = -1;
            if (gun) gun.OnFireButtonUp();
        }
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (holding)
        {
            holding = false;
            activePointerId = -1;
            if (gun) gun.OnFireButtonUp();
        }
    }

    private void OnDisable()
    {
        if (holding)
        {
            holding = false;
            activePointerId = -1;
            if (gun) gun.OnFireButtonUp();
        }
    }
}
