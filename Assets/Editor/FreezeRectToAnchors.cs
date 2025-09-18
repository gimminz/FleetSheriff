#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class FreezeRectToAnchors
{
    [MenuItem("Tools/UI/Freeze Rect To Anchors (Keep Visual)")]
    private static void FreezeSelected()
    {
        foreach (var obj in Selection.transforms)
        {
            var rt = obj as RectTransform;
            if (rt == null || rt.parent == null) continue;

            var parent = rt.parent as RectTransform;
            if (parent == null) continue;

            Undo.RecordObject(rt, "Freeze Rect To Anchors");

            Rect parentRect = parent.rect;
            Vector2 parentSize = parentRect.size;

            Vector2 min = new Vector2(
                (rt.anchoredPosition.x - rt.pivot.x * rt.rect.width) + rt.offsetMin.x,
                (rt.anchoredPosition.y - rt.pivot.y * rt.rect.height) + rt.offsetMin.y
            );
            Vector2 max = min + rt.rect.size;
            Vector2 parentBottomLeft = new Vector2(parentRect.xMin, parentRect.yMin);

            float anchorMinX = Mathf.Clamp01((rt.anchoredPosition.x - rt.pivot.x * rt.rect.width - parentRect.xMin) / parentSize.x);
            float anchorMaxX = Mathf.Clamp01((rt.anchoredPosition.x + (1f - rt.pivot.x) * rt.rect.width - parentRect.xMin) / parentSize.x);
            float anchorMinY = Mathf.Clamp01((rt.anchoredPosition.y - rt.pivot.y * rt.rect.height - parentRect.yMin) / parentSize.y);
            float anchorMaxY = Mathf.Clamp01((rt.anchoredPosition.y + (1f - rt.pivot.y) * rt.rect.height - parentRect.yMin) / parentSize.y);

            Vector3[] corners = new Vector3[4];
            rt.GetLocalCorners(corners); 

            Vector2 lb = corners[0] + (Vector3)rt.anchoredPosition;
            Vector2 rtCorner = corners[2] + (Vector3)rt.anchoredPosition;

            anchorMinX = Mathf.Clamp01((lb.x - parentRect.xMin) / parentSize.x);
            anchorMinY = Mathf.Clamp01((lb.y - parentRect.yMin) / parentSize.y);
            anchorMaxX = Mathf.Clamp01((rtCorner.x - parentRect.xMin) / parentSize.x);
            anchorMaxY = Mathf.Clamp01((rtCorner.y - parentRect.yMin) / parentSize.y);

            rt.anchorMin = new Vector2(anchorMinX, anchorMinY);
            rt.anchorMax = new Vector2(anchorMaxX, anchorMaxY);

            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        Debug.Log("Freeze Rect To Anchors: Done.");
    }
}
#endif
