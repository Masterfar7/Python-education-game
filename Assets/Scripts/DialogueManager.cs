using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton;

    [Header("Characters")]
    [SerializeField] private Sprite playerPortrait;
    [SerializeField] private Sprite guidePortrait;
    [SerializeField] private Sprite statuePortrait;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Header("Interpreter")]
    [SerializeField] private GameObject interpreterCanvas;

    [Header("Teleport Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform guide;
    [SerializeField] private Transform playerTeleportPoint;
    [SerializeField] private Transform guideTeleportPoint;
    [SerializeField] private PlayerMovement2D playerMovement;

    [Header("Effects")]
    [SerializeField] private StatueShake statueShake;
    [SerializeField] private StatueVisualEffects statueVisualEffects;
    [SerializeField] private StatueAuraController firstStatueAura;
    [SerializeField] private StatueAuraController secondStatueAura;

    [Header("Screen Fade")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Cinematic Movement")]
    [SerializeField] private float autoMoveSpeed = 5f;
    [SerializeField] private float autoMoveStopDistance = 0.05f;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Rigidbody2D guideRb;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private SpriteRenderer guideSprite;
    [SerializeField] private bool playerFlipXFacesRight = true;
    [SerializeField] private bool guideFlipXFacesRight = true;

    private DialogueLine[] lines;
    private int index;
    private bool isActive;
    private bool isTyping;
    private bool taskMode = false;
    private bool isMovingCharacters = false;
    private bool instantTextMode = false;   // для админского скипа
    private bool adminSkipUsed = false;     // скип можно использовать один раз
    private bool adminSkipInProgress = false;
    private int adminSkipTargetIndex = -1;
    private int adminSkipTargetTaskIndex = -1;
    private bool statueAwakened = false;

    private Coroutine typingCoroutine;
    private TaskSystem taskSystem;

    private void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
        nextButton.onClick.AddListener(OnNextButton);

        if (player != null && playerRb == null)
            playerRb = player.GetComponent<Rigidbody2D>();
        if (guide != null && guideRb == null)
            guideRb = guide.GetComponent<Rigidbody2D>();
        if (player != null && playerSprite == null)
            playerSprite = player.GetComponent<SpriteRenderer>();
        if (guide != null && guideSprite == null)
            guideSprite = guide.GetComponent<SpriteRenderer>();

        // Скрываем интерпретатор при старте
        if (interpreterCanvas != null)
            interpreterCanvas.SetActive(false);
    }

    private void Start()
    {
        taskSystem = FindObjectOfType<TaskSystem>();
    }

    public bool IsActive() => isActive;
    public bool IsTaskMode() => taskMode;

    public void StartDialogue(DialogueLine[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
            return;

        lines = dialogueLines;
        index = 0;
        isActive = true;
        taskMode = false;

        dialoguePanel.SetActive(true);
        nextButton.gameObject.SetActive(true);

        // Сбрасываем TaskSystem для нового диалога
        if (taskSystem != null)
            taskSystem.ResetTasks();

        ShowLine();
    }

    public void OnNextButton()
    {
        if (!isActive || taskMode || isMovingCharacters) return;

        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = lines[index].text;
            isTyping = false;
            return;
        }

        index++;

        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    // Вызывается из TaskSystem после правильного ответа
    public void OnTaskCompleted()
    {
        if (!isActive) return;

        // Выходим из taskMode
        taskMode = false;

        // Скрываем интерпретатор
        if (interpreterCanvas != null)
            interpreterCanvas.SetActive(false);

        // Разблокируем движение
        if (playerMovement != null)
            playerMovement.enabled = true;

        index++;

        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        // Показываем кнопку "далее"
        nextButton.gameObject.SetActive(true);

        // Показываем следующую реплику
        ShowLine();
    }

    private void ShowLine()
    {
        DialogueLine line = lines[index];

        // Остановить старую тряску
        if (statueShake != null)
            statueShake.StopShake();

        // Флаг "особой" реплики статуи (пробуждение с глазами)
        bool isStatueFlashLine = line.triggerStatueFlash ||
                                 (!string.IsNullOrEmpty(line.text) && line.text.Contains("ПРО-БУ-ДИЛ-СЯ"));

        switch (line.speaker)
        {
            case DialogueSpeaker.Player:
                nameText.text = "Кай";
                portraitImage.sprite = playerPortrait;
                portraitImage.rectTransform.localScale = new Vector3(-1, 1, 1);
                nameText.gameObject.SetActive(true);
                portraitImage.gameObject.SetActive(true);
                break;

            case DialogueSpeaker.Guide:
                nameText.text = "Аргус";
                portraitImage.sprite = guidePortrait;
                portraitImage.rectTransform.localScale = new Vector3(1, 1, 1);
                nameText.gameObject.SetActive(true);
                portraitImage.gameObject.SetActive(true);
                break;

            case DialogueSpeaker.Statue:
                nameText.text = "Статуя";
                portraitImage.sprite = statuePortrait;
                portraitImage.rectTransform.localScale = new Vector3(1, 1, 1);
                nameText.gameObject.SetActive(true);
                portraitImage.gameObject.SetActive(true);

                // Не трясём статую в момент вспышки глаз
                if (statueShake != null && !isStatueFlashLine)
                    statueShake.StartShake(0.04f);

                if (statueVisualEffects != null && isStatueFlashLine)
                    statueVisualEffects.PlayFlashAndEyes();
                break;

            case DialogueSpeaker.System:
                // Системный диалог: без имени и портрета
                nameText.gameObject.SetActive(false);
                portraitImage.gameObject.SetActive(false);
                break;
        }

        // Включаем ауры статуй, если указано на этой реплике
        if (line.enableFirstStatueAura && firstStatueAura != null)
            firstStatueAura.EnableAura();

        if (line.enableSecondStatueAura && secondStatueAura != null)
            secondStatueAura.EnableAura();

        // Активируем рычаг, если указано на этой реплике
        if (line.activateLever && line.leverToActivate != null)
            line.leverToActivate.ActivateLever();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(line.text));

        // Если после этой реплики должно быть задание
        if (line.startTaskAfterLine)
            ActivateTaskMode();

        // Кинематическое движение персонажей по этой реплике
        if (line.moveCharacters)
            StartCoroutine(MoveCharactersToTargets(line));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        if (instantTextMode)
        {
            // Мгновенно выводим весь текст (для админского режима)
            dialogueText.text = text;
            isTyping = false;
            yield break;
        }

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void ActivateTaskMode()
    {
        DialogueLine line = lines[index];

        // В админском скипе: сразу применяем телепорт и считаем задачу выполненной,
        // без открытия интерфейса задания.
        if (adminSkipInProgress)
        {
            if (line.teleportPlayers)
            {
                Transform playerTarget = line.playerTeleportTarget != null
                    ? line.playerTeleportTarget
                    : playerTeleportPoint;
                if (player != null && playerTarget != null)
                    player.position = playerTarget.position;

                Transform guideTarget = line.guideTeleportTarget != null
                    ? line.guideTeleportTarget
                    : guideTeleportPoint;
                if (guide != null && guideTarget != null)
                    guide.position = guideTarget.position;
            }

            // Сразу переходим к следующей строке, как при завершении задачи
            OnTaskCompleted();
            return;
        }

        // Если есть CanvasGroup для затемнения — используем плавный переход
        // и на этой реплике вообще должен быть телепорт
        if (fadeCanvas != null && fadeDuration > 0f && line.teleportPlayers)
        {
            StartCoroutine(ActivateTaskModeWithFade());
        }
        else
        {
            ActivateTaskModeInstant();
        }
    }

    private void ActivateTaskModeInstant()
    {
        taskMode = true;

        // Убираем стрелку
        nextButton.gameObject.SetActive(false);

        // Телепортируем только если на этой реплике включён телепорт
        DialogueLine line = lines[index];
        if (line.teleportPlayers)
        {
            // Телепорт игрока
            Transform playerTarget = line.playerTeleportTarget != null
                ? line.playerTeleportTarget
                : playerTeleportPoint;
            if (player != null && playerTarget != null)
                player.position = playerTarget.position;

            // Телепорт гида
            Transform guideTarget = line.guideTeleportTarget != null
                ? line.guideTeleportTarget
                : guideTeleportPoint;
            if (guide != null && guideTarget != null)
                guide.position = guideTarget.position;
        }

        // Блокируем движение
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Показываем интерпретатор
        if (interpreterCanvas != null)
            interpreterCanvas.SetActive(true);
    }

    private IEnumerator ActivateTaskModeWithFade()
    {
        taskMode = true;

        // Убираем стрелку
        nextButton.gameObject.SetActive(false);

        // Блокируем ввод по UI во время затемнения
        fadeCanvas.blocksRaycasts = true;

        // В админском скипе пропускаем визуальный фейд:
        // сразу телепортируем и считаем задачу выполненной.
        if (adminSkipInProgress)
        {
            DialogueLine lineInstant = lines[index];

            if (lineInstant.teleportPlayers)
            {
                Transform playerTarget = lineInstant.playerTeleportTarget != null
                    ? lineInstant.playerTeleportTarget
                    : playerTeleportPoint;
                if (player != null && playerTarget != null)
                    player.position = playerTarget.position;

                Transform guideTarget = lineInstant.guideTeleportTarget != null
                    ? lineInstant.guideTeleportTarget
                    : guideTeleportPoint;
                if (guide != null && guideTarget != null)
                    guide.position = guideTarget.position;
            }

            if (playerMovement != null)
                playerMovement.enabled = false;

            if (interpreterCanvas != null)
                interpreterCanvas.SetActive(false);

            OnTaskCompleted();
            yield break;
        }

        // Обычный режим: плавное затемнение экрана
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // Телепортируем только если на этой реплике включён телепорт
        DialogueLine line = lines[index];
        if (line.teleportPlayers)
        {
            // Телепорт игрока
            Transform playerTarget = line.playerTeleportTarget != null
                ? line.playerTeleportTarget
                : playerTeleportPoint;
            if (player != null && playerTarget != null)
                player.position = playerTarget.position;

            // Телепорт гида
            Transform guideTarget = line.guideTeleportTarget != null
                ? line.guideTeleportTarget
                : guideTeleportPoint;
            if (guide != null && guideTarget != null)
                guide.position = guideTarget.position;
        }

        // Блокируем движение
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Показываем интерпретатор
        if (interpreterCanvas != null)
            interpreterCanvas.SetActive(true);

        // Плавное возвращение к игре
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(1f - (t / fadeDuration));
            yield return null;
        }
        fadeCanvas.alpha = 0f;

        // Разблокируем ввод по UI
        fadeCanvas.blocksRaycasts = false;
    }

    private IEnumerator MoveCharactersToTargets(DialogueLine line)
    {
        isMovingCharacters = true;

        // Блокируем вход игрока на время движения
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (nextButton != null)
            nextButton.interactable = false;

        // Подготовка маршрутов: если массив точек задан — идём по нему,
        // иначе используем одиночную цель.
        Transform[] playerRoute = (line.playerPath != null && line.playerPath.Length > 0)
            ? line.playerPath
            : (line.playerMoveTarget != null ? new[] { line.playerMoveTarget } : null);

        Transform[] guideRoute = (line.guidePath != null && line.guidePath.Length > 0)
            ? line.guidePath
            : (line.guideMoveTarget != null ? new[] { line.guideMoveTarget } : null);

        int playerRouteIndex = 0;
        int guideRouteIndex = 0;

        // Выбираем точные скорости для каждого:
        // 1) если задана персональная скорость > 0 — используем её,
        // 2) иначе общая из реплики,
        // 3) иначе глобальную autoMoveSpeed.
        float playerSpeed = line.playerMoveSpeed > 0f
            ? line.playerMoveSpeed
            : (line.moveSpeedOverride > 0f ? line.moveSpeedOverride : autoMoveSpeed);

        float guideSpeed = line.guideMoveSpeed > 0f
            ? line.guideMoveSpeed
            : (line.moveSpeedOverride > 0f ? line.moveSpeedOverride : autoMoveSpeed);

        while (true)
        {
            // Актуализируем, есть ли ещё точки маршрута
            bool playerHasTarget = player != null && playerRoute != null && playerRouteIndex < playerRoute.Length;
            bool guideHasTarget = guide != null && guideRoute != null && guideRouteIndex < guideRoute.Length;

            bool playerDone = !playerHasTarget;
            bool guideDone = !guideHasTarget;

            if (playerHasTarget)
            {
                Transform targetPoint = playerRoute[playerRouteIndex];
                Vector2 current = playerRb != null ? playerRb.position : (Vector2)player.position;
                Vector2 target = targetPoint.position;
                Vector2 toTarget = target - current;

                if (toTarget.magnitude <= autoMoveStopDistance)
                {
                    playerDone = true;
                    if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
                    else player.position = target;

                    // Переходим к следующей точке маршрута, если есть
                    playerRouteIndex++;
                }
                else
                {
                    Vector2 dir = toTarget.normalized;
                    if (playerRb != null)
                        playerRb.linearVelocity = dir * playerSpeed;
                    else
                        player.position = Vector2.MoveTowards(player.position, target, playerSpeed * Time.deltaTime);

                    // Разворачиваем игрока по направлению движения
                    if (playerSprite != null && Mathf.Abs(dir.x) > 0.01f)
                    {
                        // С учётом того, как настроен спрайт в инспекторе
                        playerSprite.flipX = playerFlipXFacesRight ? dir.x > 0f : dir.x < 0f;
                    }
                }
            }

            if (guideHasTarget)
            {
                Transform targetPoint = guideRoute[guideRouteIndex];
                Vector2 current = guideRb != null ? guideRb.position : (Vector2)guide.position;
                Vector2 target = targetPoint.position;
                Vector2 toTarget = target - current;

                if (toTarget.magnitude <= autoMoveStopDistance)
                {
                    guideDone = true;
                    if (guideRb != null) guideRb.linearVelocity = Vector2.zero;
                    else guide.position = target;

                    // Переходим к следующей точке маршрута, если есть
                    guideRouteIndex++;
                }
                else
                {
                    Vector2 dir = toTarget.normalized;
                    if (guideRb != null)
                        guideRb.linearVelocity = dir * guideSpeed;
                    else
                        guide.position = Vector2.MoveTowards(guide.position, target, guideSpeed * Time.deltaTime);

                    // Гид смотрит в сторону движения
                    if (guideSprite != null && Mathf.Abs(dir.x) > 0.01f)
                    {
                        guideSprite.flipX = guideFlipXFacesRight ? dir.x > 0f : dir.x < 0f;
                    }
                }
            }

            if (playerDone && guideDone)
                break;

            yield return null;
        }

        // Останавливаем скорость
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
        if (guideRb != null) guideRb.linearVelocity = Vector2.zero;

        // Смотрят друг на друга по горизонтали
        if (player != null && guide != null && playerSprite != null && guideSprite != null)
        {
            // Игрок и гид встают лицом друг к другу
            bool playerOnLeft = player.position.x < guide.position.x;

            // Игрок: слева должен смотреть вправо, справа — влево
            bool playerShouldFaceRight = playerOnLeft;
            playerSprite.flipX = playerFlipXFacesRight
                ? playerShouldFaceRight
                : !playerShouldFaceRight;

            // Гид: если игрок слева, гид справа и смотрит влево; если игрок справа — наоборот
            bool guideShouldFaceRight = !playerOnLeft;
            guideSprite.flipX = guideFlipXFacesRight
                ? guideShouldFaceRight
                : !guideShouldFaceRight;
        }

        // Разблокируем кнопку "далее"
        if (nextButton != null)
            nextButton.interactable = true;

        // Возвращаем управление игроку, если не в режиме задания
        if (!taskMode && playerMovement != null)
            playerMovement.enabled = true;

        isMovingCharacters = false;
    }

    private void EndDialogue()
    {
        isActive = false;
        taskMode = false;
        dialoguePanel.SetActive(false);

        // Скрываем интерпретатор
        if (interpreterCanvas != null)
            interpreterCanvas.SetActive(false);

        // Разблокируем движение
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Остановить тряску
        if (statueShake != null)
            statueShake.StopShake();

        statueAwakened = false;
    }

    /// <summary>
    /// Админский скип: один раз запускает авто-пролистку текущего диалога
    /// до указанной реплики (включительно), последовательно выполняя все действия (как будто игрок
    /// нажимает "далее" и правильно отвечает на задачи).
    /// </summary>
    public void AdminSkipToLine(int targetIndex)
    {
        if (!isActive || lines == null || lines.Length == 0)
            return;

        if (adminSkipUsed)
            return;

        // Нормализуем индекс и запоминаем цель
        adminSkipTargetIndex = Mathf.Clamp(targetIndex, 0, lines.Length - 1);

        // Автоматически определяем, какое по счёту задание актуально на этой реплике,
        // считая количество флажков startTaskAfterLine до целевой строки.
        adminSkipTargetTaskIndex = -1;
        if (taskSystem != null)
        {
            int taskCountBefore = 0;
            for (int i = 0; i <= adminSkipTargetIndex && i < lines.Length; i++)
            {
                if (lines[i].startTaskAfterLine)
                    taskCountBefore++;
            }

            // Если есть хотя бы одно задание до/на этой строке,
            // текущим считаем следующее по счёту задание.
            // AdminSetCurrentTaskIndex сам зажмёт индекс в диапазон tasks.
            if (taskCountBefore > 0)
                adminSkipTargetTaskIndex = taskCountBefore;
        }

        adminSkipUsed = true;
        StartCoroutine(AdminSkipRoutine());
    }

    private IEnumerator AdminSkipRoutine()
    {
        adminSkipInProgress = true;
        instantTextMode = true;

        // Если указана целевая задача — синхронизируем индекс задач с перемоткой диалога
        if (taskSystem != null && adminSkipTargetTaskIndex >= 0)
        {
            taskSystem.AdminSetCurrentTaskIndex(adminSkipTargetTaskIndex);
        }

        // Текущая строка уже показана, начинаем листать дальше до нужного индекса
        while (isActive && index < lines.Length - 1)
        {
            // Ждём, пока закончатся движения персонажей и режим задания,
            // чтобы телепорты/кинетика успевали полностью отработать.
            while (isMovingCharacters || taskMode)
            {
                yield return null;
            }

            index++;
            ShowLine();

            // небольшая пауза между строками, чтобы все корутины
            // (телепорты, движение и т.п.) успели сделать шаг
            yield return new WaitForSeconds(0.1f);

            // Если достигли целевой реплики — останавливаем скип
            if (index >= adminSkipTargetIndex)
                break;
        }

        adminSkipInProgress = false;
        instantTextMode = false;
    }

    public void ReplaceCurrentText(string newText)
    {
        if (!isActive) return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = false;
        dialogueText.text = newText;
    }
}