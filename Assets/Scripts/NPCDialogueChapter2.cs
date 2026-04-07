using System.Collections;
using UnityEngine;

public class NPCDialogueChapter2 : MonoBehaviour
{
    [Header("Диалог")]
    public DialogueLine[] dialogueLines;

    [Header("Движение персонажей (с первой реплики)")]
    [Tooltip("Если включено, игрок и гид идут по точкам ниже, пока идёт первая реплика (как moveCharacters в DialogueLine).")]
    public bool charactersWalk;
    public Transform[] playerWalkPath;
    public Transform[] guideWalkPath;

    private bool dialogueAutoStarted;

    private void Start()
    {
        StartCoroutine(CheckPlayerAlreadyInTriggerNextFrame());
    }

    private IEnumerator CheckPlayerAlreadyInTriggerNextFrame()
    {
        yield return null;
        if (dialogueAutoStarted) yield break;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) yield break;
        if (col.OverlapPoint(player.transform.position))
            TryStartDialogue();
    }

    private void TryStartDialogue()
    {
        if (dialogueAutoStarted) return;
        if (DialogueManager.Instance == null) return;
        if (DialogueManager.Instance.IsActive()) return;
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        dialogueAutoStarted = true;
        DialogueLine[] linesToPlay = BuildDialogueLinesForPlay();
        DialogueManager.Instance.StartDialogue(linesToPlay);
    }

    private DialogueLine[] BuildDialogueLinesForPlay()
    {
        if (!charactersWalk)
            return dialogueLines;

        DialogueLine[] copy = new DialogueLine[dialogueLines.Length];
        for (int i = 0; i < dialogueLines.Length; i++)
            copy[i] = CloneDialogueLine(dialogueLines[i]);

        DialogueLine first = copy[0];
        first.moveCharacters = true;
        first.playerPath = playerWalkPath;
        first.guidePath = guideWalkPath;
        return copy;
    }

    private static DialogueLine CloneDialogueLine(DialogueLine src)
    {
        if (src == null) return new DialogueLine();
        return new DialogueLine
        {
            speaker = src.speaker,
            text = src.text,
            startTaskAfterLine = src.startTaskAfterLine,
            showTaskObjectUIAfterText = src.showTaskObjectUIAfterText,
            taskObjectUI = src.taskObjectUI,
            showChapterCompleteUIAfterText = src.showChapterCompleteUIAfterText,
            chapterCompleteUI = src.chapterCompleteUI,
            triggerStatueFlash = src.triggerStatueFlash,
            enableFirstStatueAura = src.enableFirstStatueAura,
            enableSecondStatueAura = src.enableSecondStatueAura,
            teleportPlayers = src.teleportPlayers,
            playerTeleportTarget = src.playerTeleportTarget,
            guideTeleportTarget = src.guideTeleportTarget,
            moveCharacters = src.moveCharacters,
            playerMoveTarget = src.playerMoveTarget,
            guideMoveTarget = src.guideMoveTarget,
            playerPath = src.playerPath,
            guidePath = src.guidePath,
            activateLever = src.activateLever,
            leverToActivate = src.leverToActivate,
            moveSpeedOverride = src.moveSpeedOverride,
            playerMoveSpeed = src.playerMoveSpeed,
            guideMoveSpeed = src.guideMoveSpeed
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        TryStartDialogue();
    }
}
