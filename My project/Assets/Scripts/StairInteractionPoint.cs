using UnityEngine;

public class StairInteractionPoint : MonoBehaviour
{
    [Header("Настройки")]
    public string stairId; 
    public float detectionRadius = 2f;
    public float messageCooldown = 3f;

    private Transform player;
    private float lastMessageTime = -10f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (FindAnyObjectByType<StaircaseManager>() == null)
        {
            GameObject manager = new GameObject("StaircaseManager");
            manager.AddComponent<StaircaseManager>();
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist < detectionRadius)
        {
            if (!StaircaseManager.Instance.IsUnlocked(stairId))
            {
                if (Time.time - lastMessageTime > messageCooldown)
                {
                    MessageUI.Instance?.Show("СИСТЕМА", "Мне пока туда не нужно.", 2.5f);
                    lastMessageTime = Time.time;
                }
            }
            else
            {
                //;
            }
        }
    }
}