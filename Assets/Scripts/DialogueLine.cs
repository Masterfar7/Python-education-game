using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public DialogueSpeaker speaker;

    [TextArea(2, 5)]
    public string text;

    public bool startTaskAfterLine;

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
}