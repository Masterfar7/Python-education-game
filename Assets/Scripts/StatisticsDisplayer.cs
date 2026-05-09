using TMPro;
using UnityEngine;

public class StatisticsDisplayer : MonoBehaviour
{
    [SerializeField] private TMP_Text statisticsText;

    void Start()
    {
        if (StatisticsManager.Instance == null)
        {
            if (statisticsText != null)
                statisticsText.text = "Статистика недоступна";
            return;
        }

        var data = StatisticsManager.Instance.GetData();

        int minutes = (int)(data.totalPlayTimeSeconds / 60);
        int hours = minutes / 60;
        int remainingMinutes = minutes % 60;

        string timeString = hours > 0
            ? $"{hours} ч {remainingMinutes} мин"
            : $"{minutes} мин";

        statisticsText.text = $"Попыток: {data.totalAttempts}\n" +
            $"Правильно: {data.successfulAttempts} ({data.SuccessRate:F0}%)\n" +
            $"Строк кода: {data.totalCodeLines}\n" +
            $"Время игры: {timeString}\n" +
            $"Ошибки:\n" +
            $"  Синтаксис: {data.syntaxErrors}\n" +
            $"  Логика: {data.logicErrors}\n" +
            $"  Выполнение: {data.runtimeErrors}";
    }
}