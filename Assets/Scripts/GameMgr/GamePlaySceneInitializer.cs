// Assets/Scripts/Game/GamePlaySceneInitializer.cs
using UnityEngine;

public class GamePlaySceneInitializer : MonoBehaviour
{
    public PlayerHealth playerHealth;

    void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("[GamePlaySceneInitializer] PlayerHealth가 없습니다.");
            return;
        }

        int shipId;

        if (LaunchContext.SelectedShipId.HasValue)
        {
            shipId = LaunchContext.SelectedShipId.Value;
        }
        else
        {
            var fallback = DataTableManager.ShipTable?.GetRandom();
            if (fallback == null)
            {
                Debug.LogError("[GamePlaySceneInitializer] ShipTable에서 초기화할 항목을 찾지 못했습니다. 기본값 41 적용.");
                shipId = 41; 
            }
            else
            {
                shipId = fallback.Id;
                Debug.LogWarning($"[GamePlaySceneInitializer] LaunchContext가 비어 있어 폴백 ShipId={shipId} 사용.");
            }
        }

        playerHealth.InitializeFromShipId(shipId);
    }
}
