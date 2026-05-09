using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string firstGameScene = "Chapter1";
    [SerializeField] private string mainMenuScene = "Menu";

    /// <summary>
    /// Starts the game by loading the first game scene
    /// </summary>
    public void StartGame()
    {
        if (!string.IsNullOrEmpty(firstGameScene))
        {
            SceneManager.LoadScene(firstGameScene);
        }
        else
        {
            Debug.LogError("First game scene name is not set!");
        }
    }

    /// <summary>
    /// Returns to the main menu by loading the main menu scene
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
        }
        else
        {
            Debug.LogError("Main menu scene name is not set!");
        }
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}