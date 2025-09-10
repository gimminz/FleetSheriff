using UnityEngine;

public class KeyboardPlaneInput:IPlaneInput
{
    public float Pitch => Input.GetAxis("Vertical");
    public float Yaw => Input.GetAxis("Horizontal");
    public float Roll => Input.GetKey(KeyCode.Q) ? -1 :
        Input.GetKey(KeyCode.E) ? 1 : 0;
}