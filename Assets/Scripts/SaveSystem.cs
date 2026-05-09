using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static string GetSlotPath(int slot)
    {
        slot = Mathf.Clamp(slot, 1, 99);
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }

    public static bool TryLoad(int slot, out SaveData data)
    {
        data = null;
        try
        {
            string path = GetSlotPath(slot);
            Debug.Log($"TryLoad: path={path}, exists={File.Exists(path)}");
            if (!File.Exists(path))
                return false;

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
                return false;

            data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"TryLoad: sceneName={data?.sceneName}, dialogueIndex={data?.dialogueIndex}");
            return data != null && !string.IsNullOrWhiteSpace(data.sceneName);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem: load failed (slot {slot}): {e.Message}");
            return false;
        }
    }

    public static bool Save(int slot, SaveData data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.sceneName))
            return false;

        try
        {
            string path = GetSlotPath(slot);
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem: save failed (slot {slot}): {e.Message}");
            return false;
        }
    }

    public static bool Delete(int slot)
    {
        try
        {
            string path = GetSlotPath(slot);
            if (!File.Exists(path))
                return true;
            File.Delete(path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem: delete failed (slot {slot}): {e.Message}");
            return false;
        }
    }

    public static void AutoSave()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        SaveData data = SaveData.Create(scene);
        Save(1, data);
    }

    public static void AutoSaveWithAchievements(bool[] achievements)
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        var data = SaveData.Create(scene);
        data.achievements = achievements;

        if (DialogueManager.Instance != null)
        {
            data.dialogueIndex = DialogueManager.Instance.CurrentDialogueIndex + 1;

            Transform player = DialogueManager.Instance.GetPlayerTransform();
            Transform guide = DialogueManager.Instance.GetGuideTransform();
            if (player != null)
            {
                data.playerX = player.position.x;
                data.playerY = player.position.y;
            }
            if (guide != null)
            {
                data.guideX = guide.position.x;
                data.guideY = guide.position.y;
            }
        }

        Debug.Log($"AutoSave: scene={scene}, dialogueIndex={data.dialogueIndex}, player=({data.playerX},{data.playerY})");
        Save(1, data);
    }

    public static Vector2? LoadPlayerPosition()
    {
        if (TryLoad(1, out SaveData data))
        {
            if (data.playerX != 0 || data.playerY != 0)
                return new Vector2(data.playerX, data.playerY);
        }
        return null;
    }

    public static Vector2? LoadGuidePosition()
    {
        if (TryLoad(1, out SaveData data))
        {
            if (data.guideX != 0 || data.guideY != 0)
                return new Vector2(data.guideX, data.guideY);
        }
        return null;
    }

    public static bool[] LoadAchievements()
    {
        if (TryLoad(1, out SaveData data))
            return data.achievements;
        return new bool[3];
    }

    public static int LoadDialogueIndex()
    {
        if (TryLoad(1, out SaveData data))
            return data.dialogueIndex;
        return 0;
    }
}

