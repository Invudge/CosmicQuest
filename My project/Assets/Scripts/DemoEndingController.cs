using UnityEngine;
using System.Collections;
using TMPro;

public class DemoEndingController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private FadeManager fadeOverlay;      // Компонент затемнения
    [SerializeField] private GameObject endPanel;          // Панель с финальным текстом
    [SerializeField] private TextMeshProUGUI endText;      // Поле текста

    [Header("Настройки")]
    [SerializeField] private string finalMessage = "Благодарим за прохождение демоверсии!";
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float messageDuration = 5f;

    private bool isPlayed = false;

    // Вызывается при завершении 8-й головоломки
    public void TriggerDemoEnding()
    {
        if (isPlayed) return;
        isPlayed = true;
        StartCoroutine(PlayEndingSequence());
    }

    IEnumerator PlayEndingSequence()
    {
        // 1. Блокируем ввод игрока
        playerController?.SetControl(false);

        // 2. Плавное затемнение экрана
        if (fadeOverlay != null)
        {
            // Адаптируйте под точное название метода вашего FadeOverlay:
            fadeOverlay.FadeIn(fadeDuration);
            // или: fadeOverlay.SetAlpha(1f, fadeDuration);
            yield return new WaitForSecondsRealtime(fadeDuration);
        }

        // 3. Вывод финального сообщения
        if (endPanel != null) endPanel.SetActive(true);
        if (endText != null) endText.text = finalMessage;

        yield return new WaitForSecondsRealtime(messageDuration);

        // 4. (Опционально) Переход в меню или завершение билда
        // SceneManager.LoadScene("MainMenu");
        // #if UNITY_EDITOR UnityEditor.EditorApplication.isPlaying = false; #else Application.Quit(); #endif
    }

#if UNITY_EDITOR
    [ContextMenu("Сбросить состояние концовки")]
    void ResetState() { isPlayed = false; StopAllCoroutines(); }
#endif
}