using UnityEngine;

public static class AchievementSystem
{
    public const int TotalAchievements = 3;

    public static void Unlock(int index)
    {
        if (index < 0 || index >= TotalAchievements)
            return;

        if (PlayerPrefs.GetInt("Achievement_" + index, 0) == 0)
        {
            PlayerPrefs.SetInt("Achievement_" + index, 1);
            PlayerPrefs.Save();
            Debug.Log($"Achievement unlocked: {index}");
        }
    }

    public static bool IsUnlocked(int index)
    {
        if (index < 0 || index >= TotalAchievements)
            return false;
        return PlayerPrefs.GetInt("Achievement_" + index, 0) == 1;
    }

    public static bool[] GetAll()
    {
        bool[] result = new bool[TotalAchievements];
        for (int i = 0; i < TotalAchievements; i++)
        {
            result[i] = IsUnlocked(i);
        }
        return result;
    }

    public static void Reset()
    {
        for (int i = 0; i < TotalAchievements; i++)
        {
            PlayerPrefs.DeleteKey("Achievement_" + i);
        }
        PlayerPrefs.Save();
    }
}