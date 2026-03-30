using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Переход по имени сцены (как в Build Settings)
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        SceneManager.LoadScene(sceneName);
    }

    // Переход по индексу сцены из Build Settings
    public void LoadSceneByIndex(int buildIndex)
    {
        if (buildIndex < 0)
            return;

        SceneManager.LoadScene(buildIndex);
    }

    // Перезапуск текущей сцены
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Выход из игры (в редакторе просто остановит Play)
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}