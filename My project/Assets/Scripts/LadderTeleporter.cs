using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;

public class LadderTeleporter : MonoBehaviour
{
    [Header("Точки телепортации")]
    public Transform pointA;
    public Transform pointB;

    [Header("Настройки")]
    public float interactRadius = 1.5f;
    public float fadeDuration = 0.8f;

    [Header("UI Подсказка")]
    private bool isLadderPromptActive = false;

    [Header("Блокировка лестницы")]
    public string stairId;


    private Transform playerTransform;
    private Transform activePoint;
    private Transform targetPoint;
    private bool isTeleporting = false;



    void Start()
    {

        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) playerTransform = pc.transform;


    }

    void Update()
    {
        if (isTeleporting || playerTransform == null)
        {
            return;
        }

        activePoint = null;
        targetPoint = null;

        float distA = Vector2.Distance(playerTransform.position, pointA.position);
        float distB = Vector2.Distance(playerTransform.position, pointB.position);

        if (distA < interactRadius)
        {
            activePoint = pointA;
            targetPoint = pointB;
        }
        else if (distB < interactRadius)
        {
            activePoint = pointB;
            targetPoint = pointA;
        }

        //if (interactPromptCG != null)
        //{
        //    interactPromptCG.alpha = activePoint != null ? 1f : 0f;
        //}
        bool shouldShow = activePoint != null;

        if (shouldShow && !isLadderPromptActive)
        {
            PromptManager.Instance?.Show();
            isLadderPromptActive = true;
        }
        else if (!shouldShow && isLadderPromptActive)
        {
            PromptManager.Instance?.Hide();
            isLadderPromptActive = false;
        }


        if (activePoint != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            HandleInteract();
        }
    }

    void HandleInteract()
    {
        if (!string.IsNullOrEmpty(stairId) && !StaircaseManager.Instance.IsUnlocked(stairId))
        {
            MessageUI.Instance?.Show("СИСТЕМА", "Мне пока туда не нужно", 2.5f);
            return;
        }

        StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {
        isTeleporting = true;
        PromptManager.Instance?.Hide();

        PlayerController pc = playerTransform.GetComponent<PlayerController>();
        pc?.SetControl(false);

        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeOut(fadeDuration);

        playerTransform.position = targetPoint.position;

        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeIn(fadeDuration);

        pc?.SetControl(true);
        isTeleporting = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (pointA) Gizmos.DrawWireSphere(pointA.position, interactRadius);
        if (pointB) Gizmos.DrawWireSphere(pointB.position, interactRadius);
    }
}