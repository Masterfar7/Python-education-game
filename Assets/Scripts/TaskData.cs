using System;

[Serializable]
public class TaskData
{
    public string taskDescription;
    public TaskType taskType;

    public string expectedString;
    public string variableName;
    public float expectedNumber;
    public bool expectedBool;
}