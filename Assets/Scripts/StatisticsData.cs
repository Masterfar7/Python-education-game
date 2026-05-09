using System;

[Serializable]
public class StatisticsData
{
    public int totalAttempts;
    public int successfulAttempts;
    public float totalPlayTimeSeconds;
    public int totalCodeLines;

    public int syntaxErrors;
    public int runtimeErrors;
    public int logicErrors;
    public int unknownErrors;

    public float SuccessRate => totalAttempts > 0
        ? (float)successfulAttempts / totalAttempts * 100f
        : 0f;
}