using UnityEngine;
using UnityEngine.UI;

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance;
    public CanvasGroup promptCG;

    private int activeRequests = 0;

    void Awake() => Instance = this;

    public void Show()
    {
        activeRequests++;
        if (promptCG != null) promptCG.alpha = 1f;
    }

    public void Hide()
    {
        if (activeRequests <= 0) return;

        activeRequests--;
        if (activeRequests == 0 && promptCG != null)
        {
            promptCG.alpha = 0f;
        }
    }
}