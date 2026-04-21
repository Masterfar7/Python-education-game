using UnityEngine;

public class ThreeRunesActivation : MonoBehaviour
{
    [Header("Runes Setup")]
    [SerializeField] private GameObject[] runeObjects = new GameObject[3]; // GameObject с SpriteRenderer
    [SerializeField] private Sprite[] inactiveSprites = new Sprite[3]; // Неактивные спрайты
    [SerializeField] private Sprite[] activeSprites = new Sprite[3]; // Активированные спрайты

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem[] runeParticles = new ParticleSystem[3];
    [SerializeField] private Color[] particleColors = new Color[3] { Color.cyan, Color.yellow, Color.magenta };
    [SerializeField] private float particleEmissionRate = 20f;
    [SerializeField] private float particleSize = 0.1f;
    [SerializeField] private float particleLifetime = 0.8f;

    [Header("Activation Animation")]
    [SerializeField] private float activationDelay = 0.3f; // Задержка между активацией рун
    [SerializeField] private float scaleAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;

    private SpriteRenderer[] spriteRenderers = new SpriteRenderer[3];
    private bool[] runesActivated = new bool[3];
    private int currentActivatingRune = -1;
    private float activationTimer = 0f;
    private float scaleTimer = 0f;
    private Vector3[] originalScales = new Vector3[3];

    private void Awake()
    {
        // Получаем SpriteRenderer из GameObject и устанавливаем неактивные спрайты
        for (int i = 0; i < 3; i++)
        {
            if (runeObjects[i] != null)
            {
                spriteRenderers[i] = runeObjects[i].GetComponent<SpriteRenderer>();
                originalScales[i] = runeObjects[i].transform.localScale;

                if (spriteRenderers[i] != null && inactiveSprites[i] != null)
                {
                    spriteRenderers[i].sprite = inactiveSprites[i];
                }
            }

            if (runeParticles[i] != null)
            {
                ConfigureParticleSystem(runeParticles[i], i);
                runeParticles[i].Stop();
            }

            runesActivated[i] = false;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (currentActivatingRune >= 0 && currentActivatingRune < 3)
        {
            scaleTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(scaleTimer / scaleAnimationDuration);
            float scale = scaleCurve.Evaluate(progress);

            if (runeObjects[currentActivatingRune] != null)
            {
                runeObjects[currentActivatingRune].transform.localScale = originalScales[currentActivatingRune] * (0.5f + scale * 0.5f);
            }

            if (progress >= 1f)
            {
                if (runeObjects[currentActivatingRune] != null)
                {
                    runeObjects[currentActivatingRune].transform.localScale = originalScales[currentActivatingRune];
                }
                currentActivatingRune = -1;
                scaleTimer = 0f;
            }
        }
    }

    private void ConfigureParticleSystem(ParticleSystem ps, int index)
    {
        if (ps == null) return;

        var main = ps.main;
        main.startColor = particleColors[index];
        main.startSize = particleSize;
        main.startLifetime = particleLifetime;

        var emission = ps.emission;
        emission.rateOverTime = particleEmissionRate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
    }

    public void ActivateAllRunes()
    {
        StartCoroutine(ActivateRunesSequentially());
    }

    private System.Collections.IEnumerator ActivateRunesSequentially()
    {
        for (int i = 0; i < 3; i++)
        {
            ActivateRune(i);
            yield return new WaitForSeconds(activationDelay);
        }
    }

    private void ActivateRune(int index)
    {
        if (index < 0 || index >= 3 || runesActivated[index])
            return;

        runesActivated[index] = true;

        // Меняем спрайт на активированный
        if (spriteRenderers[index] != null && activeSprites[index] != null)
        {
            spriteRenderers[index].sprite = activeSprites[index];
        }

        // Запускаем анимацию масштаба
        currentActivatingRune = index;
        scaleTimer = 0f;

        // Запускаем частицы
        if (runeParticles[index] != null)
        {
            runeParticles[index].Play();
        }

        // Звук активации
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
    }

    public void DeactivateAllRunes()
    {
        for (int i = 0; i < 3; i++)
        {
            DeactivateRune(i);
        }
    }

    private void DeactivateRune(int index)
    {
        if (index < 0 || index >= 3)
            return;

        runesActivated[index] = false;

        // Меняем спрайт на неактивный
        if (spriteRenderers[index] != null && inactiveSprites[index] != null)
        {
            spriteRenderers[index].sprite = inactiveSprites[index];
        }

        // Восстанавливаем масштаб
        if (runeObjects[index] != null)
        {
            runeObjects[index].transform.localScale = originalScales[index];
        }

        // Останавливаем частицы
        if (runeParticles[index] != null)
        {
            runeParticles[index].Stop();
        }
    }

    public bool AreAllRunesActivated()
    {
        return runesActivated[0] && runesActivated[1] && runesActivated[2];
    }
}
