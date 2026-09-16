using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5.0f;

    [Header("Jump")]
    public float jumpForce = 7.0f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Interaction")]
    public float interactRange = 2.0f;
    private QuestInteractable currentQuest;
    //private bool showInteractPrompt = false;

    [Header("Animation")]
    public Animator animator;
    //private bool isJumpingAnimActive = false;

    [Header("Джетпак (Квест 6)")]
    public bool isJetpackMode = false;
    public float jetpackThrust = 12f;
    public float jetpackRotSpeed = 150f;
    public float jetpackDamping = 0.98f;
    private JetpackPuzzleManager activeJetpackManager;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool jumpRequested;

    private bool canControl = true;
    public void SetControl(bool enabled) => canControl = enabled;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // блокируем управление
        if (!canControl)
        {
            moveInput.x = 0f;
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("IsJumping", false);
            }
            return;
        }

        // горизонтальное движение
        moveInput.x = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveInput.x = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveInput.x = 1f;
        
        // проверка земли
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // запрос прыжка
        if ((Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) && isGrounded)
        {
            jumpRequested = true;
            //isJumpingAnimActive = true;
        }

        //// если коснулись земли сбросили флаг прыжка
        //if (isJumpingAnimActive)
        //{
        //    if (isGrounded && rb.linearVelocity.y <= 0.1f)
        //    {
        //        isJumpingAnimActive = false;
        //    }
        //}

        // передаем инфу в аниматор
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
            animator.SetBool("IsJumping", !isGrounded);
        }

        // Разворот
        if (moveInput.x != 0)
        {
            transform.localScale = new Vector3(-Mathf.Sign(moveInput.x), 1, 1);
        }

        if (Keyboard.current.eKey.wasPressedThisFrame && currentQuest != null)
        {
            if (animator != null)
            {
                animator.SetTrigger("InteractTrigger");
            }

            currentQuest.TriggerInteraction();
        }

    }

    void FixedUpdate()
    {
        if (isJetpackMode)
        {
            HandleJetpackInput();
            return; // Пропускаем обычное движение
        }
        // движение по горизонтали
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);

        // прыжок
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequested = false;
        }
    }

    void HandleJetpackInput()
    {
        // Поворот A/D
        float rotateInput = 0f;
        if (Keyboard.current.aKey.isPressed) rotateInput = 1f;
        if (Keyboard.current.dKey.isPressed) rotateInput = -1f;
        rb.MoveRotation(rb.rotation - rotateInput * jetpackRotSpeed * Time.fixedDeltaTime);

        // Тяга W
        if (Keyboard.current.wKey.isPressed)
        {
            rb.AddForce(transform.up * jetpackThrust);
        }

        // Искусственное затухание скорости (иначе в невесомости улетит в космос)
        rb.linearVelocity *= jetpackDamping;
    }

    //private void FindInteractable()
    //{
    //    currentQuest = null;

    //    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);

    //    foreach (Collider2D col in hits)
    //    {
    //        if (col.CompareTag("Quest"))
    //        {
    //            currentQuest = col.GetComponent<QuestInteractable>();
    //            if (currentQuest != null) break; // Нашли ближайший
    //        }
    //    }

    //    showInteractPrompt = currentQuest != null;
    //}

    private void OnTriggerEnter2D(Collider2D other)
    {
        QuestInteractable quest = other.GetComponent<QuestInteractable>();
        if (quest != null)
        {
            currentQuest = quest;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        QuestInteractable quest = other.GetComponent<QuestInteractable>();
        if (quest == currentQuest)
        {
            currentQuest = null;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Вызывается менеджером при старте головоломки
    public void EnableJetpackMode(float thrust, float rot, float damp, JetpackPuzzleManager mgr)
    {
        isJetpackMode = true;
        activeJetpackManager = mgr;
        jetpackThrust = thrust;
        jetpackRotSpeed = rot;
        jetpackDamping = damp;
        rb.gravityScale = 0f;
    }

    // Вызывается менеджером при победе
    public void DisableJetpackMode()
    {
        isJetpackMode = false;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    // Проверяем, коснулись ли мы земли (можно по тегу или слою)
    //    foreach (ContactPoint2D contact in collision.contacts)
    //    {
    //        // Если нормаль направлена вверх (мы стоим на поверхности)
    //        if (contact.normal.y > 0.5f)
    //        {
    //            isJumpingAnimActive = false; // Выключаем анимацию прыжка
    //            break;
    //        }
    //    }
    //}

    //public void OnGUI()
    //{
    //    if (showInteractPrompt)
    //    {
    //        GUIStyle style = new GUIStyle();
    //        style.fontSize = 20;
    //        style.normal.textColor = Color.yellow;
    //        style.alignment = TextAnchor.MiddleCenter;

    //        // Рисуем по центру экрана
    //        Rect rect = new Rect(0, Screen.height * 0.7f, Screen.width, 30);
    //        GUI.Label(rect, "Нажмите [E] для взаимодействия", style);
    //    }
    //}

    // видим радиус проверки земли в редакторе

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Срабатывает ТОЛЬКО в режиме джетпака и ТОЛЬКО при касании тега "Wall"
        if (isJetpackMode && activeJetpackManager != null && collision.gameObject.CompareTag("Wall"))
        {
            activeJetpackManager.ResetPuzzle();
        }
    }
    private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }

}