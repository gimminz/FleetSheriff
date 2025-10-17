using UnityEngine;

public class SuccessPanelGate : MonoBehaviour
{
    private void OnEnable()
    {
        if (!GameFlowManager.Instance || !GameFlowManager.Instance.IsTimerElapsed)
        {
            // 타임업 전 조기 활성화 → 즉시 차단
            gameObject.SetActive(false);
        }
    }
}
