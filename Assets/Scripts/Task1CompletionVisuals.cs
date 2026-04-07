using System.Collections;
using UnityEngine;

/// <summary>
/// Визуал после заданий: задание 1 — кристаллы + мигание, цветок; задание 2 — отдельные кристаллы + мигание.
/// </summary>
public class Task1CompletionVisuals : MonoBehaviour
{
    [Header("Задание 1 (индекс 0)")]
    [Tooltip("Если выключено, после 1-го задания эффекты не запускаются.")]
    public bool enableForTask1 = true;

    [Header("Кристаллы — задание 1")]
    public Task1CrystalEntry[] crystals;

    [Header("Цветок — задание 1")]
    public Task1FlowerSettings flower;

    [Header("Задание 2 (индекс 1)")]
    [Tooltip("Если включено, после 2-го задания смена спрайта и мигание по списку ниже (не останавливает мигание с 1-го задания).")]
    public bool enableForTask2;

    [Header("Кристаллы — задание 2")]
    public Task1CrystalEntry[] crystalsTask2;

    public void PlayTask1()
    {
        if (!enableForTask1)
            return;

        StopAllCoroutines();
        RunCrystalRewards(crystals);

        if (flower != null &&
            flower.spriteRenderer != null &&
            flower.frames != null &&
            flower.frames.Length > 0)
        {
            StartCoroutine(FlowerFrameRoutineOnce());
        }
    }

    /// <summary>Только кристаллы второго задания; не вызывает StopAllCoroutines.</summary>
    public void PlayTask2()
    {
        if (!enableForTask2)
            return;

        RunCrystalRewards(crystalsTask2);
    }

    void RunCrystalRewards(Task1CrystalEntry[] list)
    {
        if (list == null)
            return;

        foreach (Task1CrystalEntry c in list)
        {
            if (c == null || c.spriteRenderer == null || c.activatedSprite == null)
                continue;

            c.spriteRenderer.sprite = c.activatedSprite;
            if (c.playEnergyEffect)
                StartCoroutine(CrystalBlinkRoutine(c.spriteRenderer));
        }
    }

    IEnumerator CrystalBlinkRoutine(SpriteRenderer sr)
    {
        Color tint0 = new Color(0.75f, 0.92f, 1f, sr.color.a);
        Color tint1 = new Color(1f, 1f, 1.15f, sr.color.a);
        float t0 = Time.time;

        while (sr != null)
        {
            float t = Time.time - t0;
            float w = Mathf.Sin(t * 7.9f);
            sr.color = Color.Lerp(tint0, tint1, 0.5f + 0.5f * w);
            yield return null;
        }
    }

    IEnumerator FlowerFrameRoutineOnce()
    {
        SpriteRenderer sr = flower.spriteRenderer;
        Sprite[] frames = flower.frames;
        float wait = Mathf.Max(0.02f, flower.secondsPerFrame);

        if (sr == null || frames == null || frames.Length == 0)
            yield break;

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] != null)
                sr.sprite = frames[i];
            if (i < frames.Length - 1)
                yield return new WaitForSeconds(wait);
        }
    }
}

[System.Serializable]
public class Task1CrystalEntry
{
    public SpriteRenderer spriteRenderer;
    public Sprite activatedSprite;

    [Tooltip("Мигание цветом (позиция и поворот не меняются).")]
    public bool playEnergyEffect = true;
}

[System.Serializable]
public class Task1FlowerSettings
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] frames;

    [Tooltip("Пауза между кадрами анимации цветка (один проход, затем последний кадр).")]
    [Min(0.02f)]
    public float secondsPerFrame = 0.18f;
}
