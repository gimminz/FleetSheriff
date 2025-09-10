using UnityEngine;

public class GyroPlaneInput : IPlaneInput
{
    public float Pitch => Input.gyro.attitude.x;
    public float Yaw=> Input.gyro.attitude.y;
    public float Roll=>Input.gyro.attitude.z;
}
