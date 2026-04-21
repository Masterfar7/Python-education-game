using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public DialogueSpeaker speaker;

    [TextArea(2, 5)]
    public string text;

    public bool startTaskAfterLine;

    [Tooltip("Испытание с маной: показывает UI маны")]
    public bool isManaChallenge;

    [Tooltip("Запустить рост сорняков на этой фразе")]
    public bool startWeedsGrowth;

    [Tooltip("Запустить гибель сорняков после этой фразы")]
    public bool startWeedsDeath;

    [Tooltip("Количество маны, отнимаемое за каждую фразу в испытании с маной (0 = не отнимать на этой фразе)")]
    public int manaDecreaseAmount = 10;

    [Tooltip("После полной печати текста этой реплики показать UI на объекте с DialogueTaskObjectUI.")]
    public bool showTaskObjectUIAfterText;

    public DialogueTaskObjectUI taskObjectUI;

    [Tooltip("Завершение главы: после полной печати этой реплики показать указанный UI-объект (SetActive(true)).")]
    public bool showChapterCompleteUIAfterText;

    public GameObject chapterCompleteUI;

    // Включить вспышку и сияющие глаза статуи на этой реплике
    public bool triggerStatueFlash;

    // Включить ауры статуй на этой реплике
    public bool enableFirstStatueAura;
    public bool enableSecondStatueAura;

    // Телепортировать игрока и/или гида после этой реплики
    public bool teleportPlayers;
    public Transform playerTeleportTarget;
    public Transform guideTeleportTarget;

    // Оба персонажа идут в указанные точки
    public bool moveCharacters;
    public Transform playerMoveTarget;
    public Transform guideMoveTarget;

    // Маршрут для движения (несколько точек по порядку)
    public Transform[] playerPath;
    public Transform[] guidePath;

    // Управление рычагом / объектом на этой реплике
    public bool activateLever;
    public LeverController leverToActivate;

    // Общая скорость движения (-1 = использовать значение по умолчанию из DialogueManager)
    public float moveSpeedOverride = -1f;

    // Точная скорость для каждого (если > 0, перекрывает moveSpeedOverride)
    public float playerMoveSpeed = -1f;
    public float guideMoveSpeed = -1f;

    // Скрыть окно диалога на указанное время (в секундах)
    public bool hideDialogueWindow;
    public float hideDialogueWindowDuration = 2f;

    [Header("Walking Dialogue (Chapter 3)")]
    [Tooltip("Персонажи идут во время диалога, можно листать фразы")]
    public bool walkingDialogue;

    [Tooltip("Количество фраз, которые можно пролистать пока персонажи идут")]
    public int walkingDialoguePhrasesCount = 3;

    [Header("Spirits Animation (Chapter 3)")]
    [Tooltip("Появление 3 духов на этой реплике")]
    public bool spawnSpirits;
    public GameObject[] spirits; // Массив из 3 духов

    [Tooltip("Запустить анимацию духов после выполнения задания")]
    public bool animateSpiritsAfterTask;

    [Tooltip("Скорость смены спрайтов духов (секунды между кадрами)")]
    public float spiritAnimationFrameRate = 0.1f;

    [Header("Runes Activation (Chapter 3)")]
    [Tooltip("Активировать 3 руны после выполнения задания")]
    public bool activateRunesAfterTask;
    public ThreeRunesActivation runesController;
}