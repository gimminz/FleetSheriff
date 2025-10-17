using System.Diagnostics;
using TMPro;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FontReplacerTool : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Replace TMP Fonts")]
    public static void ShowWindow()
    {
        GetWindow<FontReplacerTool>("TMP Font Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("TMP Font Replacer", EditorStyles.boldLabel);

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New TMP Font", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Replace In Scene"))
        {
            ReplaceInScene();
        }

        if (GUILayout.Button("Replace In Project (All Prefabs)"))
        {
            ReplaceInProject();
        }
    }

    private void ReplaceInScene()
    {
        if (newFont == null) return;

        var texts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            Undo.RecordObject(t, "Replace TMP Font");
            t.font = newFont;
            EditorUtility.SetDirty(t);
        }
        Debug.Log($"씬 내 TMP 텍스트 {texts.Length}개 교체 완료!");
    }

    private void ReplaceInProject()
    {
        if (newFont == null) return;

        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            var texts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (texts.Length > 0)
            {
                foreach (var t in texts)
                {
                    t.font = newFont;
                    EditorUtility.SetDirty(t);
                }
                count++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"프로젝트 전체 프리팹 {count}개에서 TMP 폰트 교체 완료!");
    }
}
