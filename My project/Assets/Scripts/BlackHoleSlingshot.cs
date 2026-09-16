using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BlackHoleSlingshot : MonoBehaviour
{
    [Header("UI Ссылки")]
    public RectTransform panelArea;
    public RectTransform projectile;
    public RectTransform startPoint;
    public RectTransform blackHole;
    public RectTransform targetZone;
    public RectTransform eventHorizon;
    public GameObject predictionDotPrefab;
    public MessageUI messageUI;
    public GameObject pauseButton;

    public LaserDoor nextDoor;
    public string toAccessStair;

    [Header("Панели управления")]
    public GameObject introPanel;         
    public GameObject puzzlePanel;        
    public Button btnStartIntro;          

    [Header("Настройки Геймплея")]
    public float maxDragDistance = 150f;
    public float launchMultiplier = 12f;
    public float gravityStrength = 6000f;
    public float targetRadius = 60f;
    public float horizonRadius = 70f;
    public int predictionSteps = 25;

    private Vector2 dragStartPos;
    private bool isDragging = false;
    private bool isLaunched = false;
    private bool isResolved = false;
    private Vector2 velocity;
    private List<GameObject> predictionDots = new List<GameObject>();
    public QuestInteractable parentQuest;

    void Start()
    {
        btnStartIntro?.onClick.AddListener(StartPuzzleGame);
        ResetPuzzle();
        SetInitialUIState();
    }

    void SetInitialUIState()
    {
        if (introPanel != null) introPanel.SetActive(false);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
    }

    public void OpenPuzzle()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        if (pauseButton != null) pauseButton.SetActive(false);

        introPanel?.SetActive(true);
        puzzlePanel?.SetActive(false);
        ResetPuzzle();
    }

    void StartPuzzleGame()
    {
        introPanel?.SetActive(false);
        puzzlePanel?.SetActive(true);
    }

    public void ClosePuzzle()
    {
        if (pauseButton != null) pauseButton.SetActive(true);
        Time.timeScale = 1f;
        //Cursor.visible = false;
        introPanel?.SetActive(false);
        puzzlePanel?.SetActive(false);
        ResetPuzzle();
    }

    void Update()
    {
        if (isResolved) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePuzzle();
            return;
        }

        if (puzzlePanel == null || !puzzlePanel.activeSelf) return;
        if (Mouse.current == null) return;

        Canvas canvas = panelArea.GetComponentInParent<Canvas>();
        Camera uiCamera = (canvas.renderMode == RenderMode.ScreenSpaceCamera) ? canvas.worldCamera : null;

        Vector2 mouseLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelArea, Mouse.current.position.ReadValue(), uiCamera, out mouseLocal);

        //Debug.Log($" Клик: {mouseLocal} | Корабль: {projectile.anchoredPosition} | : {Vector2.Distance(mouseLocal, projectile.anchoredPosition):F1}");
        Vector2 panelSize = panelArea.rect.size;
        Vector2 fixedMousePos = mouseLocal + new Vector2(panelSize.x * panelArea.pivot.x, panelSize.y * panelArea.pivot.y);

        if (!isLaunched) HandleInput(fixedMousePos);
        else SimulatePhysics(Time.unscaledDeltaTime);
    }

    void HandleInput(Vector2 mousePos)
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            float dist = Vector2.Distance(mousePos, projectile.anchoredPosition);
            if (dist < 100f)
            {
                isDragging = true;
                dragStartPos = startPoint.anchoredPosition;
            }
        }

        if (isDragging)
        {
            Vector2 dragVector = mousePos - dragStartPos;
            if (dragVector.magnitude > maxDragDistance)
                dragVector = dragVector.normalized * maxDragDistance;

            projectile.anchoredPosition = dragStartPos + dragVector;
            UpdatePrediction(-dragVector);

            if (Mouse.current.leftButton.wasReleasedThisFrame) Launch();
        }
    }

    void Launch()
    {
        isDragging = false;
        isLaunched = true;
        ClearPredictionDots();

        Vector2 dragVector = projectile.anchoredPosition - dragStartPos;

        velocity = -dragVector.normalized * (dragVector.magnitude / maxDragDistance) * launchMultiplier;
    }

    void UpdatePrediction(Vector2 launchDir)
    {
        ClearPredictionDots();

        Vector2 pos = projectile.anchoredPosition;
        Vector2 vel = launchDir.normalized * (launchDir.magnitude / maxDragDistance) * launchMultiplier;

        float stepDt = 0.02f;
        for (int i = 0; i < predictionSteps; i++)
        {
            Vector2 toHole = blackHole.anchoredPosition - pos;
            float dist = toHole.magnitude;
            float force = gravityStrength / (dist * dist + 1500f);
            vel += toHole.normalized * force * stepDt;
            pos += vel * stepDt;

            GameObject dot = Instantiate(predictionDotPrefab, panelArea);
            RectTransform dotRT = dot.GetComponent<RectTransform>();

            dotRT.anchorMin = new Vector2(0f, 0f);
            dotRT.anchorMax = new Vector2(0f, 0f);
            dotRT.pivot = new Vector2(0.5f, 0.5f);
            dotRT.anchoredPosition = pos;
            dotRT.localScale = Vector3.one * 0.5f;

            predictionDots.Add(dot);
        }
    }

    void SimulatePhysics(float dt)
    {
        Vector2 toHole = blackHole.anchoredPosition - projectile.anchoredPosition;
        float dist = toHole.magnitude;

        float force = gravityStrength / (dist * dist + 1500f);
        velocity += toHole.normalized * force * dt;
        projectile.anchoredPosition += velocity * dt;

        float distToTarget = Vector2.Distance(projectile.anchoredPosition, targetZone.anchoredPosition);
        float distToHorizon = Vector2.Distance(projectile.anchoredPosition, eventHorizon.anchoredPosition);

        if (distToTarget < targetRadius) Win();
        else if (distToHorizon < horizonRadius) Lose("Корабль разорван приливными силами чёрной дыры.");
        else if (dist > 1200f) Lose("Траектория ушла в открытый космос.");
    }

    void Win()
    {
        isResolved = true;
        //if (nextDoor != null) nextDoor.UnlockDoor();
        StaircaseManager.Instance?.Unlock(toAccessStair);
        messageUI?.Show("НАВИГАТОР", "Манёвр успешен! Гравитация использована для ускорения.", 3f);
        parentQuest?.OnPuzzleCompleted();
        Invoke(nameof(ClosePuzzle), 2.5f);
    }

    void Lose(string reason)
    {
        isResolved = true;
        messageUI?.Show("АВАРИЯ", reason, 3f);
        Invoke(nameof(ResetPuzzle), 1.5f);
    }

    public void ResetPuzzle()
    {
        isResolved = false;
        isLaunched = false;
        isDragging = false;
        velocity = Vector2.zero;
        projectile.anchoredPosition = startPoint.anchoredPosition;
        ClearPredictionDots();
    }

    void ClearPredictionDots()
    {
        foreach (var dot in predictionDots) Destroy(dot);
        predictionDots.Clear();
    }
}