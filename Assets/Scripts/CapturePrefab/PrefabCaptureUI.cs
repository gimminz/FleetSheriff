using UnityEngine;
using UnityEngine.UI;

public class PrefabCaptureUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PrefabCaptureSystem captureSystem;

    [Header("슬라이더")]
    [SerializeField] private Slider scaleSlider; // 0.1 ~ 3.0

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
            scaleSlider.onValueChanged.AddListener(v => { if (captureSystem) captureSystem.SetUserScale(v); });
        }

        if (captureButton) captureButton.onClick.AddListener(() => captureSystem?.CaptureToPNG());
        if (resetButton) resetButton.onClick.AddListener(() =>
        {
            if (scaleSlider) scaleSlider.value = 1f;  // 내부적으로 SetUserScale 호출됨
            OnBg(Color.clear);
        });

        if (transparentButton) transparentButton.onClick.AddListener(() => OnBg(Color.clear));
        if (whiteButton) whiteButton.onClick.AddListener(() => OnBg(Color.white));
        if (blackButton) blackButton.onClick.AddListener(() => OnBg(Color.black));
        if (grayButton) grayButton.onClick.AddListener(() => OnBg(Color.gray));
    }

    private void OnBg(Color c) { if (captureSystem) captureSystem.UpdateBackgroundColor(c); }
}
