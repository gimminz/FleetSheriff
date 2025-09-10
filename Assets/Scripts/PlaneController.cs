using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlaneController : MonoBehaviour
{
    public float speed = 50f;
    public float rotationSpeed = 100f;
    public float smoothSpeed = 5f;

    private Rigidbody rb;
    private Vector3 smoothEuler;

    private Vector3 rotationInput;
    private float forwardInput = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        smoothEuler = Vector3.zero;
    }

    private void Update()
    {
        smoothEuler = Vector3.Lerp(smoothEuler, rotationInput, Time.deltaTime * smoothSpeed);
        
        transform.Rotate(
            smoothEuler.x * rotationSpeed * Time.deltaTime,
            smoothEuler.y * rotationSpeed * Time.deltaTime,
            -smoothEuler.z * rotationSpeed * Time.deltaTime,
            Space.Self
            );

        transform.Translate(Vector3.forward * speed *forwardInput* Time.deltaTime, Space.Self);
    }

    public void SetRotationInput(Vector3 input)
    {
        rotationInput = input;
        Debug.Log($"[PLANE] rotationInput = {rotationInput}");
    }

    public void SetForwardInput(float value)
    {
        forwardInput = Mathf.Clamp(value, -1f, 5f);
    }

    public void AccelButton()
    {
        SetForwardInput(forwardInput + 1f);
        Debug.Log("Accel");
    }

    public void DecelButton()
    {
        SetForwardInput(forwardInput - 1f);
        Debug.Log("Decel");
    }
}
