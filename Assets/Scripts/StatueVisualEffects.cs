using System.Collections;
using UnityEngine;

public class StatueVisualEffects : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer eyesRenderer;
    [SerializeField] private SpriteRenderer baseStatueRenderer;

    [Header("Timings")]
    [SerializeField] private float eyesFadeInDuration = 0.25f;

    private Coroutine effectCoroutine;
    private Color eyesBaseColor;

    private void Awake()
    {
        if (eyesRenderer != null)
        {
            eyesBaseColor = eyesRenderer.color;
            var c = eyesBaseColor;
            c.a = 0f;
            eyesRenderer.color = c;
        }
    }

    public void PlayFlashAndEyes()
    {
        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        effectCoroutine = StartCoroutine(FlashAndEyesRoutine());
    }

    private IEnumerator FlashAndEyesRoutine()
    {
        float t = 0f;

        // Подготовка
        if (baseStatueRenderer != null)
            baseStatueRenderer.enabled = false;

        if (eyesRenderer != null)
        {
            var c = eyesBaseColor;
            c.a = 0f;
            eyesRenderer.color = c;
        }

        // Плавное загорание глаз
        t = 0f;
        while (t < eyesFadeInDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / eyesFadeInDuration);

            if (eyesRenderer != null)
            {
                var c = eyesBaseColor;
                c.a = a;
                eyesRenderer.color = c;
            }

            yield return null;
        }

        if (eyesRenderer != null)
        {
            var c = eyesBaseColor;
            c.a = 1f;
            eyesRenderer.color = c;
        }

        effectCoroutine = null;
    }
}

