using System;
using UnityEngine;

[Serializable]
public class TaskData
{
    public string taskDescription;
    public TaskType taskType;

    [TextArea(3, 22)]
    [Tooltip("Если заполнено — зачёт только по сравнению с этим кодом (на состоянии переменных до ввода игрока). Остальные поля можно не заполнять.")]
    public string referenceCode;

    public string expectedString;
    public string variableName;
    public float expectedNumber;
    public bool expectedBool;

    public string doorVarLeftClaims;
    public string doorVarRightClaims;
    public string doorVarTruthful;
    public bool doorExpectedLeft;
    public bool doorExpectedRight;
    public string doorPrintIfLeft;
    public string doorPrintIfRight;
}