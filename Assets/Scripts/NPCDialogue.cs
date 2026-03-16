using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public DialogueLine[] dialogueLines;
    private bool playerInRange;

    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!DialogueManager.Instance.IsActive())
            {
                DialogueManager.Instance.StartDialogue(dialogueLines);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}