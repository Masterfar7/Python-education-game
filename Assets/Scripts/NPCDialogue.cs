using System.Collections;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public DialogueLine[] dialogueLines;
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
        DialogueManager.Instance.StartDialogue(dialogueLines);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        TryStartDialogue();
    }

}
