using UnityEngine;

/// <summary>
/// 가로 모드 기준: 기울임은 Pitch/ Roll(자이로), Yaw는 터치로 제어.
/// 축 규약: x=Pitch, y=Yaw, z=Roll.
/// plane.SetRotationInput(Vector3(x,y,z)) 에 프레임별 입력 전달.
/// </summary>
public class GyroTouchFlightInput : MonoBehaviour
{
    [Header("Targets")]
    public PlaneController plane;

    [Header("Toggles")]
    public bool useGyro = true;
    public bool useTouchYaw = true;
    public bool invertPitch = true;     // 요구사항: 앞으로 숙이면 Pitch Up(+)
    public bool showDebugHUD = false;

    [Header("Sensitivity")]
    [Tooltip("도 단위 입력 → 컨트롤러 입력 스케일")]
    public float pitchSensitivity = 1.0f;
    public float rollSensitivity = 1.0f;
    public float yawTouchSensitivity = 100f; // 화면정규화 델타(0~1) → 입력

    [Header("Filtering (degrees)")]
    [Tooltip("데드존 각도 (±deg)")]
    public float deadzoneDeg = 0.5f;
    [Tooltip("Pitch 컷오프(Hz)")]
    public float pitchCutoffHz = 4.0f;
    [Tooltip("Roll 컷오프(Hz)")]
    public float rollCutoffHz = 6.0f;
    [Tooltip("프레임당 변화 제한 (deg/sec)")]
    public float slewLimitDegPerSec = 360f;

    [Header("Yaw Touch Options")]
    [Tooltip("Yaw 터치 영역: 화면 우측 절반만 허용할지")]
    public bool yawRightHalfOnly = true;
    [Tooltip("터치가 없을 때 Yaw를 0으로 서서히 복귀 (단위: 입력/초)")]
    public float yawReturnRate = 200f;

    [Header("Recenter / Zeroing")]
    [Tooltip("앱 시작 시 자동 영점 설정")]
    public bool recenterOnStart = true;
    [Tooltip("더블탭 인식 시간 (초)")]
    public float doubleTapWindow = 0.3f;
    [Tooltip("소프트 리센터 시간 (초)")]
    public float softRecenterTime = 0.5f;

#if UNITY_ANDROID || UNITY_IOS
    private Quaternion _reference = Quaternion.identity;
    private Quaternion _currentUnityGyro = Quaternion.identity;
    private bool _hasReference = false;
#endif

    // Filters
    private LowPassFilter _pitchLP;
    private LowPassFilter _rollLP;
    private SlewLimiter _pitchSlew;
    private SlewLimiter _rollSlew;

    // State
    private int _yawFingerId = -1;
    private float _lastTapTime = -999f;
    private float _yawInput = 0f; // 프레임 누적이 아니라 즉시형 입력. (복귀는 MoveTowards)

    private void Awake()
    {
        _pitchLP = new LowPassFilter();
        _rollLP = new LowPassFilter();
        _pitchSlew = new SlewLimiter();
        _rollSlew = new SlewLimiter();

        _pitchLP.Reset(0f);
        _rollLP.Reset(0f);
        _pitchSlew.Reset(0f);
        _rollSlew.Reset(0f);
    }

    private void Start()
    {
#if UNITY_EDITOR
        // 에디터에서는 키/마우스로 대체 입력 가능 (여기서는 생략)
#else
        if (useGyro && SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }
        if (recenterOnStart)
        {
            TryRecenter(forceHard: true);
        }
#endif
    }

    private void Update()
    {
        if (!plane) return;

        float dt = Mathf.Max(Time.deltaTime, 1e-4f);

        // --- Yaw (Touch 전담) ---
        float yawThisFrame = 0f;
        if (useTouchYaw)
        {
            yawThisFrame = SampleYawFromTouch(dt);
            // 터치가 없으면 0으로 복귀 (부드럽게)
            if (_yawFingerId == -1)
                _yawInput = Mathf.MoveTowards(_yawInput, 0f, yawReturnRate * dt);
        }
        _yawInput += yawThisFrame;

        // --- Pitch & Roll (Gyro) ---
        float pitchDeg = 0f;
        float rollDeg = 0f;

#if UNITY_ANDROID || UNITY_IOS
        if (useGyro && SystemInfo.supportsGyroscope)
        {
            _currentUnityGyro = GetUnityGyroAttitude(); // 좌표 보정 포함

            if (!_hasReference)
            {
                // 아직 기준이 없으면 지금 자세를 기준으로
                _reference = _currentUnityGyro;
                _hasReference = true;

                // 필터/슬루도 초기화
                _pitchLP.Reset(0f);
                _rollLP.Reset(0f);
                _pitchSlew.Reset(0f);
                _rollSlew.Reset(0f);
            }

            Quaternion relative = Quaternion.Inverse(_reference) * _currentUnityGyro;

            // 안전한 각 추출: -180~180
            Vector3 euler = NormalizeEuler(relative.eulerAngles);
            // 규약: x=Pitch, z=Roll (Yaw는 터치로 제어하므로 무시)
            pitchDeg = euler.x;
            rollDeg = euler.z;

            // 요구사항: "앞으로 숙이면 Pitch Up(+), 뒤로 젖히면 Pitch Down(-)"
            if (invertPitch) pitchDeg = -pitchDeg;

            // Landscape Left/Right에서의 축 반전/스왑 이슈는 GetUnityGyroAttitude()에서 정규화함.
        }
#endif

        // --- Deadzone ---
        pitchDeg = ApplyDeadzone(pitchDeg, deadzoneDeg);
        rollDeg = ApplyDeadzone(rollDeg, deadzoneDeg);

        // --- Low-Pass Filtering ---
        _pitchLP.UpdateByHz(pitchDeg, pitchCutoffHz, dt);
        _rollLP.UpdateByHz(rollDeg, rollCutoffHz, dt);

        float pitchFiltered = _pitchLP.Value;
        float rollFiltered = _rollLP.Value;

        // --- Slew Limiting (deg/sec) ---
        pitchFiltered = _pitchSlew.Step(pitchFiltered, slewLimitDegPerSec, dt);
        rollFiltered = _rollSlew.Step(rollFiltered, slewLimitDegPerSec, dt);

        // --- Sensitivity (degree → input scale) ---
        Vector3 rotationInput = Vector3.zero;
        rotationInput.x = pitchFiltered * pitchSensitivity; // Pitch
        rotationInput.z = rollFiltered * rollSensitivity;  // Roll
        rotationInput.y = _yawInput;                        // Yaw (터치)

#if UNITY_EDITOR
        // 에디터 보정 입력(선택): 방향키/마우스로 최소 테스트 가능
        rotationInput.x += Input.GetAxis("Vertical") * 50f * dt;
        rotationInput.y += Input.GetAxis("Horizontal") * 80f * dt;
#endif

        plane.SetRotationInput(rotationInput);

        // --- Recenter input ---
#if UNITY_ANDROID || UNITY_IOS
        if (DetectDoubleTap()) TryRecenter(forceHard: false);
#endif
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R)) TryRecenter(forceHard: false);
#endif
    }

    // ------------------------ Gyro Helpers ------------------------

#if UNITY_ANDROID || UNITY_IOS
    private Quaternion GetUnityGyroAttitude()
    {
        // 장치 쿼터니언 → Unity 좌표계 보정
        // (안드로이드/ iOS 공통으로 잘 알려진 변환. 프로젝트에 따라 미세 조정 필요)
        Quaternion a = Input.gyro.attitude;
        // 일반적 변환: 오른손→왼손 보정
        Quaternion q = new Quaternion(-a.x, -a.y, a.z, a.w);

        // 화면 방향 보정 (Landscape 에서 축 뒤집힘 방지)
        switch (Screen.orientation)
        {
            case ScreenOrientation.LandscapeLeft:
                // Z축으로 +90 회전(화면 기준)
                q = Quaternion.Euler(0, 0, 90) * q;
                break;
            case ScreenOrientation.LandscapeRight:
                // Z축으로 -90 회전
                q = Quaternion.Euler(0, 0, -90) * q;
                break;
            case ScreenOrientation.Portrait:
                // 필요 시 별도 처리
                break;
            case ScreenOrientation.PortraitUpsideDown:
                break;
        }
        return q;
    }
#endif

    private static Vector3 NormalizeEuler(Vector3 euler)
    {
        // 0~360 → -180~180 로 변환
        euler.x = (euler.x > 180f) ? euler.x - 360f : euler.x;
        euler.y = (euler.y > 180f) ? euler.y - 360f : euler.y;
        euler.z = (euler.z > 180f) ? euler.z - 360f : euler.z;
        return euler;
    }

    private static float ApplyDeadzone(float v, float dz)
    {
        return Mathf.Abs(v) < dz ? 0f : v;
    }

    // ------------------------ Touch (Yaw) ------------------------

    private float SampleYawFromTouch(float dt)
    {
        float yawDelta = 0f;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.touches[i];

            if (t.phase == TouchPhase.Began)
            {
                if (_yawFingerId == -1 && IsYawTouchArea(t.position))
                {
                    _yawFingerId = t.fingerId;
                }
                // 더블탭 감지(같은 손가락 아니어도 전체 이벤트로 본다)
                float now = Time.time;
                if (now - _lastTapTime <= doubleTapWindow)
                {
                    // Update() 말미에서 TryRecenter() 호출
                }
                _lastTapTime = now;
            }

            if (t.fingerId != _yawFingerId) continue;

            if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                // 해상도 독립 정규화
                Vector2 nd = new Vector2(
                    t.deltaPosition.x / Screen.width,
                    t.deltaPosition.y / Screen.height
                );
                // Yaw는 x 이동만 사용 (오른쪽 드래그 → +Yaw)
                yawDelta += nd.x * yawTouchSensitivity;
            }

            if (t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended)
            {
                if (t.fingerId == _yawFingerId)
                    _yawFingerId = -1;
            }
        }

        return yawDelta;
    }

    private bool IsYawTouchArea(Vector2 pos)
    {
        if (!yawRightHalfOnly) return true;
        return pos.x > Screen.width * 0.5f;
    }

    private bool DetectDoubleTap()
    {
        // _lastTapTime는 Began에서 갱신됨. 여기선 부가 판단 없이 true만 리턴 가능.
        return false; // 더블탭 즉시 리센터를 원하면 여기 로직을 옮겨도 됨.
    }

    private void TryRecenter(bool forceHard)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!SystemInfo.supportsGyroscope) return;
        if (!useGyro) return;

        Quaternion current = GetUnityGyroAttitude();

        if (!_hasReference || forceHard || softRecenterTime <= 0f)
        {
            _reference = current;
            _hasReference = true;
            _pitchLP.Reset(0f);
            _rollLP.Reset(0f);
            _pitchSlew.Reset(0f);
            _rollSlew.Reset(0f);
            return;
        }

        // 소프트 리센터: 일정 시간 동안 참조를 현재로 보간
        StopAllCoroutines();
        StartCoroutine(CoSoftRecenter(current, softRecenterTime));
#endif
    }

#if UNITY_ANDROID || UNITY_IOS
    private System.Collections.IEnumerator CoSoftRecenter(Quaternion target, float duration)
    {
        Quaternion start = _reference;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            _reference = Quaternion.Slerp(start, target, u);
            yield return null;
        }
        _reference = target;

        // 안정화를 위해 필터 상태도 0 근처로 재정렬
        _pitchLP.Reset(0f);
        _rollLP.Reset(0f);
        _pitchSlew.Reset(0f);
        _rollSlew.Reset(0f);
    }
#endif

    // ------------------------ Debug HUD ------------------------
    private void OnGUI()
    {
        if (!showDebugHUD) return;

        GUI.Label(new Rect(10, 10, 400, 24), $"useGyro:{useGyro} useTouchYaw:{useTouchYaw} invertPitch:{invertPitch}");
#if UNITY_ANDROID || UNITY_IOS
        GUI.Label(new Rect(10, 34, 600, 24), $"orientation:{Screen.orientation} gyroEnabled:{Input.gyro.enabled}");
        if (_hasReference) GUI.Label(new Rect(10, 58, 300, 24), $"Recentered: YES");
#endif
        GUI.Label(new Rect(10, 82, 600, 24), $"pitchCutoff:{pitchCutoffHz}Hz rollCutoff:{rollCutoffHz}Hz deadzone:{deadzoneDeg}° slew:{slewLimitDegPerSec}°/s");
        GUI.Label(new Rect(10, 106, 600, 24), $"yawSens:{yawTouchSensitivity} yawReturn:{yawReturnRate}/s rightHalf:{yawRightHalfOnly}");
    }

    // ------------------------ Utility Classes ------------------------
    private class LowPassFilter
    {
        public float Value { get; private set; }
        private bool _initialized = false;

        public void Reset(float v)
        {
            Value = v;
            _initialized = true;
        }

        public void UpdateByHz(float input, float cutoffHz, float dt)
        {
            if (!_initialized) { Reset(input); return; }
            cutoffHz = Mathf.Max(0.001f, cutoffHz);
            dt = Mathf.Max(1e-4f, dt);
            // alpha = 2pi f dt / (2pi f dt + 1)
            float x = 2f * Mathf.PI * cutoffHz * dt;
            float alpha = x / (x + 1f);
            Value = Mathf.Lerp(Value, input, alpha);
        }
    }

    private class SlewLimiter
    {
        public float Value { get; private set; }
        private bool _initialized = false;

        public void Reset(float v)
        {
            Value = v;
            _initialized = true;
        }

        public float Step(float target, float maxDegPerSec, float dt)
        {
            if (!_initialized) { Reset(target); return Value; }
            float maxDelta = Mathf.Max(0f, maxDegPerSec) * Mathf.Max(1e-4f, dt);
            Value = Mathf.MoveTowards(Value, target, maxDelta);
            return Value;
        }
    }
}
