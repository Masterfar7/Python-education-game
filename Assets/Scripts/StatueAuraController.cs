using UnityEngine;

public class StatueAuraController : MonoBehaviour
{
    [Header("Renderer статуи")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Материал ауры")]
    [SerializeField] private Material auraMaterial;

    private Material originalMaterial;

    private void Awake()
    {
        if (targetRenderer != null)
            originalMaterial = targetRenderer.sharedMaterial;

        // В начале игры статуя без ауры
        DisableAura();
    }

    public void EnableAura()
    {
        if (targetRenderer == null || auraMaterial == null)
            return;

        targetRenderer.material = auraMaterial;
    }

    public void DisableAura()
    {
        if (targetRenderer == null || originalMaterial == null)
            return;

        targetRenderer.material = originalMaterial;
    }
}

