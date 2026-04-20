using UnityEngine;
using System.Collections;

public class WeedAnimationController : MonoBehaviour
{
    [Header("Анимация роста")]
    [SerializeField] private float growDuration = 2f;
    [SerializeField] private Sprite[] growSprites; // Спрайты роста (по порядку)
    [SerializeField] private float timeBetweenGrowSprites = 0.3f; // Время между сменой спрайтов роста
    [SerializeField] private Vector3 targetScale = new Vector3(2.2f, 2.2f, 2.2f);
    [SerializeField] private AnimationCurve growCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Анимация гибели")]
    [SerializeField] private float deathDuration = 1.5f;
    [SerializeField] private Sprite[] deathSprites; // Спрайты гибели (по порядку)
    [SerializeField] private float timeBetweenDeathSprites = 0.3f; // Время между сменой спрайтов гибели
    [SerializeField] private AnimationCurve deathCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private Color deathColor = new Color(0.5f, 0.3f, 0.1f); // Коричневый (если нет спрайтов)

    [Header("Компоненты")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector3 initialScale;
    private Color initialColor;
    private Sprite initialSprite;
    private Coroutine currentAnimation;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        initialScale = transform.localScale;
        if (spriteRenderer != null)
        {
            initialColor = spriteRenderer.color;
            initialSprite = spriteRenderer.sprite;
        }

        // Начинаем с нулевого размера, но объект активен
        transform.localScale = Vector3.zero;

        // НЕ деактивируем объект в Awake, только устанавливаем scale = 0
        // gameObject.SetActive(false); - убрали эту строку
    }

    /// <summary>
    /// Запускает анимацию роста сорняка
    /// </summary>
    public void PlayGrowAnimation()
    {
        Debug.Log($"WeedAnimationController: PlayGrowAnimation вызван на {gameObject.name}");

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        // Объект уже активен, просто сбрасываем scale
        transform.localScale = Vector3.zero;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = initialColor;
            spriteRenderer.sprite = initialSprite;
            Debug.Log($"WeedAnimationController: Спрайт установлен, начинаем корутину");
        }

        currentAnimation = StartCoroutine(GrowCoroutine());
    }

    /// <summary>
    /// Запускает анимацию гибели сорняка (после выполнения кода)
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(DeathCoroutine());
    }

    /// <summary>
    /// Сброс состояния (для повторного использования)
    /// </summary>
    public void ResetWeed()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        transform.localScale = Vector3.zero;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = initialColor;
            spriteRenderer.sprite = initialSprite;
        }
        gameObject.SetActive(false);
    }

    private IEnumerator GrowCoroutine()
    {
        Debug.Log($"WeedAnimationController: GrowCoroutine начата, growDuration={growDuration}");

        float elapsed = 0f;
        int currentSpriteIndex = 0;
        float nextSpriteTime = 0f;

        // Если есть спрайты роста, устанавливаем первый
        if (growSprites != null && growSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = growSprites[0];
            nextSpriteTime = timeBetweenGrowSprites;
            Debug.Log($"WeedAnimationController: Установлен первый спрайт роста, всего спрайтов: {growSprites.Length}");

            // Устанавливаем начальный scale в зависимости от количества спрайтов
            if (growSprites.Length > 1)
            {
                float scaleStep = 1f / growSprites.Length;
                transform.localScale = targetScale * scaleStep;
            }
        }
        else
        {
            Debug.LogWarning($"WeedAnimationController: Нет спрайтов роста! growSprites={growSprites}, length={growSprites?.Length}");
        }

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;

            // Смена спрайтов роста
            if (growSprites != null && growSprites.Length > 1 && spriteRenderer != null)
            {
                if (elapsed >= nextSpriteTime && currentSpriteIndex < growSprites.Length - 1)
                {
                    currentSpriteIndex++;
                    spriteRenderer.sprite = growSprites[currentSpriteIndex];
                    nextSpriteTime += timeBetweenGrowSprites;

                    // Увеличиваем scale с каждым новым спрайтом
                    float scaleProgress = (float)(currentSpriteIndex + 1) / growSprites.Length;
                    transform.localScale = targetScale * scaleProgress;

                    Debug.Log($"WeedAnimationController: Смена спрайта на #{currentSpriteIndex}, scale={transform.localScale}");
                }
            }

            yield return null;
        }

        transform.localScale = targetScale;
        Debug.Log($"WeedAnimationController: Рост завершён, финальный scale={transform.localScale}");

        // Устанавливаем последний спрайт роста
        if (growSprites != null && growSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = growSprites[growSprites.Length - 1];
        }
    }

    private IEnumerator DeathCoroutine()
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        int currentSpriteIndex = 0;
        float nextSpriteTime = 0f;

        // Если есть спрайты гибели, устанавливаем первый
        if (deathSprites != null && deathSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = deathSprites[0];
            nextSpriteTime = timeBetweenDeathSprites;
        }
        else
        {
            // Если нет спрайтов гибели - меняем цвет (старая логика)
            float colorDuration = deathDuration * 0.3f;
            while (elapsed < colorDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / colorDuration);

                if (spriteRenderer != null)
                    spriteRenderer.color = Color.Lerp(initialColor, deathColor, t);

                yield return null;
            }

            if (spriteRenderer != null)
                spriteRenderer.color = deathColor;

            elapsed = 0f;
        }

        // Фаза уменьшения размера + смена спрайтов
        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / deathDuration);

            // Анимация уменьшения
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // Смена спрайтов гибели
            if (deathSprites != null && deathSprites.Length > 1 && spriteRenderer != null)
            {
                if (elapsed >= nextSpriteTime && currentSpriteIndex < deathSprites.Length - 1)
                {
                    currentSpriteIndex++;
                    spriteRenderer.sprite = deathSprites[currentSpriteIndex];
                    nextSpriteTime += timeBetweenDeathSprites;
                }
            }

            yield return null;
        }

        transform.localScale = Vector3.zero;

        // Устанавливаем последний спрайт гибели
        if (deathSprites != null && deathSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = deathSprites[deathSprites.Length - 1];
        }

        gameObject.SetActive(false);
    }
}
