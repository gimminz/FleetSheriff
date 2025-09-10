using UnityEngine;
using UnityEngine.InputSystem.Users;

public class PlayerInputHandler : MonoBehaviour
{
    public PlaneController plane;

    public bool useGyro = true;
    public bool useTouch = true;
    public float gyroSensitivity = 0.5f;
    public float touchSensitivity = 100f;

    private Quaternion lastGyroRot;
    private int rotationFingerId = -1;

    private void Start()
    {
#if UNITY_EDITOR
#else
        if (useGyro) Input.gyro.enabled = true;
        lastGyroRot = Input.gyro.attitude;
#endif
    }

    private void Update()
    {
        Vector3 rotationInput = Vector3.zero;
#if UNITY_EDITOR
        rotationInput.x = Input.GetAxis("Vertical") * touchSensitivity;
        rotationInput.y = Input.GetAxis("Horizontal") * touchSensitivity;
#endif

#if UNITY_ANDROID || UNITY_IOS
        //gyro
        if (useGyro&&SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Quaternion deviceRot = Input.gyro.attitude;

            Quaternion corrected = new Quaternion(
                -deviceRot.x, -deviceRot.y, deviceRot.z, deviceRot.w);

            Quaternion delta = corrected * Quaternion.Inverse(lastGyroRot);
            lastGyroRot = corrected;
            
            Vector3 deltaEuler = delta.eulerAngles;

            if (deltaEuler.x > 180) deltaEuler.x -= 360;
            if (deltaEuler.y > 180) deltaEuler.y -= 360;
            if (deltaEuler.z > 180) deltaEuler.z -= 360;

            rotationInput += deltaEuler * gyroSensitivity;
        }

        //touch
        if (useTouch)
        {
            foreach (Touch touch in Input.touches)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (isRotationTouchArea(touch.position))
                            rotationFingerId = touch.fingerId;
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (touch.fingerId == rotationFingerId)
                        {
                            Vector2 delta = new Vector2(
                                touch.deltaPosition.x / Screen.width,
                                touch.deltaPosition.y / Screen.height
                                );
                            rotationInput.x += -delta.y * touchSensitivity;
                            rotationInput.y += delta.x * touchSensitivity;
                            Debug.Log($"[TOUCH] delta={delta}, rot=({rotationInput.x}, {rotationInput.y})");
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (touch.fingerId == rotationFingerId)
                            rotationFingerId = -1;
                        break;
                }
            }
        }
#endif
        plane.SetRotationInput(rotationInput);
    }

    private bool isRotationTouchArea(Vector2 touchPos)
    {
        ////현재 화면 반쪽 -> 새로 영역 설정해주기
        //return touchPos.x > Screen.width / 2;

        return true;
    }

}
