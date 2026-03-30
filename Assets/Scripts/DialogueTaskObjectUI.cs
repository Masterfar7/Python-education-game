using UnityEngine;

/// <summary>
/// Вешается на объект с дочерними UI: скрыт при старте, показывается после печати текста задания,
/// скрывается и при необходимости удаляет другой объект после выполнения задания.
/// </summary>
public class DialogueTaskObjectUI : MonoBehaviour
{
    [Tooltip("Если не задан — показывается/скрывается этот GameObject.")]
    [SerializeField] private GameObject uiRoot;

    [Tooltip("Уничтожить после успешного выполнения задания (например декорация на сцене).")]
    [SerializeField] private GameObject objectToDestroyWhenTaskDone;

    private void Awake()
    {
        ApplyVisible(false);
    }

    public void ShowAfterTaskText()
    {
        ApplyVisible(true);
    }

    /// <summary>Скрыть UI без удаления объекта (новый диалог, прерывание).</summary>
    public void HideWithoutDestroy()
    {
        ApplyVisible(false);
    }

    /// <summary>После успешного задания: скрыть UI и удалить указанный объект.</summary>
    public void HideAfterTaskComplete()
    {
        ApplyVisible(false);

        if (objectToDestroyWhenTaskDone != null)
        {
            Destroy(objectToDestroyWhenTaskDone);
            objectToDestroyWhenTaskDone = null;
        }
    }

    private void ApplyVisible(bool visible)
    {
        GameObject root = uiRoot != null ? uiRoot : gameObject;
        if (root != null)
            root.SetActive(visible);
    }
}
