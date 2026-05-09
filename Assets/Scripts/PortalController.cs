using UnityEngine;

public class PortalController : MonoBehaviour
{
    [Header("Анимации портала")]
    [Tooltip("Спрайты для анимации включения портала (проигрывается один раз)")]
    public Sprite[] activationSprites;

    [Tooltip("Спрайты для анимации активного портала (зацикленная)")]
    public Sprite[] idleSprites;

    [Header("Настройки анимации")]
    [Tooltip("Скорость анимации (кадров в секунду)")]
    public float frameRate = 10f;

    [Header("Активация")]
    [Tooltip("Активировать портал после выполнения задания")]
    public bool activateAfterTask = true;

    [Tooltip("Портал активен с самого начала")]
    public bool startActive = false;

    [Tooltip("Показывать неактивный портал (первый кадр idle)")]
    public bool showInactive = true;

    private SpriteRenderer spriteRenderer;
    private bool isActive = false;
    private bool isActivating = false;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private bool activationComplete = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (startActive)
        {
            isActive = true;
        }
        else
        {
            isActive = false;
        }
    }

    private void Start()
    {
        if (startActive)
        {
            ActivatePortal();
        }
    }

    private void Update()
    {
        if (!isActive)
            return;

        frameTimer += Time.deltaTime;

        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;
            NextFrame();
        }
    }

    private void NextFrame()
    {
        // Если проигрывается анимация активации
        if (isActivating && !activationComplete)
        {
            if (activationSprites != null && activationSprites.Length > 0)
            {
                spriteRenderer.sprite = activationSprites[currentFrame];
                currentFrame++;

                // Если анимация активации закончилась
                if (currentFrame >= activationSprites.Length)
                {
                    activationComplete = true;
                    currentFrame = 0;
                    isActivating = false;
                }
            }
            else
            {
                // Если нет спрайтов активации - сразу переходим к idle
                activationComplete = true;
                isActivating = false;
            }
        }
        // Если проигрывается зацикленная анимация
        else if (activationComplete)
        {
            if (idleSprites != null && idleSprites.Length > 0)
            {
                spriteRenderer.sprite = idleSprites[currentFrame];
                currentFrame++;

                // Зацикливаем анимацию
                if (currentFrame >= idleSprites.Length)
                {
                    currentFrame = 0;
                }
            }
        }
    }

    /// <summary>
    /// Активирует портал (запускает анимацию включения)
    /// </summary>
    public void ActivatePortal()
    {
        if (isActive)
            return;

        isActive = true;
        isActivating = true;
        activationComplete = false;
        currentFrame = 0;
        frameTimer = 0f;

        spriteRenderer.enabled = true;

        // Если нет анимации активации - сразу показываем idle
        if (activationSprites == null || activationSprites.Length == 0)
        {
            activationComplete = true;
            isActivating = false;
        }

        Debug.Log($"Портал {gameObject.name} активирован!");
    }

    /// <summary>
    /// Деактивирует портал
    /// </summary>
    public void DeactivatePortal()
    {
        isActive = false;
        isActivating = false;
        activationComplete = false;
        spriteRenderer.enabled = false;

        Debug.Log($"Портал {gameObject.name} деактивирован!");
    }

    /// <summary>
    /// Проверяет, активен ли портал
    /// </summary>
    public bool IsActive()
    {
        return isActive && activationComplete;
    }

    /// <summary>
    /// Проверяет, проигрывается ли анимация активации
    /// </summary>
    public bool IsActivating()
    {
        return isActivating;
    }

    private void OnDrawGizmos()
    {
        // Визуализация портала в редакторе
        Gizmos.color = isActive ? Color.cyan : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
