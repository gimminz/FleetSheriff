using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickProbe : MonoBehaviour
{
    private PointerEventData _ped;
    private List<RaycastResult> _hits = new();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current == null) { Debug.Log("No EventSystem"); return; }

            _ped ??= new PointerEventData(EventSystem.current);
            _ped.position = Input.mousePosition;

            _hits.Clear();
            EventSystem.current.RaycastAll(_ped, _hits);

            if (_hits.Count == 0) Debug.Log("UI Raycast: no hits");
            else
            {
                Debug.Log($"UI Raycast hits ({_hits.Count}):");
                foreach (var h in _hits) Debug.Log($" - {h.gameObject.name}");
            }
        }
    }
}
