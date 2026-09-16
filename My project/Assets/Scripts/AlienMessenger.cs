using UnityEngine;
using System.Collections;

public class AlienMessenger : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private MessageUI messageUI;
    [SerializeField] private PlayerController playerController;

    [Header("Диалоги")]
    [TextArea(2, 6)][SerializeField] private string[] dialogueBefore;
    [TextArea(2, 6)][SerializeField] private string[] dialogueAfter;

    [Header("Настройки")]
    [SerializeField] private float messageDuration = 3.5f;
    [SerializeField] private bool isRepeatable = false;

    private bool hasPlayedBefore = false;
    private bool hasPlayedAfter = false;
    private bool isPlaying = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isPlaying) return;
        if (hasPlayedBefore && !isRepeatable) return;

        StartCoroutine(PlaySequence(dialogueBefore, () => hasPlayedBefore = true));
    }

    public void PlayAfterPuzzleDialogue()
    {
        if (hasPlayedAfter || isPlaying) return;
        StartCoroutine(PlaySequence(dialogueAfter, () => hasPlayedAfter = true));
    }

    IEnumerator PlaySequence(string[] lines, System.Action onComplete)
    {
        if (messageUI == null || lines == null || lines.Length == 0) yield break;
        isPlaying = true;

        playerController?.SetControl(false);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            messageUI.Show("Связь", line, messageDuration);
            yield return new WaitForSecondsRealtime(messageDuration);
        }

        playerController?.SetControl(true);
        isPlaying = false;
        onComplete?.Invoke();
    }

#if UNITY_EDITOR
    [ContextMenu("Сбросить состояние")]
    void ResetState()
    {
        hasPlayedBefore = false;
        hasPlayedAfter = false;
        isPlaying = false;
        StopAllCoroutines();
    }
#endif
}