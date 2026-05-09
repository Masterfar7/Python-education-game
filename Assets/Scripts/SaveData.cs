using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int version = 4;
    public string sceneName;
    public float savedAtUnix;
    public bool[] achievements = new bool[3];
    public int dialogueIndex;
    public float playerX;
    public float playerY;
    public float guideX;
    public float guideY;

    public static SaveData Create(string sceneName)
    {
        return new SaveData
        {
            sceneName = sceneName,
            savedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            achievements = new bool[3],
            dialogueIndex = 0
        };
    }
}