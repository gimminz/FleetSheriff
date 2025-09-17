using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + target.rotation*offset;
        transform.position = Vector3.Lerp(
            transform.position, 
            desiredPos, 
            followSpeed * Time.deltaTime);

        transform.LookAt(target, target.up);
    }
}
