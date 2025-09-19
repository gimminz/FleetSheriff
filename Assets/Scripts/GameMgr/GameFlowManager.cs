using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    [Header("Refs")]
    public PlayerHealth player;
    public RestartPanelController restartPanel;

    [Header("Main Menu Options")]
    public bool useSeparateMainMenuScene = true;
    public string mainMenuSceneName = "MainMenu";

    public MenuManager menuManager;           // 같은 씬 내 UI 전환용
    public Menus mainMenuId = Menus.MainMenuSelect;

    public void OnClickMainMenu()
    {
        Debug.Log($"[GFM] MainMenu clicked | useSeparateMainMenuScene={useSeparateMainMenuScene}");

        // 항상 일단 시간/오디오 복구
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (useSeparateMainMenuScene)
        {
            if (string.IsNullOrEmpty(mainMenuSceneName))
            {
                Debug.LogError("[GFM] mainMenuSceneName 비어있음");
                return;
            }

            // 빌드 세팅에 등록되어 있는지 빠른 검증
            bool inBuild = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == mainMenuSceneName) { inBuild = true; break; }
            }
            if (!inBuild)
            {
                Debug.LogError($"[GFM] 씬 '{mainMenuSceneName}' 이(가) Build Settings에 없음 (File > Build Settings > Scenes In Build 확인)");
                return;
            }

            Debug.Log($"[GFM] Loading scene '{mainMenuSceneName}'...");
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            if (!menuManager)
            {
                Debug.LogError("[GFM] menuManager 레퍼런스가 비어있음 (같은 씬 패널 전환 모드인데 MenuManager 미할당)");
                return;
            }

            Debug.Log($"[GFM] Opening menu '{mainMenuId}' via MenuManager...");
            menuManager.Open(mainMenuId);
        }
    }

    public void OnClickRestart()
    {
        Debug.Log("[GFM] Restart clicked -> Reload current scene");
        Time.timeScale = 1f;
        AudioListener.pause = false;
        var idx = SceneManager.GetActiveScene().buildIndex;

        // 현재 씬이 빌드세팅에 없는 경우 대비
        if (idx < 0)
        {
            Debug.LogError("[GFM] 현재 씬이 Build Settings에 등록되어 있지 않음. 이름으로 로드 시도");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(idx);
        }
    }

    public void OnClickResume()
    {
        Debug.Log("[GFM] Resume clicked");
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (restartPanel) restartPanel.Hide();
    }
}
