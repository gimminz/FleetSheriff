using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonOnClick : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void ManageSpaceShipButton()
    {
        SceneManager.LoadScene("ManageSpaceShipScene");
    }

    public void MssionSelectButton()
    {
        SceneManager.LoadScene("MissionInfoScene");
    }

    public void StartButton()
    {
        SceneManager.LoadScene("GamePlayScene");
    }

    public void BackButton()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
