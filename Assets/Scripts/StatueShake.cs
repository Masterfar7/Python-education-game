using UnityEngine;
using System.Collections;

public class StatueShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void StartShake(float strength = 0.05f)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeLoop(strength));
    }

    public void StopShake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        transform.localPosition = originalPosition;
    }

    private IEnumerator ShakeLoop(float strength)
    {
        while (true)
        {
            float offsetX = Random.Range(-strength, strength);
            float offsetY = Random.Range(-strength, strength);

            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

            yield return null;
        }
    }
}