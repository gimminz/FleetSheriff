using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrefabCaptureSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Prefab Capture System", false, 0)]
    public static void CreatePrefabCaptureSystem()
    {
        // Canvas 생성
        GameObject canvasObj = new GameObject("PrefabCaptureCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.referenceResolution = new Vector2(1920, 1080);

        // System 오브젝트 생성
        GameObject systemObj = new GameObject("PrefabCaptureSystem");
        PrefabCaptureSystem captureSystem = systemObj.AddComponent<PrefabCaptureSystem>();

        // CapturePoint 생성
        GameObject capturePoint = new GameObject("CapturePoint");
        capturePoint.transform.SetParent(systemObj.transform);
        capturePoint.transform.localPosition = Vector3.zero;

        // UI 컨트롤러 추가
        PrefabCaptureUI uiController = canvasObj.AddComponent<PrefabCaptureUI>();

        // 프레임 영역 생성(좌상단): 프리뷰 패널 대신 "프레임"으로
        GameObject framePanel = CreateCaptureFrame(canvas.transform, out RawImage frameRaw, out RectTransform frameRT);

        // 컨트롤 패널 생성 (우상단)
        GameObject controlPanel = CreateControlPanel(canvas.transform);

        // 슬라이더 찾기
        Slider scaleSlider = null;
        Slider rotXSlider = null, rotYSlider = null, rotZSlider = null;

        foreach (Slider s in controlPanel.GetComponentsInChildren<Slider>(true))
        {
            if (s.name.Contains("Scale")) scaleSlider = s;
            else if (s.name.Contains("RotationX")) rotXSlider = s;
            else if (s.name.Contains("RotationY")) rotYSlider = s;
            else if (s.name.Contains("RotationZ")) rotZSlider = s;
        }

        // 버튼 찾기
        Button captureBtn = null, resetBtn = null;
        Button transBtn = null, whiteBtn = null, blackBtn = null, grayBtn = null;

        foreach (Button btn in controlPanel.GetComponentsInChildren<Button>(true))
        {
            if (btn.name.Contains("Capture")) captureBtn = btn;
            else if (btn.name.Contains("Reset")) resetBtn = btn;
            else if (btn.name.Contains("Transparent")) transBtn = btn;
            else if (btn.name.Contains("White")) whiteBtn = btn;
            else if (btn.name.Contains("Black")) blackBtn = btn;
            else if (btn.name.Contains("Gray")) grayBtn = btn;
        }

        // CaptureSystem 직렬화 설정
        SerializedObject soCaptureSystem = new SerializedObject(captureSystem);
        soCaptureSystem.FindProperty("previewCanvas").objectReferenceValue = canvas;
        soCaptureSystem.FindProperty("capturePoint").objectReferenceValue = capturePoint.transform;

        // 프레임 연결
        soCaptureSystem.FindProperty("captureFrame").objectReferenceValue = frameRT;
        soCaptureSystem.FindProperty("frameRawImage").objectReferenceValue = frameRaw;

        // 전용 카메라/레이어는 런타임에 생성되므로 여기선 생략 가능
        soCaptureSystem.ApplyModifiedProperties();

        // UI Controller 직렬화 설정
        SerializedObject soUI = new SerializedObject(uiController);
        soUI.FindProperty("captureSystem").objectReferenceValue = captureSystem;
        soUI.FindProperty("scaleSlider").objectReferenceValue = scaleSlider;
        soUI.FindProperty("rotationXSlider").objectReferenceValue = rotXSlider;
        soUI.FindProperty("rotationYSlider").objectReferenceValue = rotYSlider;
        soUI.FindProperty("rotationZSlider").objectReferenceValue = rotZSlider;
        soUI.FindProperty("captureButton").objectReferenceValue = captureBtn;
        soUI.FindProperty("resetButton").objectReferenceValue = resetBtn;
        soUI.FindProperty("transparentButton").objectReferenceValue = transBtn;
        soUI.FindProperty("whiteButton").objectReferenceValue = whiteBtn;
        soUI.FindProperty("blackButton").objectReferenceValue = blackBtn;
        soUI.FindProperty("grayButton").objectReferenceValue = grayBtn;
        soUI.ApplyModifiedProperties();

        Selection.activeGameObject = systemObj;
        Debug.Log("✅ 프리팹 캡처 시스템(프레임 기반) 생성 완료!");
    }

    private static GameObject CreateCaptureFrame(Transform parent, out RawImage raw, out RectTransform frameRT)
    {
        GameObject panel = new GameObject("CaptureFrame");
        panel.transform.SetParent(parent, false);

        frameRT = panel.AddComponent<RectTransform>();
        frameRT.anchorMin = new Vector2(0, 1);
        frameRT.anchorMax = new Vector2(0, 1);
        frameRT.pivot = new Vector2(0, 1);
        frameRT.anchoredPosition = new Vector2(20, -20);
        frameRT.sizeDelta = new Vector2(512, 512); // 기본값, 런타임에 imageSize로 동기화

        // 배경(체커 느낌)
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.22f, 0.22f, 0.22f, 0.95f);

        // 내부 RawImage(미리보기)
        GameObject rawGO = new GameObject("FrameRawImage");
        rawGO.transform.SetParent(panel.transform, false);
        RectTransform rawRT = rawGO.AddComponent<RectTransform>();
        rawRT.anchorMin = Vector2.zero;
        rawRT.anchorMax = Vector2.one;
        rawRT.offsetMin = new Vector2(4, 4);
        rawRT.offsetMax = new Vector2(-4, -4);

        raw = rawGO.AddComponent<RawImage>();
        raw.color = Color.white;

        // 테두리(옵션)
        GameObject border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        RectTransform bRT = border.AddComponent<RectTransform>();
        bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
        bRT.offsetMin = Vector2.zero; bRT.offsetMax = Vector2.zero;

        Image bImg = border.AddComponent<Image>();
        bImg.color = new Color(1, 1, 1, 0.12f);

        return panel;
    }

    private static GameObject CreateControlPanel(Transform parent)
    {
        GameObject panel = new GameObject("ControlPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -20);
        rt.sizeDelta = new Vector2(340, 600);

        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.18f, 0.18f, 0.18f, 0.95f);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // 제목
        CreateLabel(panel.transform, "프리팹 캡처 설정", 20, FontStyle.Bold);

        // 스케일
        CreateSliderGroup(panel.transform, "Scale", "사이즈", 0.1f, 3f, 1f);
        CreateSpacer(panel.transform, 5);

        // 회전
        CreateLabel(panel.transform, "회전", 16, FontStyle.Bold);
        CreateSliderGroup(panel.transform, "RotationX", "X축", 0f, 360f, 0f);
        CreateSliderGroup(panel.transform, "RotationY", "Y축", 0f, 360f, 0f);
        CreateSliderGroup(panel.transform, "RotationZ", "Z축", 0f, 360f, 0f);

        CreateSpacer(panel.transform, 10);

        // 배경색
        CreateLabel(panel.transform, "배경 색상", 16, FontStyle.Bold);
        CreateBackgroundButtons(panel.transform);

        CreateSpacer(panel.transform, 15);

        // 액션 버튼
        CreateButton(panel.transform, "CaptureButton", "PNG로 저장", new Color(0.3f, 0.69f, 0.31f), 50);
        CreateButton(panel.transform, "ResetButton", "초기화", new Color(1f, 0.6f, 0f), 40);

        return panel;
    }

    // 이하 UI 유틸 함수들(기존과 동일)
    private static void CreateBackgroundButtons(Transform parent)
    {
        GameObject group = new GameObject("BackgroundButtons");
        group.transform.SetParent(parent, false);

        RectTransform groupRT = group.AddComponent<RectTransform>();
        groupRT.sizeDelta = new Vector2(0, 50);

        HorizontalLayoutGroup hlg = group.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        CreateColorButton(group.transform, "TransparentButton", "투명", new Color(0.8f, 0.8f, 0.8f));
        CreateColorButton(group.transform, "WhiteButton", "흰색", Color.white);
        CreateColorButton(group.transform, "BlackButton", "검정", Color.black);
        CreateColorButton(group.transform, "GrayButton", "회색", Color.gray);
    }

    private static GameObject CreateButton(Transform parent, string name, string label, Color color, float height)
    {
        GameObject btn = new GameObject(name);
        btn.transform.SetParent(parent, false);

        LayoutElement le = btn.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;

        Image img = btn.AddComponent<Image>();
        img.color = color;

        Button button = btn.AddComponent<Button>();
        button.targetGraphic = img;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btn.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = (height > 45) ? 18 : 16;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        return btn;
    }

    private static GameObject CreateLabel(Transform parent, string text, int fontSize, FontStyle style)
    {
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(parent, false);

        LayoutElement le = labelObj.AddComponent<LayoutElement>();
        le.minHeight = fontSize + 5;

        Text label = labelObj.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Color.white;

        return labelObj;
    }

    private static void CreateSliderGroup(Transform parent, string name, string label, float min, float max, float value)
    {
        GameObject group = new GameObject(name + "Group");
        group.transform.SetParent(parent, false);

        LayoutElement le = group.AddComponent<LayoutElement>();
        le.minHeight = 45; le.preferredHeight = 45;

        VerticalLayoutGroup vlg = group.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4; vlg.childControlWidth = true; vlg.childControlHeight = false;

        GameObject labelObj = CreateLabel(group.transform, label, 13, FontStyle.Normal);
        var _ = labelObj.AddComponent<LayoutElement>(); // spacing만 목적

        GameObject sliderObj = new GameObject(name + "Slider");
        sliderObj.transform.SetParent(group.transform, false);

        RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
        sliderRT.sizeDelta = new Vector2(0, 20);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = min; slider.maxValue = max; slider.value = value; slider.wholeNumbers = false;

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.sizeDelta = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>(); bgImg.color = new Color(0.29f, 0.29f, 0.29f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillRT = fillArea.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one; fillRT.sizeDelta = new Vector2(-10, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillImgRT = fill.AddComponent<RectTransform>(); fillImgRT.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>(); fillImg.color = new Color(0.29f, 0.56f, 0.89f);
        slider.fillRect = fillImgRT;

        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRT = handleArea.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero; handleAreaRT.anchorMax = Vector2.one; handleAreaRT.sizeDelta = new Vector2(-10, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRT = handle.AddComponent<RectTransform>(); handleRT.sizeDelta = new Vector2(15, 0);
        Image handleImg = handle.AddComponent<Image>(); handleImg.color = Color.white;

        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;
    }

    private static void CreateColorButton(Transform parent, string name, string label, Color color)
    {
        GameObject btn = new GameObject(name);
        btn.transform.SetParent(parent, false);

        Image img = btn.AddComponent<Image>(); img.color = color;

        Button button = btn.AddComponent<Button>(); button.targetGraphic = img;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btn.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one; textRT.sizeDelta = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 12;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = (color == Color.white) ? Color.black : Color.white;
    }

    private static void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(parent, false);
        LayoutElement le = spacer.AddComponent<LayoutElement>();
        le.minHeight = height; le.preferredHeight = height;
    }
#endif
}
