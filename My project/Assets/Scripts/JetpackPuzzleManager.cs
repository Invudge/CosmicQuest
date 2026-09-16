using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class JetpackPuzzleManager : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform startPoint;
    public Transform finishPoint;
    public GameObject puzzleWall;      
    public GameObject nextDoor;
    public string toAccessStair;
    public MessageUI messageUI;
    public GameObject pauseButton;
    public GameObject puzzleFloor;

    //[Header("Лазеры")]
    //public LaserTrap[] lasers;
    //public float laserOffTime = 2f;
    //public float laserOnTime = 3f;

    [Header("Физика джетпака")]
    public float thrustForce = 12f;
    public float rotationSpeed = 150f;
    public float velocityDamping = 0.98f; // Затухание инерции (0.95-0.99)

    private Coroutine laserRoutine;
    private bool isPuzzleActive = false;

    public QuestInteractable parentQuest;

    void Start()
    {
        //foreach (var laser in lasers) laser.Setup(this);
        //ResetPuzzleState();
    }

    // Вызывается из QuestInteractable
    public void StartPuzzle()
    {
        isPuzzleActive = true;
        if (pauseButton != null) pauseButton.SetActive(false);

        messageUI?.Show("ДЖЕТПАК", "Гравитация отключена.\nA/D - поворот, W - тяга.\nИзбегай лазеров!\nДоберись до финиша.", 5f);

        var player = FindAnyObjectByType<PlayerController>();
        if (player != null) player.EnableJetpackMode(thrustForce, rotationSpeed, velocityDamping, this);

        ResetPlayerPosition();
        //StartLaserCycle();
    }

    public void ResetPuzzle()
    {
        messageUI?.Show("СБОЙ", "Лазерное поражение. Попытка заново.", 2f);
        ResetPlayerPosition();
    }

    public void WinPuzzle()
    {
        isPuzzleActive = false;
        //StopLaserCycle();
        StaircaseManager.Instance?.Unlock(toAccessStair);
        messageUI?.Show("УСПЕХ", "Маршрут пройден. Возврат в шлюз.", 3f);
        parentQuest?.OnPuzzleCompleted();

        // Телепорт в начало и сброс физики
        ResetPlayerPosition();

        // Возврат обычного управления
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null) player.DisableJetpackMode();

        // Открытие пути
        if (puzzleFloor != null) puzzleFloor.SetActive(true);
        if (puzzleWall != null) puzzleWall.SetActive(false);
        if (nextDoor != null)
        {
            var door = nextDoor.GetComponent<LaserDoor>();
            if (door != null) door.UnlockDoor();
            else nextDoor.SetActive(false);
        }

        if (pauseButton != null) pauseButton.SetActive(true);
    }

    void ResetPlayerPosition()
    {
        var player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;
        player.transform.position = startPoint.position;
        player.transform.rotation = Quaternion.identity;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    //void StartLaserCycle()
    //{
    //    if (laserRoutine != null) StopCoroutine(laserRoutine);
    //    laserRoutine = StartCoroutine(LaserCycleCoroutine());
    //}

    //IEnumerator LaserCycleCoroutine()
    //{
    //    while (isPuzzleActive)
    //    {
    //        foreach (var l in lasers) l.SetState(false);
    //        yield return new WaitForSeconds(laserOffTime);

    //        foreach (var l in lasers) l.SetState(true);
    //        yield return new WaitForSeconds(laserOnTime);
    //    }
    //}

    //void StopLaserCycle()
    //{
    //    if (laserRoutine != null) { StopCoroutine(laserRoutine); laserRoutine = null; }
    //    foreach (var l in lasers) l.SetState(false);
    //}

    //void ResetPuzzleState()
    //{
    //    if (puzzleWall != null) puzzleWall.SetActive(true);
    //    foreach (var l in lasers) l.SetState(false);
    //}
}