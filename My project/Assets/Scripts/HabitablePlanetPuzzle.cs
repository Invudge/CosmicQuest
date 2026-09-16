using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class HabitablePlanetPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class PlanetData
    {
        public string name;
        public string description;
        public float temperature;
        public bool hasWater;
        public bool hasAtmosphere;
    }

    [System.Serializable]
    public class PlanetUI
    {
        public Button button;
        public Image feedbackImage;
        public TMP_Text nameText;
        public TMP_Text descText;
    }

    [System.Serializable]
    public class Condition
    {
        public string title;
        public string description;
        public float minTemp, maxTemp;
        public bool needsWater, needsAtmosphere;
    }

    [Header("UI Панели")]
    public GameObject puzzlePanel;
    public GameObject introPanel;        
    public Button btnStartIntro;         

    [Header("Интерфейс")]
    public TMP_Text conditionTitle;
    public TMP_Text conditionDesc;
    public TMP_Text livesText;
    public Button btnClose;
    public Button btnReset;

    [Header("Связи")]
    public GameObject pauseButton;       
    public LaserDoor nextDoor;           

    [Header("Данные")]
    public PlanetData[] planets;      
    public PlanetUI[] planetCards;    
    public Condition[] conditions;    

    [Header("Настройки")]
    public int maxLives = 3;
    public float feedbackDuration = 1.5f;

    private int currentConditionIndex = 0;
    private int lives;
    private bool isPuzzleActive = false;
    public QuestInteractable parentQuest;

    void Start()
    {
        // Привязка кнопок планет
        for (int i = 0; i < planetCards.Length; i++)
        {
            int idx = i;
            planetCards[i].button.onClick.AddListener(() => OnPlanetClicked(idx));
            planetCards[i].nameText.text = planets[i].name;
            planetCards[i].descText.text = planets[i].description;
        }

        btnClose?.onClick.AddListener(ClosePuzzle);
        btnReset?.onClick.AddListener(ResetPuzzle);
        btnStartIntro?.onClick.AddListener(StartAnalysis);
    }

    // Вызывается из QuestInteractable
    public void OpenPuzzle()
    {
        introPanel?.SetActive(true);   
        puzzlePanel?.SetActive(false); 
        if (pauseButton != null) pauseButton.SetActive(false);
        Time.timeScale = 0f;
        Cursor.visible = true;
        isPuzzleActive = true;
    }


    void StartAnalysis()
    {
        introPanel?.SetActive(false);
        puzzlePanel?.SetActive(true);
        ResetPuzzle(); 
    }

    public void ClosePuzzle()
    {
        if (pauseButton != null) pauseButton.SetActive(true);
        Time.timeScale = 1f;
        //Cursor.visible = false;
        introPanel?.SetActive(false);
        puzzlePanel?.SetActive(false);
        isPuzzleActive = false;
    }

    public void ResetPuzzle()
    {
        currentConditionIndex = 0;
        lives = maxLives;
        isPuzzleActive = true;
        UpdateLivesUI();
        ClearFeedbackColors();
        LoadCondition(0);
    }

    void LoadCondition(int index)
    {
        if (index >= conditions.Length)
        {
            CompletePuzzle();
            return;
        }

        Condition c = conditions[index];

        // Безопасное обновление текста
        if (conditionTitle != null) conditionTitle.text = c.title;

        if (conditionDesc != null) conditionDesc.text = c.description;

        if (conditionTitle != null)
            MessageUI.Instance?.Show("АНАЛИЗ", $"Этап {index + 1}/{conditions.Length}", 2f);
    }
    void OnPlanetClicked(int planetIndex)
    {
        if (!isPuzzleActive) return;

        Condition c = conditions[currentConditionIndex];
        PlanetData p = planets[planetIndex];

        bool isTempOk = p.temperature >= c.minTemp && p.temperature <= c.maxTemp;
        bool isWaterOk = p.hasWater == c.needsWater;
        bool isAtmoOk = p.hasAtmosphere == c.needsAtmosphere;
        bool isCorrect = isTempOk && isWaterOk && isAtmoOk;

        if (isCorrect) HandleSuccess(planetIndex);
        else HandleFailure(planetIndex);
    }

    void HandleSuccess(int idx)
    {
        Debug.Log($"Планета {idx} подошла! Ждем перехода...");
        StartCoroutine(FlashColor(idx, Color.green));
        MessageUI.Instance?.Show("УСПЕХ", "Параметры совпали.", 2f);

        isPuzzleActive = false;

        StartCoroutine(DelayNextCondition());
    }

    void HandleFailure(int idx)
    {
        StartCoroutine(FlashColor(idx, Color.red));
        lives--;
        UpdateLivesUI();
        MessageUI.Instance?.Show("ОШИБКА", "Планета не подходит. Потеряна жизнь.", 2f);

        if (lives <= 0)
        {
            isPuzzleActive = false;
            MessageUI.Instance?.Show("ПРОВАЛ", "Анализ провален. Запуск заново...", 3f);
            StartCoroutine(DelayResetPuzzle());
        }
    }

    void NextCondition()
    {
        Debug.Log("Переход к следующему этапу");
        currentConditionIndex++;
        ClearFeedbackColors();
        isPuzzleActive = true;
        LoadCondition(currentConditionIndex);
    }

    void CompletePuzzle()
    {
        MessageUI.Instance?.Show("ГОТОВО", "Все условия выполнены. Доступ к следующему отсеку разрешен.", 3f);
        parentQuest?.OnPuzzleCompleted();

        if (nextDoor != null) nextDoor.UnlockDoor();

        StartCoroutine(DelayClosePuzzle());
    }

    void UpdateLivesUI()
    {
        if (livesText != null) livesText.text = $"Жизни: {lives}/{maxLives}";
    }

    void ClearFeedbackColors()
    {
        foreach (var card in planetCards)
        {
            if (card.feedbackImage != null) card.feedbackImage.color = Color.white;
        }
    }

    IEnumerator FlashColor(int idx, Color target)
    {
        if (planetCards[idx].feedbackImage == null) yield break;
        Image img = planetCards[idx].feedbackImage;
        Color original = img.color;
        img.color = target;
        yield return new WaitForSecondsRealtime(feedbackDuration);
        img.color = original;
    }

    void Update()
    {
        if (isPuzzleActive && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePuzzle();
        }
    }

    private IEnumerator DelayNextCondition()
    {
        // игнор timeScale
        yield return new WaitForSecondsRealtime(feedbackDuration);
        NextCondition();
    }

    private IEnumerator DelayClosePuzzle()
    {
        yield return new WaitForSecondsRealtime(3.5f);
        ClosePuzzle();
    }

    private IEnumerator DelayResetPuzzle()
    {
        yield return new WaitForSecondsRealtime(3.5f);
        ResetPuzzle();
    }
}