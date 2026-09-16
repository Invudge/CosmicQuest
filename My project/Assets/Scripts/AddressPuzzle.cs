using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddressPuzzle : MonoBehaviour
{
    [Header("Панели")]
    public GameObject computerPanel;
    public GameObject selectionPopupPrefab;
    private GameObject currentPopupInstance;
    
    [Header("Данные для 9 строк")]
    public RowData[] rowOptions;

    [Header("UI элементы")]
    public AddressRowUI[] addressRows;
    public TextMeshProUGUI hintText;
    public Button btnClose, btnCheck, btnToggleHint;

    [Header("Связь с UI игры")]
    public GameObject pauseButton;
    
    [Header("Разблокировка")]
    public LaserDoor targetDoor;

    [Header("Настройки")]
    public Color correctColor = new Color(0.4f, 0.85f, 0.4f); 
    public Color wrongColor = new Color(0.85f, 0.3f, 0.3f);   
    public Color defaultColor = Color.white;

    [Header("Интро")]
    public GameObject introPanel;
    public GameObject puzzleContent;
    public Button btnStartIntro;

    private bool isHintVisible = false;
    private int selectedRowIndex = -1;
    public QuestInteractable parentQuest;
    private bool isChecking = false;
    

    void Start()
    {
        // безопасная привязка основных кнопок
        if (btnClose != null) btnClose.onClick.AddListener(ClosePuzzle);
        if (btnCheck != null) btnCheck.onClick.AddListener(CheckAddress);
        if (btnToggleHint != null) btnToggleHint.onClick.AddListener(ToggleHint);
        if (hintText != null) hintText.gameObject.SetActive(false);
        if (btnStartIntro != null) btnStartIntro.onClick.AddListener(StartPuzzle);
        if (introPanel != null) introPanel.SetActive(false);
        if (puzzleContent != null) puzzleContent.SetActive(false);


        if (addressRows != null)
        {
            for (int i = 0; i < addressRows.Length; i++)
            {
                int index = i;
                if (addressRows[i]?.btn != null)
                    addressRows[i].btn.onClick.AddListener(() => OpenSelectionPopup(index));
            }
        }

        // создание попапа
        if (selectionPopupPrefab != null)
        {
            currentPopupInstance = Instantiate(selectionPopupPrefab, transform);
            currentPopupInstance.SetActive(false);

            Button[] optBtns = currentPopupInstance.GetComponentsInChildren<Button>(true);
            if (optBtns.Length >= 3)
            {
                optBtns[0].onClick.AddListener(() => SelectOption(0));
                optBtns[1].onClick.AddListener(() => SelectOption(1));
                optBtns[2].onClick.AddListener(() => SelectOption(2));
            }
        }
    }

    public void OpenPuzzle()
    {
        computerPanel?.SetActive(true);

        if (pauseButton != null) pauseButton.SetActive(false);
        if (introPanel != null) introPanel.SetActive(true);
        if (puzzleContent != null) puzzleContent.SetActive(false);

        Time.timeScale = 0f;
        Cursor.visible = true;
    }

    void ClosePuzzle()
    {
        HideSelectionPopup();
        computerPanel?.SetActive(false);

        if (pauseButton != null) pauseButton.SetActive(true);
        if (introPanel != null) introPanel.SetActive(true);
        if (puzzleContent != null) puzzleContent.SetActive(false);

        Time.timeScale = 1f;
        //Cursor.visible = false;
    }

    public void StartPuzzle()
    {
        if (introPanel != null) introPanel.SetActive(false);
        if (puzzleContent != null) puzzleContent.SetActive(true);
    }

    void ToggleHint()
    {
        isHintVisible = !isHintVisible;
        hintText?.gameObject.SetActive(isHintVisible);
    }

    void OpenSelectionPopup(int rowIndex)
    {
        if (isChecking) return;

        selectedRowIndex = rowIndex;
        if (rowIndex >= rowOptions.Length || currentPopupInstance == null) return;

        List<string> optionsList = rowOptions[rowIndex].GetOptions().ToList();

        //  алгоритм Фишера-Йетса (честное перемешивание за O(n))
        for (int i = optionsList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = optionsList[i];
            optionsList[i] = optionsList[j];
            optionsList[j] = temp;
        }

        string[] options = optionsList.ToArray();

        Button[] optBtns = currentPopupInstance.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < 3 && i < options.Length; i++)
        {
            var txt = optBtns[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = options[i];
            optBtns[i].gameObject.SetActive(true);
        }

        currentPopupInstance.SetActive(true);
    }

    public void SelectOption(int buttonIndex)
    {
        if (selectedRowIndex == -1 || currentPopupInstance == null) return;

        Button[] optBtns = currentPopupInstance.GetComponentsInChildren<Button>(true);
        var txt = optBtns[buttonIndex].GetComponentInChildren<TextMeshProUGUI>();
        string chosenText = txt != null ? txt.text : "";

        if (selectedRowIndex < addressRows.Length && addressRows[selectedRowIndex].displayText != null)
            addressRows[selectedRowIndex].displayText.text = chosenText;

        HideSelectionPopup();
    }

    void HideSelectionPopup()
    {
        if (currentPopupInstance != null)
            currentPopupInstance.SetActive(false);
        selectedRowIndex = -1;
    }

    public void CheckAddress()
    {
        if (isChecking) return;
        StartCoroutine(ValidateAndReset());
    }

    IEnumerator ValidateAndReset()
    {
        isChecking = true;

        // 1. Блокируем все кнопки, чтобы игрок не нажимал во время показа результата
        foreach (var row in addressRows)
            if (row?.btn != null) row.btn.interactable = false;

        // 2. Сравниваем каждую строку
        bool[] results = new bool[addressRows.Length];
        bool allCorrect = true;

        for (int i = 0; i < addressRows.Length; i++)
        {
            string playerText = addressRows[i]?.displayText?.text?.Trim() ?? "";
            string correctText = rowOptions[i].correct.Trim();
            bool isMatch = playerText.Equals(correctText, System.StringComparison.OrdinalIgnoreCase);

            results[i] = isMatch;
            if (!isMatch) allCorrect = false;
        }

        for (int i = 0; i < addressRows.Length; i++)
        {
            Color feedbackColor = results[i] ? correctColor : wrongColor;
            ApplyRowColor(addressRows[i], feedbackColor);
        }

        if(allCorrect)
        {
            MessageUI.Instance?.Show("СИСТЕМА", "НАВИГАЦИЯ ВОССТАНОВЛЕНА!", 3f);
        }
        else
        {
            MessageUI.Instance?.Show("СИСТЕМА", "ОШИБКА АДРЕСА. Сбрасываю ввод...", 3f);
        }


        yield return new WaitForSecondsRealtime(3f);

        if (allCorrect)
        {
            //MessageUI.Instance?.Show("СИСТЕМА", "НАВИГАЦИЯ ВОССТАНОВЛЕНА!", 3f);

            if (targetDoor != null) targetDoor.UnlockDoor();

            parentQuest?.OnPuzzleCompleted();
            ClosePuzzle();
        }
        else
        {
            //MessageUI.Instance?.Show("СИСТЕМА", "ОШИБКА АДРЕСА. Сбрасываю ввод...", 3f);

            foreach (var row in addressRows)
            {
                ApplyRowColor(row, defaultColor);
                if (row?.displayText != null) row.displayText.text = "";
                if (row?.btn != null) row.btn.interactable = true;
            }
        }

        isChecking = false;
    }

    void ApplyRowColor(AddressRowUI row, Color color)
    {
        if (row == null) return;


        if (row.btn != null && row.btn.image != null)
            row.btn.image.color = color;

        else if (row.displayText != null)
            row.displayText.color = color;
    }
}


[System.Serializable]
public class RowData
{
    public string correct;
    public string wrong1;
    public string wrong2;
    public string[] GetOptions() => new[] { correct, wrong1, wrong2 };
}

[System.Serializable]
public class AddressRowUI
{
    public Button btn;
    public TextMeshProUGUI displayText;
}