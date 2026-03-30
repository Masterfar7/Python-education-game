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
            if (!File.Exists(path))
                return false;

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
                return false;

            data = JsonUtility.FromJson<SaveData>(json);
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
}

