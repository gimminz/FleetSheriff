using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class LeftLeanText : MonoBehaviour
{
    private TMP_Text m_TextComponent;
    public float shearAmount = 10.0f; // 기울기 강도

    void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        // 텍스트가 업데이트될 때마다 정점 수정 함수를 호출하도록 이벤트에 등록
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    void OnTextChanged(Object obj)
    {
        if (obj == m_TextComponent)
        {
            WarpText();
        }
    }

    void Start()
    {
        WarpText();
    }

    private void WarpText()
    {
        m_TextComponent.ForceMeshUpdate(); // 최신 텍스트 정보로 메쉬를 강제 업데이트

        TMP_TextInfo textInfo = m_TextComponent.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) return;

        // 각 문자의 정점을 순회
        for (int i = 0; i < characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // 정점 4개(하나의 문자)의 위치를 수정
            Vector3 offset = new Vector3(-vertices[vertexIndex + 0].y / shearAmount, 0, 0);

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        // 수정된 정점 정보로 메쉬를 업데이트
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}