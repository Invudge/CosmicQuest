using UnityEngine;
using UnityEngine.UI;

public class QuestInteractable : MonoBehaviour
{
    //[Header("UI Подсказка")]
    //private bool isPlayerNearby = false;

    [Header("Тип взаимодействия 1 квест")]
    public bool isComputerPuzzle = false;
    public AddressPuzzle computerPuzzle;
    [SerializeField] private AlienMessenger roomAlien;

    //[Header("Терминал Большой Взрыв")]
    //public bool isBigBangTerminal = false;
    //public BigBangAnimation bigBangPanel;
    [Header("Анимация комнаты 2 квест")]
    public bool isAnimationSecond = false;
    public RoomRevealManager roomReveal;

    [Header("Головоломка Ферми (Квест 3)")]
    public bool isFermiPuzzle = false;
    public FermiPuzzleManager fermiPuzzle;

    [Header("Квест 4: Гравитационный манёвр")]
    public bool isBlackHolePuzzle = false;
    public BlackHoleSlingshot blackHolePuzzle;

    [Header("Квест 5: Поиск жизни")]
    public bool isPlanetPuzzle = false;
    public HabitablePlanetPuzzle planetPuzzle;

    [Header("Квест 6: Джетпка")]
    public bool isJetpackPuzzle = false;
    public JetpackPuzzleManager jetpackManager;

    [Header("Квест 7: Тёмная материя")]
    public bool isDarkMatterPuzzle = false;
    public DarkMatterPuzzleManager darkMatterPuzzle;

    [Header("Стандартный квест")]
    public bool isStandardQuest = true;

    [Header("Финал демоверсии")]
    [SerializeField] private DemoEndingController demoEnding;



    // Срабатывает, когда игрок заходит в триггер
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //isPlayerNearby = true;
            //Debug.Log($"Зашел!");
            PromptManager.Instance?.Show();
        }
    }

    // Срабатывает, когда игрок уходит
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //isPlayerNearby = false;
            //Debug.Log($"Вышел!");
            PromptManager.Instance?.Hide();
        }
    }

    // метод вызывается из PlayerController при нажатии E
    public void TriggerInteraction()
    {
        if (isDarkMatterPuzzle && darkMatterPuzzle != null)
        {
            darkMatterPuzzle.OpenPuzzle();
            return;
        }

        if (isJetpackPuzzle && jetpackManager != null)
        {
            jetpackManager.StartPuzzle();
            return;
        }

        if (isPlanetPuzzle && planetPuzzle != null)
        {
            planetPuzzle.OpenPuzzle();
            return;
        }

        if (isBlackHolePuzzle && blackHolePuzzle != null)
        {
            blackHolePuzzle.OpenPuzzle();
            return;
        }

        if (roomReveal != null && !roomReveal.isRevealed && isAnimationSecond)
        {
            MessageUI.Instance?.Show("СИСТЕМА", "Активация систем помещения...", 2f);

            roomReveal.StartReveal();
            return;
        }

        if (isComputerPuzzle && computerPuzzle != null)
        {
            computerPuzzle.parentQuest = this;
            computerPuzzle.OpenPuzzle();
            return;
        }

        if (isFermiPuzzle && fermiPuzzle != null)
        {
            fermiPuzzle.OpenPuzzle();
            return;
        }

        //if (isBigBangTerminal && bigBangPanel != null)
        //{
        //    bigBangPanel.OpenPanel();
        //    return;
        //}

        if (demoEnding != null)
        {
            demoEnding.TriggerDemoEnding();
        }

        if (isStandardQuest)
        {
            MessageUI.Instance?.Show("СИСТЕМА", "Взаимодействие выполнено.", 2f);
            // Твоя старая логика квеста здесь
        }
        //Debug.Log("Обычное взаимодействие с квестом");
    }

    public void OnPuzzleCompleted()
    {
        //puzzleCompleted = true;
        MessageUI.Instance?.Show("СИСТЕМА", "Головоломка решена! Навигация восстановлена!", 4f);
        roomAlien?.PlayAfterPuzzleDialogue();

        //Debug.Log("Головоломка решена! Навигация восстановлена!");
        // Здесь можно активировать следующий этап квеста
    }

    //void ShowPrompt()
    //{
    //    if (interactPromptCG != null)
    //    {
    //        interactPromptCG.alpha = 1; // Делаем видимым
    //        Debug.Log(interactPromptCG.alpha);
    //    }
    //}

    //// Универсальный метод скрытия
    //void HidePrompt()
    //{
    //    if (interactPromptCG != null)
    //    {
    //        interactPromptCG.alpha = 0; // Делаем невидимым
    //    }
    //}
}
