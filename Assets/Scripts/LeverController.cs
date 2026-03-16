using UnityEngine;

/// <summary>
/// Управляет спрайтом рычага и связанным объектом.
/// Можно включить рычаг (сменить спрайт) и при необходимости удалить объект.
/// </summary>
public class LeverController : MonoBehaviour
{
    [Header("Спрайты рычага")]
    [SerializeField] private SpriteRenderer leverRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

    [Header("Объект для удаления при активации")]
    [SerializeField] private GameObject objectToDestroy;

    [Header("Состояние")]
    [SerializeField] private bool isOn = false;

    private void Awake()
    {
        // На старте показываем правильный спрайт
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (leverRenderer == null) return;

        leverRenderer.sprite = isOn ? onSprite : offSprite;
    }

    /// <summary>
    /// Включить рычаг (сменить спрайт на "включенный")
    /// и удалить связанный объект (если задан).
    /// </summary>
    public void ActivateLever()
    {
        if (isOn) return; // уже включен

        isOn = true;
        UpdateSprite();

        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
            objectToDestroy = null;
        }
    }
}

