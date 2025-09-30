using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PrefabCaptureSystem : MonoBehaviour
{
    [Header("캡처 설정")]
    [SerializeField] private GameObject prefabToCapture;
    [SerializeField] private int imageSize = 200;
    [SerializeField] private Color backgroundColor = Color.clear;
    [SerializeField] private string savePath = "Assets/PrefabImages";

    [Header("카메라(직교 전용)")]
    [SerializeField] private float baseOrthoSize = 1.5f;

    [Header("미리보기 UI")]
    [SerializeField] private RawImage previewImage;
    [SerializeField] private Canvas previewCanvas;

    [Header("캡처 컴포넌트")]
    [SerializeField] private Camera captureCamera;
    [SerializeField] private Transform capturePoint;

    [Header("오토 핏")]
    [Tooltip("바운즈에 맞춰 기본으로 얼마나 꽉 채울지 (0~1)")]
    [Range(0.5f, 0.98f)] public float fitFill = 0.9f;

    [Header("사용자 스케일 배수 (슬라이더로 조절)")]
    [SerializeField, Range(0.1f, 3f)] private float userScale = 1f;

    // 내부 상태
    private RenderTexture renderTexture;
    private GameObject currentPrefabInstance;   // 캡처용 복제본
    private Transform pivot;                    // 바운즈 기준 피벗
    private float baseFitScale = 1f;            // 자동 핏으로 산출된 기준 스케일

    private void Start()
    {
        SetupCaptureCamera();
        EnsurePivot();

        if (prefabToCapture) LoadPrefab();
    }

    private void EnsurePivot()
    {
        if (!capturePoint)
        {
            var go = new GameObject("CapturePoint");
            go.transform.SetParent(transform, false);
            capturePoint = go.transform;
        }

        var pivotGO = new GameObject("Pivot");
        pivotGO.transform.SetParent(capturePoint, false);
        pivot = pivotGO.transform;
        pivot.localPosition = Vector3.zero;
        pivot.localRotation = Quaternion.identity;
        pivot.localScale = Vector3.one;
    }

    private void SetupCaptureCamera()
    {
        if (captureCamera == null)
        {
            var camObj = new GameObject("CaptureCamera");
            camObj.transform.SetParent(transform, false);
            captureCamera = camObj.AddComponent<Camera>();
        }

        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = backgroundColor;
        captureCamera.orthographic = true;
        captureCamera.orthographicSize = baseOrthoSize;

        captureCamera.transform.position = Vector3.back * 3f; // Z- 로 떨어트림
        captureCamera.transform.rotation = Quaternion.identity;
        captureCamera.transform.LookAt(Vector3.zero);

        if (renderTexture != null) renderTexture.Release();

        var desc = new RenderTextureDescriptor(imageSize, imageSize, RenderTextureFormat.ARGB32, 24)
        {
            sRGB = true,
            msaaSamples = 1,
            useMipMap = false
        };
        renderTexture = new RenderTexture(desc);
        renderTexture.Create();
        captureCamera.targetTexture = renderTexture;

        if (previewImage) previewImage.texture = renderTexture;
    }

    public void LoadPrefab()
    {
        ClearObject();

        if (!prefabToCapture || !pivot) return;

        currentPrefabInstance = Instantiate(prefabToCapture, pivot);
        currentPrefabInstance.transform.localPosition = Vector3.zero;
        currentPrefabInstance.transform.localRotation = Quaternion.identity;
        currentPrefabInstance.transform.localScale = Vector3.one;

        // 바운즈 기준 가운데 정렬 + 기본 핏
        FitAndCenter();

        // 사용자 배수 적용
        ApplyUserScale();
    }

    private void FitAndCenter()
    {
        if (!currentPrefabInstance) return;

        // 월드 바운즈 수집
        var rends = currentPrefabInstance.GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0) return;

        Bounds worldBounds = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) worldBounds.Encapsulate(rends[i].bounds);

        // Pivot을 바운즈 중심에 오도록 이동(자식은 반대로 이동하는 느낌)
        // 방법: 자식 루트를 offset만큼 이동시켜 화면 원점이 모델 중앙이 되게 함
        Vector3 center = worldBounds.center;
        // pivot은 (0,0,0)에 있고, 자식 루트를 -center만큼 옮겨서 중심 정렬
        currentPrefabInstance.transform.position = -center;

        // 다시 바운즈 재계산 (정렬 후)
        rends = currentPrefabInstance.GetComponentsInChildren<Renderer>(true);
        worldBounds = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) worldBounds.Encapsulate(rends[i].bounds);

        float maxSize = Mathf.Max(worldBounds.size.x, worldBounds.size.y, worldBounds.size.z);

        // 직교카메라의 높이 = 2*orthographicSize
        // 미리보기 정사각형 안에 들어오도록 여유(fitFill) 포함해서 기준 스케일 산출
        float viewHeight = captureCamera.orthographicSize * 2f;
        baseFitScale = (viewHeight * fitFill) / Mathf.Max(maxSize, 1e-4f);

        // 기준 스케일 적용
        pivot.localScale = Vector3.one * baseFitScale;
    }

    private void ApplyUserScale()
    {
        if (!pivot) return;
        pivot.localScale = Vector3.one * (baseFitScale * userScale);
    }

    public void SetUserScale(float scale01to3)
    {
        userScale = Mathf.Clamp(scale01to3, 0.1f, 3f);
        ApplyUserScale();
    }

    public void UpdateBackgroundColor(Color color)
    {
        backgroundColor = color;
        if (captureCamera) captureCamera.backgroundColor = color;
    }

    public void CaptureToPNG()
    {
        if (!currentPrefabInstance)
        {
            Debug.LogError("캡처할 프리팹이 없습니다.");
            return;
        }

#if UNITY_EDITOR
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        string dir = savePath;
#else
        string dir = Path.Combine(Application.persistentDataPath, "PrefabImages");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
#endif

        var prev = RenderTexture.active;
        RenderTexture.active = renderTexture;

        captureCamera.Render();

        Texture2D tex = new Texture2D(imageSize, imageSize, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, imageSize, imageSize), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;

        string fileName = (prefabToCapture ? prefabToCapture.name : "Captured") + "_" +
                          System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string fullPath = Path.Combine(dir, fileName);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Destroy(tex);

        Debug.Log($"이미지 저장: {fullPath}");
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public void ClearObject()
    {
        if (currentPrefabInstance) DestroyImmediate(currentPrefabInstance);
        currentPrefabInstance = null;
    }

    private void OnDestroy()
    {
        if (renderTexture != null) renderTexture.Release();
    }
}
