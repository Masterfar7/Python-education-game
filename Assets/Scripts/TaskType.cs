public enum TaskType
{
    PrintExact,
    PrintContains,
    VariableAssignment,
    ExpressionResult,
    BooleanValue,

    /// <summary>Двери: проверка двух bool-утверждений, left_is_truthful и финального print.</summary>
    BooleanDoorRiddle
}