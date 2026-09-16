using UnityEngine;
public class FinishTrigger : MonoBehaviour
{
    public JetpackPuzzleManager manager;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && manager != null) manager.WinPuzzle();
    }
}