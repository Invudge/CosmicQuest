using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class FermiPuzzleManager : MonoBehaviour
{
    [System.Serializable] public class RaceCard { public string id; public Button connectPoint; public Image pointVisual; public bool isConnected; }
    [System.Serializable] public class HypothesisCard { public string id; public Button connectPoint; public Image pointVisual; public bool isConnected; }
    [System.Serializable] public class CorrectMatch { public string raceId; public string hypothesisId; }

    private class Connection
    {
        public RaceCard race;
        public HypothesisCard hypothesis;
        public GameObject line;
        public Connection(RaceCard r, HypothesisCard h, GameObject l) { race = r; hypothesis = h; line = l; }
    }

    [Header("UI Панели")]
    public GameObject puzzlePanel;   
    public GameObject introPanel;     
    public Button btnStartPuzzle;     
    public GameObject pauseButton;

    [Header("Игровые элементы")]
    public Transform linesContainer; 
    public Image linePrefab;          
    public RaceCard[] races;
    public HypothesisCard[] hypotheses;
    public CorrectMatch[] correctMatches;
    public Button btnCheck;
    public Button btnReset;
    public Button btnClose;
    public MessageUI messageUI;

    public LaserDoor nextDoor;

    private List<Connection> activeConnections = new List<Connection>();
    private RaceCard selectedRace;
    public QuestInteractable parentQuest;

    void Start()
    {
        SetupClicks();
        if (btnClose != null) btnClose.onClick.AddListener(ClosePuzzle);
        btnCheck?.onClick.AddListener(CheckMatches);
        btnReset?.onClick.AddListener(ClearAll);
        btnStartPuzzle?.onClick.AddListener(BeginPuzzle);


        if (introPanel != null) introPanel.SetActive(false);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
    }

    void Update()
    {
        if (puzzlePanel != null && puzzlePanel.activeSelf && Keyboard.current.escapeKey.wasPressedThisFrame)
            ClosePuzzle();
    }

    public void OpenPuzzle()
    {

        if (pauseButton != null) pauseButton.SetActive(false);
        if (introPanel != null) introPanel.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        Time.timeScale = 0f;
        Cursor.visible = true;
    }

    void BeginPuzzle()
    {

        if (introPanel != null) introPanel.SetActive(false);
        if (puzzlePanel != null) puzzlePanel.SetActive(true);
    }

    public void ClosePuzzle()
    {
        ClearAll();
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        if (introPanel != null) introPanel.SetActive(false);
        Time.timeScale = 1f;
        //Cursor.visible = false;
    }

    void SetupClicks()
    {
        foreach (var r in races)
        {
            var race = r;
            r.connectPoint.onClick.AddListener(() => OnRaceClick(race));
        }
        foreach (var h in hypotheses)
        {
            var hyp = h;
            h.connectPoint.onClick.AddListener(() => OnHypothesisClick(hyp));
        }
    }

    void OnRaceClick(RaceCard race)
    {
        if (race.isConnected) { RemoveConnection(race); return; }
        ClearSelection();
        selectedRace = race;
        if (race.pointVisual != null) race.pointVisual.color = Color.yellow;
    }

    void OnHypothesisClick(HypothesisCard hyp)
    {
        if (hyp.isConnected || selectedRace == null) return;

        GameObject lineObj = DrawLine(selectedRace.connectPoint.transform, hyp.connectPoint.transform);
        activeConnections.Add(new Connection(selectedRace, hyp, lineObj));

        selectedRace.isConnected = true;
        hyp.isConnected = true;

        if (selectedRace.pointVisual != null) selectedRace.pointVisual.color = Color.green;
        if (hyp.pointVisual != null) hyp.pointVisual.color = Color.green;
        selectedRace = null;
    }

    GameObject DrawLine(Transform top, Transform bottom)
    {

        Vector3 localTop = linesContainer.InverseTransformPoint(top.position);
        Vector3 localBottom = linesContainer.InverseTransformPoint(bottom.position);

        Vector3 localDir = localBottom - localTop;
        float distance = localDir.magnitude;
        float angle = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
        Vector3 localMid = (localTop + localBottom) * 0.5f;


        GameObject lineObj = Instantiate(linePrefab.gameObject, linesContainer);
        RectTransform lineRT = lineObj.GetComponent<RectTransform>();
        lineRT.localPosition = localMid;
        lineRT.localEulerAngles = new Vector3(0, 0, angle);
        lineRT.sizeDelta = new Vector2(distance, 4f);
        lineRT.localScale = Vector3.one;
        return lineObj;
    }

    void RemoveConnection(RaceCard race)
    {
        var conn = activeConnections.Find(c => c.race == race);
        if (conn != null)
        {
            if (conn.line != null) Destroy(conn.line);
            if (conn.race.pointVisual != null) conn.race.pointVisual.color = Color.white;
            if (conn.hypothesis.pointVisual != null) conn.hypothesis.pointVisual.color = Color.white;
            conn.race.isConnected = false;
            conn.hypothesis.isConnected = false;
            activeConnections.Remove(conn);
        }
        ClearSelection();
    }

    void CheckMatches()
    {
        if (activeConnections.Count < races.Length)
        {
            messageUI?.Show("ТЕРМИНАЛ", "Соедини все три расы с гипотезами перед проверкой.", 3f);
            return;
        }

        bool allCorrect = true;
        foreach (var conn in activeConnections)
        {
            var match = System.Array.Find(correctMatches, m => m.raceId == conn.race.id);
            if (match == null || match.hypothesisId != conn.hypothesis.id)
            {
                allCorrect = false;
                if (conn.race.pointVisual != null) conn.race.pointVisual.color = Color.red;
                if (conn.hypothesis.pointVisual != null) conn.hypothesis.pointVisual.color = Color.red;
            }
        }

        if (allCorrect)
        {
            messageUI?.Show("ТЕРМИНАЛ", "Анализ завершён. Парадокс Ферми имеет логическое объяснение. Доступ разблокирован.", 4f);
            OnPuzzleSolved();
        }
        else
        {
            messageUI?.Show("ТЕРМИНАЛ", "Ошибка сопоставления. Попробуй ещё раз.", 2.5f);
            Invoke(nameof(ClearAll), 1.5f);
        }
    }

    void ClearAll()
    {
        foreach (var c in activeConnections)
        {
            if (c.line != null) Destroy(c.line);
            if (c.race.pointVisual != null) c.race.pointVisual.color = Color.white;
            if (c.hypothesis.pointVisual != null) c.hypothesis.pointVisual.color = Color.white;
            c.race.isConnected = false;
            c.hypothesis.isConnected = false;
        }
        activeConnections.Clear();
        ClearSelection();
    }

    void ClearSelection()
    {
        if (selectedRace != null)
        {
            if (selectedRace.pointVisual != null) selectedRace.pointVisual.color = Color.white;
            selectedRace = null;
        }
    }

    void OnPuzzleSolved()
    {
        if (nextDoor != null) nextDoor.UnlockDoor();

        MessageUI.Instance?.Show("ТЕРМИНАЛ", "Логическая модель подтверждена. Маршрут к следующей зоне проложен.", 3.5f);
        parentQuest?.OnPuzzleCompleted();

        // Автозакрытие через 4 секунды
        Invoke(nameof(ClosePuzzle), 4f);
    }
}