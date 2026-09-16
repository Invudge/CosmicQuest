using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class DarkMatterPuzzleManager : MonoBehaviour
{
    public static DarkMatterPuzzleManager Instance;

    [Header("UI Головоломки")]
    public TextMeshProUGUI scaleResultText;
    public TextMeshProUGUI systemMessageText;
    public GameObject hintPanel;
    public Button hintButton;
    public string stairToAccess;
    public GameObject puzzlePanel; 

    [Header("Интро и Поток")]
    public GameObject introPanel;  
    public Button btnStartIntro;   
    public GameObject pauseButton; 
    public MessageUI messageUI;    

    private bool isPuzzleActive = false;

    public QuestInteractable parentQuest;

    void Awake() => Instance = this;

    void Start()
    {
        hintButton?.onClick.AddListener(() => hintPanel.SetActive(!hintPanel.activeSelf));
        btnStartIntro?.onClick.AddListener(StartPuzzle);


        if (introPanel) introPanel.SetActive(false);
        if (puzzlePanel) puzzlePanel.SetActive(false);
    }


    public void OpenPuzzle()
    {
        if (introPanel) introPanel.SetActive(true);
        if (puzzlePanel) puzzlePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(false);

        Time.timeScale = 0f;
        Cursor.visible = true;
        isPuzzleActive = true;
    }

    void StartPuzzle()
    {
        if (introPanel) introPanel.SetActive(false);
        if (puzzlePanel) puzzlePanel.SetActive(true);

        systemMessageText.text = "Перетащите контейнер на весы для измерения массы.";
        messageUI?.Show("СИСТЕМА", "Обнаружьте тёмную материю по массе и отсутствию свечения.", 3f);
    }

    public void ClosePuzzle()
    {
        if (pauseButton != null) pauseButton.SetActive(true);
        Time.timeScale = 1f;
        if (introPanel) introPanel.SetActive(false);
        if (puzzlePanel) puzzlePanel.SetActive(false);
        isPuzzleActive = false;
    }


    public void OnContainerMeasured(bool hasMass)
    {
        if (!isPuzzleActive) return;
        scaleResultText.text = hasMass ? "Масса: ОБНАРУЖЕНА" : "Масса: ОТСУТСТВУЕТ";
    }


    public void TryLoadContainer(DarkMatterContainer container)
    {
        if (!isPuzzleActive) return;

        if (container.isDarkMatter)
        {
            MessageUI.Instance?.Show("СИСТЕМА", "Проход к капитанской рубке открыт", 2f);

            UnlockProgress();
        }
        else
        {
            systemMessageText.text = $"Ошибка: В контейнере {container.contentName}. Это не тёмная материя.";
        }
    }

    void UnlockProgress()
    {
        isPuzzleActive = false;
        parentQuest?.OnPuzzleCompleted();
        StaircaseManager.Instance?.Unlock(stairToAccess);
        StartCoroutine(DelayClosePuzzle());
    }

    IEnumerator DelayClosePuzzle()
    {
        yield return new WaitForSecondsRealtime(3f);
        ClosePuzzle();
    }

    void Update()
    {
        if (isPuzzleActive && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePuzzle();
        }
    }

    // Публичный геттер для контейнеров
    public bool IsPuzzleActive => isPuzzleActive;
}