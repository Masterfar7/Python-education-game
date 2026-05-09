using System.Collections;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public DialogueLine[] dialogueLines;
    private bool dialogueAutoStarted;

    [Tooltip("Запустить диалог сразу при старте сцены (без триггера)")]
    public bool startOnSceneLoad = false;

    private void Start()
    {
        if (startOnSceneLoad)
        {
            TryStartDialogue();
        }
        else
        {
            StartCoroutine(CheckPlayerAlreadyInTriggerNextFrame());
        }
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

    public void TryStartDialogue()
    {
        if (dialogueAutoStarted) return;
        if (DialogueManager.Instance == null) return;
        if (DialogueManager.Instance.IsActive()) return;
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        dialogueAutoStarted = true;
        int startIdx = DialogueManager.PendingStartIndex;
        DialogueManager.Instance.StartDialogue(dialogueLines, startIdx);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        TryStartDialogue();
    }

}
