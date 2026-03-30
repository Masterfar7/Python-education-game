using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject savesPanel;

    [Header("Saves UI (optional)")]
    [SerializeField] private TMP_Text savesStatusText;

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
            return;

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSaves()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (savesPanel != null) savesPanel.SetActive(true);
        RefreshSavesStatus();
    }

    public void BackToMain()
    {
        if (savesPanel != null) savesPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void SaveToSlot(int slot)
    {
        string scene = SceneManager.GetActiveScene().name;
        bool ok = SaveSystem.Save(slot, SaveData.Create(scene));
        SetStatus(ok ? $"Сохранено в слот {slot}" : $"Не удалось сохранить (слот {slot})");
        RefreshSavesStatus();
    }

    public void LoadFromSlot(int slot)
    {
        if (!SaveSystem.TryLoad(slot, out SaveData data))
        {
            SetStatus($"Слот {slot} пуст");
            return;
        }

        SceneManager.LoadScene(data.sceneName);
    }

    public void DeleteSlot(int slot)
    {
        bool ok = SaveSystem.Delete(slot);
        SetStatus(ok ? $"Слот {slot} удалён" : $"Не удалось удалить слот {slot}");
        RefreshSavesStatus();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void RefreshSavesStatus()
    {
        if (savesStatusText == null)
            return;

        string s1 = SaveSystem.TryLoad(1, out _) ? "есть" : "пусто";
        string s2 = SaveSystem.TryLoad(2, out _) ? "есть" : "пусто";
        string s3 = SaveSystem.TryLoad(3, out _) ? "есть" : "пусто";
        savesStatusText.text = $"Слот 1: {s1}\nСлот 2: {s2}\nСлот 3: {s3}";
    }

    void SetStatus(string text)
    {
        if (savesStatusText != null && !string.IsNullOrWhiteSpace(text))
            savesStatusText.text = text;
    }
}

