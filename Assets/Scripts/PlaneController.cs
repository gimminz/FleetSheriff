using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlaneController : MonoBehaviour
{
    public float speed = 50f;
    public float rotationSpeed = 100f;

    public bool useGyro = true;
    public bool useTouch = true;
    public float gyroSensitivity = 0.5f;
    public float touchSensitivity = 0.05f;
    public float smoothSpeed = 5f;

    private Rigidbody rb;
    private Vector3 inputEuler;
    private Vector3 smoothEuler;
    private Quaternion lastGyroRot;

    private float forwardInput = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (useGyro) Input.gyro.enabled = true;

        smoothEuler = Vector3.zero;
        lastGyroRot = Input.gyro.attitude;
    }

    private void Update()
    {
        inputEuler = Vector3.zero;

#if UNITY_EDITOR
        inputEuler.x = Input.GetAxis("Vertical");
        inputEuler.y = Input.GetAxis("Horizontal");
        inputEuler.z = 0;
        //forwardInput = Mathf.Max(0, Input.GetAxis("Vertical"));
#endif

#if UNITY_ANDROID || UNITY_IOS


        if (useGyro)
        {
            Quaternion deviceRot = Input.gyro.attitude;
            Quaternion corrected = new Quaternion(
                -deviceRot.x, -deviceRot.y, deviceRot.z, deviceRot.w);
            Vector3 gyroEuler = corrected.eulerAngles;

            Quaternion delta = corrected * Quaternion.Inverse(lastGyroRot);
            lastGyroRot = corrected;
            Vector3 deltaEuler = delta.eulerAngles;

            if (deltaEuler.x > 180) deltaEuler.x -= 360;
            if (deltaEuler.y > 180) deltaEuler.y -= 360;
            if (deltaEuler.z > 180) deltaEuler.z -= 360;

            inputEuler += deltaEuler * gyroSensitivity;
        }

        if (useTouch && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 delta = new Vector2(
                touch.deltaPosition.x / Screen.width,
                touch.deltaPosition.y / Screen.height);
            inputEuler.x += -delta.y * touchSensitivity*100f;
            inputEuler.y += delta.x * touchSensitivity*100f;
        }
#endif

        smoothEuler = Vector3.Lerp(smoothEuler, inputEuler, Time.deltaTime * smoothSpeed);
        
        transform.Rotate(
            smoothEuler.x * rotationSpeed * Time.deltaTime,
            smoothEuler.y * rotationSpeed * Time.deltaTime,
            -smoothEuler.z * rotationSpeed * Time.deltaTime,
            Space.Self
            );

        transform.Translate(Vector3.forward * speed *forwardInput* Time.deltaTime, Space.Self);
    }

    public void AccelButton()
    {
        forwardInput++;
        forwardInput = Mathf.Clamp(forwardInput, -1, 5);
        Debug.Log("Accel");
    }

    public void DecelButton()
    {
        forwardInput--;
        forwardInput = Mathf.Clamp(forwardInput, -1, 5);
        Debug.Log("Decel");
    }
}
