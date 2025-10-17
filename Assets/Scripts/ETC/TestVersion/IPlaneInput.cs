using UnityEngine;

public interface IPlaneInput
{
    float Pitch { get; } //상하
    float Yaw { get; } //좌우
    float Roll { get; } //기울기
}
