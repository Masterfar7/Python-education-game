using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int version = 1;
    public string sceneName;
    public float savedAtUnix;

    public static SaveData Create(string sceneName)
    {
        return new SaveData
        {
            sceneName = sceneName,
            savedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }
}

