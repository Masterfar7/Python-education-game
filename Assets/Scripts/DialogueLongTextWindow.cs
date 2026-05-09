using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueLongTextWindow : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private GameObject longTextPanel;
    [SerializeField] private TextMeshProUGUI longTextContent;
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Header("Auto Scroll")]
    [SerializeField] private bool autoScrollDown = true;
    [SerializeField] private float autoScrollSpeed = 2f;

    private bool isTyping = false;
    private System.Collections.IEnumerator typingCoroutine;
    private System.Collections.IEnumerator autoScrollCoroutine;

    private void Awake()
    {
        if (longTextPanel != null)
            longTextPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButton);
    }

    /// <summary>
    /// Показывает окно с длинным текстом
    /// </summary>
    public void ShowLongText(string text)
    {
        if (longTextPanel != null)
            longTextPanel.SetActive(true);

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        if (longTextContent != null)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = TypeText(text);
            StartCoroutine(typingCoroutine);
        }

        if (autoScrollDown && scrollRect != null)
        {
            if (autoScrollCoroutine != null)
                StopCoroutine(autoScrollCoroutine);

            autoScrollCoroutine = AutoScrollDown();
            StartCoroutine(autoScrollCoroutine);
        }
    }

    private void OnEnable()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButton);
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseButton);
    }

    /// <summary>
    /// Скрывает окно с длинным текстом
    /// </summary>
    public void HideLongText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (autoScrollCoroutine != null)
        {
            StopCoroutine(autoScrollCoroutine);
            autoScrollCoroutine = null;
        }

        isTyping = false;

        if (longTextPanel != null)
            longTextPanel.SetActive(false);

        if (longTextContent != null)
            longTextContent.text = "";
    }

    /// <summary>
    /// Печатает текст посимвольно
    /// </summary>
    private System.Collections.IEnumerator TypeText(string text)
    {
        isTyping = true;
        longTextContent.text = "";

        foreach (char c in text)
        {
            longTextContent.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        if (autoScrollCoroutine != null)
        {
            StopCoroutine(autoScrollCoroutine);
            autoScrollCoroutine = null;
        }
    }

    private System.Collections.IEnumerator AutoScrollDown()
    {
        while (isTyping)
        {
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = Mathf.MoveTowards(
                    scrollRect.verticalNormalizedPosition,
                    0f,
                    autoScrollSpeed * Time.deltaTime
                );
            }
            yield return null;
        }
    }

    /// <summary>
    /// Обработчик кнопки закрытия
    /// </summary>
    private void OnCloseButton()
    {
        // Если текст еще печатается - показываем весь текст сразу
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            isTyping = false;
            // Текст уже частично напечатан, просто останавливаем анимацию
        }
        else
        {
            // Текст полностью напечатан - закрываем окно
            HideLongText();
        }
    }

    /// <summary>
    /// Проверяет, активно ли окно
    /// </summary>
    public bool IsActive()
    {
        return longTextPanel != null && longTextPanel.activeSelf;
    }

    /// <summary>
    /// Проверяет, печатается ли текст
    /// </summary>
    public bool IsTyping()
    {
        return isTyping;
    }

    /// <summary>
    /// Мгновенно показывает весь текст (для пропуска анимации)
    /// </summary>
    public void ShowFullText(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        if (longTextContent != null)
            longTextContent.text = text;

        if (longTextPanel != null)
            longTextPanel.SetActive(true);
    }
}
