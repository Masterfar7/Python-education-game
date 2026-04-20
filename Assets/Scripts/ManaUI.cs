using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ManaUI : MonoBehaviour
{
    [Header("UI Элементы")]
    [SerializeField] private RectTransform manaFillRect; // RectTransform заполнения (будем менять Width)
    [SerializeField] private Image manaFillImage; // Image заполнения
    [SerializeField] private Image manaBackgroundImage; // Внешняя часть (рамка)
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Настройки маны")]
    [SerializeField] private int maxMana = 100;
    [SerializeField] private int manaDecreasePerLine = 10; // Сколько маны отнимается за каждую фразу
    [SerializeField] private float maxFillWidth = 200f; // Максимальная ширина заполнения

    private int currentMana = 100;
    private Vector2 initialFillPosition; // Начальная позиция заполнения

    [Header("Анимация")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float manaDecreaseAnimDuration = 0.3f;

    [Header("Цвета заполнения")]
    [SerializeField] private Color fullManaColor = Color.cyan;
    [SerializeField] private Color halfManaColor = Color.yellow;
    [SerializeField] private Color lowManaColor = Color.red;

    private Coroutine fadeCoroutine;
    private Coroutine manaChangeCoroutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (manaFillRect == null && manaFillImage != null)
            manaFillRect = manaFillImage.GetComponent<RectTransform>();

        // Сохраняем максимальную ширину и начальную позицию
        if (manaFillRect != null)
        {
            maxFillWidth = manaFillRect.sizeDelta.x;
            initialFillPosition = manaFillRect.anchoredPosition;
        }

        // Скрываем UI по умолчанию
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Показывает UI маны с анимацией и инициализирует значения
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);

        // Сбрасываем ману до максимума
        currentMana = maxMana;
        UpdateManaFill(instant: true);

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCoroutine(1f, fadeInDuration));
    }

    /// <summary>
    /// Скрывает UI маны с анимацией
    /// </summary>
    public void Hide()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCoroutine(0f, fadeOutDuration, () => gameObject.SetActive(false)));
    }

    /// <summary>
    /// Уменьшает ману на заданное количество (вызывается при каждой фразе диалога)
    /// </summary>
    public void DecreaseMana()
    {
        DecreaseMana(manaDecreasePerLine);
    }

    /// <summary>
    /// Уменьшает ману на указанное количество
    /// </summary>
    public void DecreaseMana(int amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
        AnimateManaDecrease();
    }

    /// <summary>
    /// Устанавливает количество маны, отнимаемое за каждую фразу
    /// </summary>
    public void SetManaDecreasePerLine(int amount)
    {
        manaDecreasePerLine = Mathf.Max(1, amount);
    }

    /// <summary>
    /// Проверяет, закончилась ли мана
    /// </summary>
    public bool IsManaEmpty()
    {
        return currentMana <= 0;
    }

    /// <summary>
    /// Возвращает текущее значение маны
    /// </summary>
    public int GetCurrentMana()
    {
        return currentMana;
    }

    /// <summary>
    /// Возвращает процент заполнения маны (0-1)
    /// </summary>
    public float GetManaPercent()
    {
        return maxMana > 0 ? (float)currentMana / maxMana : 0f;
    }

    private void AnimateManaDecrease()
    {
        if (manaChangeCoroutine != null)
            StopCoroutine(manaChangeCoroutine);

        manaChangeCoroutine = StartCoroutine(ManaDecreaseCoroutine());
    }

    private void UpdateManaFill(bool instant = false)
    {
        if (manaFillRect == null || manaFillImage == null) return;

        float targetPercent = GetManaPercent();
        float targetWidth = maxFillWidth * targetPercent;

        if (instant)
        {
            // Меняем ширину RectTransform
            Vector2 sizeDelta = manaFillRect.sizeDelta;
            sizeDelta.x = targetWidth;
            manaFillRect.sizeDelta = sizeDelta;

            // Возвращаем начальную позицию
            manaFillRect.anchoredPosition = initialFillPosition;
        }

        // Обновляем цвет в зависимости от количества маны
        Color targetColor;
        if (targetPercent > 0.5f)
            targetColor = Color.Lerp(halfManaColor, fullManaColor, (targetPercent - 0.5f) * 2f);
        else
            targetColor = Color.Lerp(lowManaColor, halfManaColor, targetPercent * 2f);

        manaFillImage.color = targetColor;
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

    private IEnumerator ManaDecreaseCoroutine()
    {
        if (manaFillRect == null || manaFillImage == null) yield break;

        float startWidth = manaFillRect.sizeDelta.x;
        float targetPercent = GetManaPercent();
        float targetWidth = maxFillWidth * targetPercent;
        Color startColor = manaFillImage.color;

        float elapsed = 0f;
        while (elapsed < manaDecreaseAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / manaDecreaseAnimDuration);

            // Плавно меняем ширину
            float currentWidth = Mathf.Lerp(startWidth, targetWidth, t);

            Vector2 sizeDelta = manaFillRect.sizeDelta;
            sizeDelta.x = currentWidth;
            manaFillRect.sizeDelta = sizeDelta;

            // Сохраняем начальную позицию
            manaFillRect.anchoredPosition = initialFillPosition;

            // Плавное изменение цвета
            float currentPercent = currentWidth / maxFillWidth;
            Color targetColor;
            if (currentPercent > 0.5f)
                targetColor = Color.Lerp(halfManaColor, fullManaColor, (currentPercent - 0.5f) * 2f);
            else
                targetColor = Color.Lerp(lowManaColor, halfManaColor, currentPercent * 2f);

            manaFillImage.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        UpdateManaFill(instant: true);
    }
}
