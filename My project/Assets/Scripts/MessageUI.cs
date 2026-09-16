using UnityEngine;
using TMPro;
using System.Collections;

public class MessageUI : MonoBehaviour
{
    public static MessageUI Instance;

    [Header("UI Элементы")]
    public GameObject messagePanel;
    public CanvasGroup messagePanelCG;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI messageText;

    [Header("Настройки")]
    public float defaultDuration = 3f;
    public float fadeTime = 0.3f;

    [Header("Эффект печати")]
    public float typingSpeed = 0.03f;
    public bool autoHideAfterTyping = false;

    [Header("Звук печати")]
    public AudioClip typeSound;
    [Range(0f, 1f)] public float typeVolume = 0.3f;
    private AudioSource audioSource;

    private Coroutine currentRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (messagePanel != null) messagePanel.SetActive(false);
        if (messagePanelCG != null) messagePanelCG.alpha = 0f;


        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = typeVolume;
    }

    public void Show(string speaker, string message, float duration = -1f)
    {
        if (duration < 0f) duration = defaultDuration;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(DisplayRoutine(speaker, message, duration));
    }

    private IEnumerator DisplayRoutine(string speaker, string message, float duration)
    {
        if (speakerText != null) speakerText.text = speaker;
        if (messageText != null)
        {
            messageText.text = message;
            messageText.maxVisibleCharacters = 0;
        }

        if (messagePanel != null) messagePanel.SetActive(true);
        yield return Fade(0f, 1f, fadeTime);

        if (messageText != null)
        {
            int totalChars = messageText.text.Length;
            for (int i = 1; i <= totalChars; i++)
            {
                messageText.maxVisibleCharacters = i;

                // Звук каждые 2 символа 
                if (typeSound != null && i % 2 == 0)
                    audioSource.PlayOneShot(typeSound, typeVolume);

                yield return new WaitForSecondsRealtime(typingSpeed);
            }
        }

        if (!autoHideAfterTyping)
            yield return new WaitForSecondsRealtime(duration);

        yield return Fade(1f, 0f, fadeTime);
        if (messagePanel != null) messagePanel.SetActive(false);

        currentRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.unscaledDeltaTime;
            if (messagePanelCG != null)
                messagePanelCG.alpha = Mathf.Lerp(from, to, elapsed / time);
            yield return null;
        }
        if (messagePanelCG != null) messagePanelCG.alpha = to;
    }
}