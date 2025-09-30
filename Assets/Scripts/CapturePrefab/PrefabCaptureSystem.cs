using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PrefabCaptureSystem : MonoBehaviour
{
    [Header("캡처 설정")]
    [SerializeField] private GameObject prefabToCapture;
    [SerializeField] private int imageSize = 512;                // PNG 출력 해상도(정사각)
    [SerializeField] private Color backgroundColor = Color.clear; // 투명 배경
    [SerializeField] private string savePath = "Assets/PrefabImages";

    [Header("카메라(직교 전용)")]
    [SerializeField] private float baseOrthoSize = 1.5f;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private LayerMask captureLayer = ~0; // 전용 레이어를 쓰면 깔끔

    [Header("캡처 프레임(UI)")]
    [SerializeField] private RectTransform captureFrame;          // 캔버스 위 사각 가이드
    [SerializeField] private RawImage frameRawImage;              // 프레임 안의 RawImage(미리보기)
    [SerializeField] private Canvas previewCanvas;                // 프레임이 있는 캔버스

    [Header("캡처 컴포넌트")]
    [SerializeField] private Camera captureCamera;                // 전용 카메라(직교)
    [SerializeField] private Transform capturePoint;

    [Header("오토 핏")]
    [Tooltip("바운즈에 맞춰 기본으로 얼마나 꽉 채울지 (0~1)")]
    [Range(0.5f, 0.98f)] public float fitFill = 0.9f;

    [Header("사용자 스케일 배수")]
    [SerializeField, Range(0.1f, 3f)] private float userScale = 1f;

    [Header("회전 설정")]
    public Vector3 rotation = Vector3.zero;

    // 내부 상태
    private RenderTexture renderTexture;
    private GameObject currentPrefabInstance;
    private Transform pivot;
    private float baseFitScale = 1f;
    private GameObject lastPrefab;

    void Start()
    {
        EnsurePivot();
        SetupCaptureCamera();
        EnsureRenderTexture();
        HookPreviewToFrame();   // 프레임에 RT 연결
        ApplyFrameSize();       // 프레임 크기를 imageSize에 맞춤

        if (prefabToCapture)
        {
            lastPrefab = prefabToCapture;
            LoadPrefab();
        }
    }

    void OnValidate()
    {
        // 에디터에서 값 바뀔 때 프레임/RT 동기화
        ApplyFrameSize();
        EnsureRenderTexture();
        HookPreviewToFrame();
        if (captureCamera) captureCamera.backgroundColor = backgroundColor;
    }

    void Update()
    {
        if (prefabToCapture != lastPrefab)
        {
            lastPrefab = prefabToCapture;
            LoadPrefab();
        }

        // 카메라는 targetTexture로 자동 렌더링되므로 수동 Render 불필요
        // 다만 회전/스케일 슬라이더 반영은 매 프레임 유지
        if (pivot) pivot.localRotation = Quaternion.Euler(rotation);
    }

    private void EnsurePivot()
    {
        if (!capturePoint)
        {
            var go = new GameObject("CapturePoint");
            go.transform.SetParent(transform, false);
            capturePoint = go.transform;
            capturePoint.localPosition = Vector3.zero;
        }

        if (pivot) DestroyImmediate(pivot.gameObject);
        var pivotGO = new GameObject("Pivot");
        pivotGO.transform.SetParent(capturePoint, false);
        pivot = pivotGO.transform;
        pivot.localPosition = Vector3.zero;
        pivot.localRotation = Quaternion.Euler(rotation);
        pivot.localScale = Vector3.one;
    }

    private void SetupCaptureCamera()
    {
        if (!captureCamera)
        {
            var camObj = new GameObject("CaptureCamera");
            camObj.transform.SetParent(transform, false);
            captureCamera = camObj.AddComponent<Camera>();
        }

        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = backgroundColor;   // α=0 유지
        captureCamera.orthographic = true;
        captureCamera.orthographicSize = baseOrthoSize;
        captureCamera.nearClipPlane = 0.01f;
        captureCamera.farClipPlane = 100f;
        captureCamera.depth = -100;
        captureCamera.cullingMask = captureLayer;

        // 위치/방향
        if (capturePoint)
        {
            captureCamera.transform.position = capturePoint.position + Vector3.back * cameraDistance;
            captureCamera.transform.LookAt(capturePoint.position);
        }
        else
        {
            captureCamera.transform.position = Vector3.back * cameraDistance;
            captureCamera.transform.rotation = Quaternion.identity;
        }
    }

    private void EnsureRenderTexture()
    {
        // RT 재생성(사이즈나 포맷이 다르면)
        if (renderTexture &&
            (renderTexture.width != imageSize || renderTexture.height != imageSize))
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
            renderTexture = null;
        }

        if (!renderTexture)
        {
            renderTexture = new RenderTexture(imageSize, imageSize, 24, RenderTextureFormat.ARGB32);
            renderTexture.useMipMap = false;
            renderTexture.autoGenerateMips = false;
            renderTexture.antiAliasing = 4;
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.Create();
        }

        if (captureCamera) captureCamera.targetTexture = renderTexture;
    }

    private void HookPreviewToFrame()
    {
        if (frameRawImage)
        {
            frameRawImage.texture = renderTexture;
            frameRawImage.uvRect = new Rect(0, 0, 1, 1); // 꽉 채우기
        }
    }

    private void ApplyFrameSize()
    {
        // 캔버스의 ReferenceResolution 기준으로 프레임을 정사각 유지.
        if (captureFrame)
        {
            captureFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageSize);
            captureFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageSize);
        }
    }

    public void LoadPrefab()
    {
        ClearObject();

        if (!prefabToCapture || !pivot)
        {
            Debug.LogWarning("프리팹 또는 Pivot이 없습니다.");
            return;
        }

        currentPrefabInstance = Instantiate(prefabToCapture, pivot);
        currentPrefabInstance.transform.localPosition = Vector3.zero;
        currentPrefabInstance.transform.localRotation = Quaternion.identity;
        currentPrefabInstance.transform.localScale = Vector3.one;

        // 전용 레이어로 이동(선택)
        ApplyLayerRecursively(currentPrefabInstance, LayerMaskToLayer(captureLayer));

        FitAndCenter();
        ApplyUserScale();
    }

    private int LayerMaskToLayer(LayerMask mask)
    {
        int m = mask.value;
        if (m == 0 || (m & (m - 1)) != 0) return gameObject.layer; // 다중/빈 마스크면 현재 레이어 유지
        int layer = 0;
        while (m > 1) { m >>= 1; layer++; }
        return layer;
    }

    private void ApplyLayerRecursively(GameObject go, int layer)
    {
        if (layer < 0 || layer > 31) return;
        go.layer = layer;
        foreach (Transform t in go.transform) ApplyLayerRecursively(t.gameObject, layer);
    }

    private void FitAndCenter()
    {
        if (!currentPrefabInstance || !captureCamera) return;

        var rends = currentPrefabInstance.GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0)
        {
            Debug.LogWarning("프리팹에 Renderer가 없습니다.");
            return;
        }

        Bounds worldBounds = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) worldBounds.Encapsulate(rends[i].bounds);

        // 중심 정렬: pivot 기준으로 가운데 오도록 보정
        Vector3 center = worldBounds.center;
        currentPrefabInstance.transform.position = -center;

        // 재계산 후 최대 치수 구하기
        worldBounds = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) worldBounds.Encapsulate(rends[i].bounds);

        float maxSize = Mathf.Max(worldBounds.size.x, worldBounds.size.y, worldBounds.size.z);
        float viewHeight = captureCamera.orthographicSize * 2f;
        baseFitScale = (viewHeight * fitFill) / Mathf.Max(maxSize, 1e-4f);
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

    public void SetRotationX(float angle) { rotation.x = angle; if (pivot) pivot.localRotation = Quaternion.Euler(rotation); }
    public void SetRotationY(float angle) { rotation.y = angle; if (pivot) pivot.localRotation = Quaternion.Euler(rotation); }
    public void SetRotationZ(float angle) { rotation.z = angle; if (pivot) pivot.localRotation = Quaternion.Euler(rotation); }

    public void UpdateBackgroundColor(Color color)
    {
        backgroundColor = color;
        if (captureCamera) captureCamera.backgroundColor = color;
    }

    public void SetImageSize(int size)
    {
        imageSize = Mathf.Max(32, size);
        ApplyFrameSize();
        EnsureRenderTexture();
        HookPreviewToFrame();
    }

    public void CaptureToPNG()
    {
        if (currentPrefabInstance == null)
        {
            Debug.LogError("❌ 캡처할 프리팹이 없습니다!");
            return;
        }

#if UNITY_EDITOR
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        string dir = savePath;
#else
        string dir = Path.Combine(Application.persistentDataPath, "PrefabImages");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
#endif
        // RT에서 바로 픽셀 읽기(프레임에 보이는 그대로)
        var prev = RenderTexture.active;
        RenderTexture.active = renderTexture;

        // 최신 프레임 보장(에디터/런타임 모두 안전)
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

        Debug.Log($"✅ 이미지 저장: {fullPath}");
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
        if (renderTexture)
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }
    }
}
