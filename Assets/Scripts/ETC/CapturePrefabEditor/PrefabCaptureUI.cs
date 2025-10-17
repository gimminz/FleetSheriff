using UnityEngine;
using UnityEngine.UI;

public class PrefabCaptureUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PrefabCaptureSystem captureSystem;

    [Header("슬라이더")]
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private Slider rotationXSlider;
    [SerializeField] private Slider rotationYSlider;
    [SerializeField] private Slider rotationZSlider;

    [Header("버튼")]
    [SerializeField] private Button captureButton;
    [SerializeField] private Button resetButton;

    [Header("배경색")]
    [SerializeField] private Button transparentButton;
    [SerializeField] private Button whiteButton;
    [SerializeField] private Button blackButton;
    [SerializeField] private Button grayButton;

    private void Start()
    {
        if (scaleSlider)
        {
            scaleSlider.minValue = 0.1f;
            scaleSlider.maxValue = 3f;
            scaleSlider.value = 1f;
            scaleSlider.onValueChanged.AddListener(v => captureSystem?.SetUserScale(v));
        }

        if (rotationXSlider)
        {
            rotationXSlider.minValue = 0f; rotationXSlider.maxValue = 360f; rotationXSlider.value = 0f;
            rotationXSlider.onValueChanged.AddListener(v => captureSystem?.SetRotationX(v));
        }
        if (rotationYSlider)
        {
            rotationYSlider.minValue = 0f; rotationYSlider.maxValue = 360f; rotationYSlider.value = 0f;
            rotationYSlider.onValueChanged.AddListener(v => captureSystem?.SetRotationY(v));
        }
        if (rotationZSlider)
        {
            rotationZSlider.minValue = 0f; rotationZSlider.maxValue = 360f; rotationZSlider.value = 0f;
            rotationZSlider.onValueChanged.AddListener(v => captureSystem?.SetRotationZ(v));
        }

        if (captureButton) captureButton.onClick.AddListener(() => captureSystem?.CaptureToPNG());

        if (resetButton) resetButton.onClick.AddListener(() =>
        {
            if (scaleSlider) scaleSlider.value = 1f;
            if (rotationXSlider) rotationXSlider.value = 0f;
            if (rotationYSlider) rotationYSlider.value = 0f;
            if (rotationZSlider) rotationZSlider.value = 0f;
            OnBg(Color.clear);
        });

        if (transparentButton) transparentButton.onClick.AddListener(() => OnBg(Color.clear));
        if (whiteButton) whiteButton.onClick.AddListener(() => OnBg(Color.white));
        if (blackButton) blackButton.onClick.AddListener(() => OnBg(Color.black));
        if (grayButton) grayButton.onClick.AddListener(() => OnBg(Color.gray));
    }

    private void OnBg(Color c) => captureSystem?.UpdateBackgroundColor(c);
}
