using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(1000)]
public class ButtonProbe : MonoBehaviour, IPointerClickHandler
{
    public Button target;
    public string tagName = "BackButton";

    private void Reset()
    {
        if (!target) target = GetComponent<Button>();
    }

    private void Awake()
    {
        if (!target) target = GetComponent<Button>();
        if (!target) Debug.LogError($"[Probe:{tagName}] Button is NULL on {name}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[Probe:{tagName}] PointerClick. pos={eventData.position} button={eventData.button}");
        DumpState("OnPointerClick");
    }

    private void OnEnable()
    {
        Debug.Log($"[Probe:{tagName}] OnEnable");
        DumpState("OnEnable");
    }

    public void DumpState(string where)
    {
        var es = EventSystem.current;
        var selected = es ? es.currentSelectedGameObject : null;
        var selectedPath = selected ? selected.transform.GetHierarchyPath() : "(null)";

        Debug.Log(
            $"[Probe:{tagName}] {where}\n" +
            $"- button.interactable={target?.interactable}\n" +
            $"- gameObject.activeInHierarchy={gameObject.activeInHierarchy}\n" +
            $"- Time.timeScale={Time.timeScale}\n" +
            $"- EventSystem.selected={selectedPath}\n" +
            $"- thisPath={transform.GetHierarchyPath()}"
        );
    }
}

public static class TransformExt
{
    public static string GetHierarchyPath(this Transform t)
    {
        if (!t) return "(null)";
        var path = t.name;
        while (t.parent)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
