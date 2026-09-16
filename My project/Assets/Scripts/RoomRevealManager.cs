using UnityEngine;
using System.Collections;

public class RoomRevealManager : MonoBehaviour
{
    [Header("Объекты комнаты")]
    public Transform[] revealObjects;
    public Transform originPoint;

    [Header("Затемнение комнаты")]
    public SpriteRenderer roomBlackout;

    [Header("Спиральная траектория")]
    [Range(0, 5)] public float spiralRadius = 2f;
    [Range(0, 3)] public float spiralTurns = 1.5f;

    [Header("Время анимации")]
    public float objectsDuration = 2.5f;
    public float backgroundFadeDuration = 6.0f;

    [Header("Сообщения во время анимации (Большой Взрыв)")]
    public TimelineMessage[] timelineMessages;

    [Header("Завершение квеста")]
    public LaserDoor nextDoor;

    public Vector3 startScale = new Vector3(0.01f, 0.01f, 0.01f);
    public bool isRevealed { get; private set; } = false;

    private Vector3[] originalScales;
    private Vector3[] originalPositions;
    private Vector3[] startPositions;
    private int nextMessageIndex = 0;
    public QuestInteractable parentQuest;

    void Start()
    {
        if (roomBlackout != null)
            roomBlackout.color = new Color(0f, 0f, 0f, 1f);

        originalScales = new Vector3[revealObjects.Length];
        originalPositions = new Vector3[revealObjects.Length];
        startPositions = new Vector3[revealObjects.Length];

        for (int i = 0; i < revealObjects.Length; i++)
        {
            Transform obj = revealObjects[i];
            originalScales[i] = obj.localScale;
            originalPositions[i] = obj.localPosition;

            startPositions[i] = obj.parent != null
                ? obj.parent.InverseTransformPoint(originPoint.position)
                : originPoint.position;

            obj.localScale = startScale;
            obj.localPosition = startPositions[i];
        }
    }

    public void StartReveal(float delay = 0f)
    {
        if (isRevealed) return;
        isRevealed = true;
        nextMessageIndex = 0;
        StartCoroutine(DelayedStart(delay));
    }

    IEnumerator DelayedStart(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        StartCoroutine(AnimateReveal());
    }

    IEnumerator AnimateReveal()
    {
        float elapsed = 0f;
        float maxDuration = Mathf.Max(objectsDuration, backgroundFadeDuration);

        while (elapsed < maxDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            if (roomBlackout != null)
            {
                float bgT = Mathf.Clamp01(elapsed / backgroundFadeDuration);
                roomBlackout.color = new Color(0f, 0f, 0f, Mathf.Lerp(1f, 0f, bgT));
            }

            if (elapsed < objectsDuration)
            {
                float objT = elapsed / objectsDuration;
                float ease = 1f - Mathf.Pow(1f - objT, 3);

                for (int i = 0; i < revealObjects.Length; i++)
                {
                    Vector3 basePos = Vector3.Lerp(startPositions[i], originalPositions[i], ease);
                    float spiralFactor = Mathf.Sin(objT * Mathf.PI);
                    float angle = objT * spiralTurns * 360f;

                    Vector3 spiralOffset = Quaternion.Euler(0, 0, angle) * Vector3.up * (spiralFactor * spiralRadius);
                    spiralOffset.y += Mathf.Sin(objT * Mathf.PI * 2) * spiralRadius * 0.3f;

                    revealObjects[i].localPosition = basePos + spiralOffset;
                    revealObjects[i].localScale = Vector3.Lerp(startScale, originalScales[i], ease);
                }
            }
            else
            {
                for (int i = 0; i < revealObjects.Length; i++)
                {
                    revealObjects[i].localPosition = originalPositions[i];
                    revealObjects[i].localScale = originalScales[i];
                }
            }

            // вывод сообщений по таймлайну
            if (nextMessageIndex < timelineMessages.Length)
            {
                if (elapsed >= timelineMessages[nextMessageIndex].startTime)
                {
                    var msg = timelineMessages[nextMessageIndex];
                    MessageUI.Instance?.Show(msg.speaker, msg.message, msg.duration);
                    nextMessageIndex++;
                }
            }

            yield return null;
        }

        // фиксация и завершение
        if (roomBlackout != null) roomBlackout.color = new Color(0f, 0f, 0f, 0f);
        for (int i = 0; i < revealObjects.Length; i++)
        {
            revealObjects[i].localPosition = originalPositions[i];
            revealObjects[i].localScale = originalScales[i];
        }

        // разблокировка двери
        if (nextDoor != null) nextDoor.isLocked = false;
        MessageUI.Instance?.Show("СИСТЕМА", "Материализация завершена. Структура стабильна. Проход разрешён.", 4f);
        parentQuest?.OnPuzzleCompleted();
    }
}

[System.Serializable]
public class TimelineMessage
{
    public string speaker;
    public string message;
    [Tooltip("Через сколько секунд от начала анимации показать")]
    public float startTime;
    [Tooltip("Как долго висит на экране (секунды)")]
    public float duration;
}