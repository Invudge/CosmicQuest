using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("Настройки")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float defaultFadeTime = 1f;

    void Awake() => Instance = this;

    public IEnumerator FadeOut(float duration = -1)
    {
        float time = duration > 0 ? duration : defaultFadeTime;
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = true;

        float elapsed = 0;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / time);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1;
    }

    public IEnumerator FadeIn(float duration = -1)
    {
        float time = duration > 0 ? duration : defaultFadeTime;
        fadeCanvasGroup.alpha = 1;

        float elapsed = 0;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / time);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.interactable = true;
        fadeCanvasGroup.blocksRaycasts = false;
    }
}