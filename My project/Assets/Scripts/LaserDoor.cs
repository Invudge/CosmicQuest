using UnityEngine;
using UnityEngine.InputSystem;

public class LaserDoor : MonoBehaviour
{
    [Header("Компоненты")]
    public Animator animator;
    public Collider2D laserCollider;
    public float interactRadius = 2f;

    private bool isOpen = false;
    private Transform playerTransform;
    private bool isPlayerNear = false;

    public bool isLocked = true;

    void Start()
    {
        playerTransform = FindAnyObjectByType<PlayerController>()?.transform;
    }

    void Update()
    {
        if (isOpen || playerTransform == null) return;

        // Проверка дистанции
        float dist = Vector2.Distance(playerTransform.position, transform.position);
        bool wasNear = isPlayerNear;
        isPlayerNear = dist < interactRadius;

        // подсказка
        if (isPlayerNear && !wasNear)
            PromptManager.Instance?.Show();
        else if (!isPlayerNear && wasNear)
            PromptManager.Instance?.Hide();

        if (isPlayerNear && !isOpen && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isLocked)
            {
                MessageUI.Instance?.Show("СИСТЕМА", "Навигация отключена. Восстановите координаты корабля.", 4f);
                return;
            }

            OpenDoor();
        }
    }

    void OpenDoor()
    {
        isOpen = true;

        // анимация
        if (animator != null)
            animator.SetTrigger("OpenDoor");

        // Отключаем физику лазера
        if (laserCollider != null)
            laserCollider.enabled = false;

        PromptManager.Instance?.Hide();

    }

    public void UnlockDoor()
    {
        isLocked = false;
        MessageUI.Instance?.Show("СИСТЕМА", "Дверь разблокирована!", 4f);
    }
}