using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("UI Objects")]
    [SerializeField] private GameObject[] menuButtons;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject savesContent;
    [SerializeField] private GameObject achievementsContent;

    void Awake()
    {
        if (savesContent != null) savesContent.SetActive(false);
        if (achievementsContent != null) achievementsContent.SetActive(false);

        bool[] savedAchievements = SaveSystem.LoadAchievements();
        for (int i = 0; i < savedAchievements.Length; i++)
        {
            if (savedAchievements[i])
                AchievementSystem.Unlock(i);
        }

        bool hasSave = SaveSystem.TryLoad(1, out _);
        if (continueButton != null)
            continueButton.SetActive(hasSave);
    }

    public void ContinueGame()
    {
        Debug.Log("Кнопка Продолжить нажата!");
        if (SaveSystem.TryLoad(1, out SaveData data))
        {
            Debug.Log($"Загрузка сцены: {data.sceneName}, диалог индекс: {data.dialogueIndex}");
            PlayerPrefs.SetInt("ContinueDialogueIndex", data.dialogueIndex);
            PlayerPrefs.Save();
            SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            Debug.LogWarning("Сохранение не найдено!");
        }
    }

    [Header("Saves UI (optional)")]
    [SerializeField] private TMP_Text savesStatusText;

    [Header("Achievements UI")]
    [SerializeField] private Image[] achievementImages;
    [SerializeField] private Sprite[] unlockedSprites;
    [SerializeField] private Sprite[] lockedSprites;

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
            return;

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSaves()
    {
        SetMenuButtons(false);
        if (savesContent != null) savesContent.SetActive(true);
        RefreshSavesStatus();
    }

    public void BackToMain()
    {
        if (savesContent != null) savesContent.SetActive(false);
        if (achievementsContent != null) achievementsContent.SetActive(false);
        if (backButton != null) backButton.SetActive(false);
        if (achievementImages != null)
        {
            foreach (var img in achievementImages)
            {
                if (img != null) img.gameObject.SetActive(false);
            }
        }
        SetMenuButtons(true);
    }

    public void OpenAchievements()
    {
        SetMenuButtons(false);
        if (backButton != null) backButton.SetActive(true);
        RefreshAchievements();
        if (achievementsContent != null)
            achievementsContent.SetActive(true);
    }

    void SetMenuButtons(bool visible)
    {
        if (menuButtons == null) return;
        foreach (var btn in menuButtons)
        {
            if (btn != null) btn.SetActive(visible);
        }
    }

    void RefreshAchievements()
    {
        if (achievementImages == null)
            return;

        for (int i = 0; i < achievementImages.Length && i < AchievementSystem.TotalAchievements; i++)
        {
            if (achievementImages[i] == null) continue;

            bool isUnlocked = AchievementSystem.IsUnlocked(i);
            achievementImages[i].gameObject.SetActive(true);

            if (unlockedSprites != null && unlockedSprites.Length > i && unlockedSprites[i] != null)
                achievementImages[i].sprite = isUnlocked ? unlockedSprites[i] : lockedSprites[i];
        }
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

