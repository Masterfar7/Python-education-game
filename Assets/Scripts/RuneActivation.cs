using UnityEngine;

public class RuneActivation : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem magicParticles;
    [SerializeField] private SpriteRenderer runeSprite;

    [Header("Activation Settings")]
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.cyan;
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private bool pulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip ambientSound;

    private bool isActive = false;
    private Material runeMaterial;
    private float pulseTimer = 0f;
    private Vector3 originalScale;

    private void Awake()
    {
        if (runeSprite != null)
        {
            // Создаём копию материала для индивидуальных настроек
            runeMaterial = new Material(runeSprite.material);
            runeSprite.material = runeMaterial;

            originalScale = transform.localScale;

            // Устанавливаем неактивное состояние
            runeSprite.color = inactiveColor;
        }

        if (magicParticles != null)
        {
            magicParticles.Stop();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (isActive && pulseEffect)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(pulseTimer) * pulseAmount;

            if (runeSprite != null)
            {
                transform.localScale = originalScale * pulse;
            }
        }
    }

    public void Activate()
    {
        if (isActive) return;

        isActive = true;

        // Визуальные эффекты
        if (runeSprite != null)
        {
            runeSprite.color = activeColor;

            // Включаем свечение (Emission)
            if (runeMaterial.HasProperty("_EmissionColor"))
            {
                runeMaterial.EnableKeyword("_EMISSION");
                runeMaterial.SetColor("_EmissionColor", activeColor * glowIntensity);
            }
        }

        // Запускаем частицы
        if (magicParticles != null)
        {
            magicParticles.Play();
        }

        // Звуковые эффекты
        if (audioSource != null)
        {
            if (activationSound != null)
            {
                audioSource.PlayOneShot(activationSound);
            }

            if (ambientSound != null)
            {
                audioSource.clip = ambientSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        pulseTimer = 0f;
    }

    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;

        // Возвращаем неактивное состояние
        if (runeSprite != null)
        {
            runeSprite.color = inactiveColor;
            transform.localScale = originalScale;

            // Отключаем свечение
            if (runeMaterial.HasProperty("_EmissionColor"))
            {
                runeMaterial.DisableKeyword("_EMISSION");
                runeMaterial.SetColor("_EmissionColor", Color.black);
            }
        }

        // Останавливаем частицы
        if (magicParticles != null)
        {
            magicParticles.Stop();
        }

        // Останавливаем ambient звук
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == ambientSound)
        {
            audioSource.Stop();
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    public void Toggle()
    {
        if (isActive)
            Deactivate();
        else
            Activate();
    }

    private void OnDestroy()
    {
        // Очищаем созданный материал
        if (runeMaterial != null)
        {
            Destroy(runeMaterial);
        }
    }
}
