using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlaneController : MonoBehaviour
{
    public IPlaneInput input;
    public float speed = 50f;
    public float rotationSpeed = 100f;

    private void Awake()
    {
        input = new KeyboardPlaneInput();
    }

    private void Update()
    {
        if (input == null) return;
        transform.Rotate(
            input.Pitch * rotationSpeed * Time.deltaTime,
            input.Yaw * rotationSpeed * Time.deltaTime,
            -input.Roll * rotationSpeed * Time.deltaTime,
            Space.Self
            );

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
