using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelProgressUI : MonoBehaviour
{
    [Header("UI Элементы")]
    [SerializeField] private RectTransform progressFillRect; // RectTransform заполнения (будем менять Width)
    [SerializeField] private Image progressFillImage; // Image заполнения
    [SerializeField] private Image progressBackgroundImage; // Внешняя часть (рамка)
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Настройки прогресса")]
    [SerializeField] private int totalTasks = 5; // Общее количество заданий на уровне
    [SerializeField] private float maxFillWidth = 200f; // Максимальная ширина заполнения

    private int completedTasks = 0;
    private Vector2 initialFillPosition;

    [Header("Анимация")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float progressIncreaseAnimDuration = 0.5f;

    private Coroutine fadeCoroutine;
    private Coroutine progressChangeCoroutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (progressFillRect == null && progressFillImage != null)
            progressFillRect = progressFillImage.GetComponent<RectTransform>();

        // Сохраняем максимальную ширину и начальную позицию
        if (progressFillRect != null)
        {
            maxFillWidth = progressFillRect.sizeDelta.x;
            initialFillPosition = progressFillRect.anchoredPosition;
        }

        // Скрываем UI по умолчанию
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Показывает UI прогресса с анимацией и инициализирует значения
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);

        // Сбрасываем прогресс до нуля
        completedTasks = 0;
        UpdateProgressFill(instant: true);

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCoroutine(1f, fadeInDuration));
    }

    /// <summary>
    /// Скрывает UI прогресса с анимацией
    /// </summary>
    public void Hide()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCoroutine(0f, fadeOutDuration, () => gameObject.SetActive(false)));
    }

    /// <summary>
    /// Увеличивает прогресс на одно задание
    /// </summary>
    public void CompleteTask()
    {
        completedTasks = Mathf.Min(totalTasks, completedTasks + 1);
        AnimateProgressIncrease();
    }

    /// <summary>
    /// Устанавливает общее количество заданий на уровне
    /// </summary>
    public void SetTotalTasks(int total)
    {
        totalTasks = Mathf.Max(1, total);
        UpdateProgressFill(instant: true);
    }

    /// <summary>
    /// Проверяет, завершены ли все задания
    /// </summary>
    public bool IsLevelComplete()
    {
        return completedTasks >= totalTasks;
    }

    /// <summary>
    /// Возвращает текущее количество выполненных заданий
    /// </summary>
    public int GetCompletedTasks()
    {
        return completedTasks;
    }

    /// <summary>
    /// Возвращает процент заполнения прогресса (0-1)
    /// </summary>
    public float GetProgressPercent()
    {
        return totalTasks > 0 ? (float)completedTasks / totalTasks : 0f;
    }

    private void AnimateProgressIncrease()
    {
        if (progressChangeCoroutine != null)
            StopCoroutine(progressChangeCoroutine);

        progressChangeCoroutine = StartCoroutine(ProgressIncreaseCoroutine());
    }

    private void UpdateProgressFill(bool instant = false)
    {
        if (progressFillRect == null || progressFillImage == null) return;

        float targetPercent = GetProgressPercent();
        float targetWidth = maxFillWidth * targetPercent;

        if (instant)
        {
            // Меняем ширину RectTransform
            Vector2 sizeDelta = progressFillRect.sizeDelta;
            sizeDelta.x = targetWidth;
            progressFillRect.sizeDelta = sizeDelta;

            // Возвращаем начальную позицию
            progressFillRect.anchoredPosition = initialFillPosition;
        }
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duration, System.Action onComplete = null)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
    }

    private IEnumerator ProgressIncreaseCoroutine()
    {
        if (progressFillRect == null || progressFillImage == null) yield break;

        float startWidth = progressFillRect.sizeDelta.x;
        float targetPercent = GetProgressPercent();
        float targetWidth = maxFillWidth * targetPercent;

        float elapsed = 0f;
        while (elapsed < progressIncreaseAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / progressIncreaseAnimDuration);

            // Плавно меняем ширину
            float currentWidth = Mathf.Lerp(startWidth, targetWidth, t);

            Vector2 sizeDelta = progressFillRect.sizeDelta;
            sizeDelta.x = currentWidth;
            progressFillRect.sizeDelta = sizeDelta;

            // Сохраняем начальную позицию
            progressFillRect.anchoredPosition = initialFillPosition;

            yield return null;
        }

        UpdateProgressFill(instant: true);
    }
}
