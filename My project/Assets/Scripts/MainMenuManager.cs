using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Сцены")]
    public string gameSceneName = "GameScene";

    [Header("Панели меню")]
    public GameObject mainMenuContainer;
    public GameObject settingsPanel;

    [Header("Анимация меню")]
    public CanvasGroup menuCanvasGroup;
    public float fadeInDuration = 0.8f;

    void Start()
    {
        // Плавное появление при запуске
        if (menuCanvasGroup != null)
            StartCoroutine(FadeCanvas(0f, 1f, fadeInDuration));
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        if (mainMenuContainer) mainMenuContainer.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);


    }

    public void CloseSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        if (mainMenuContainer) mainMenuContainer.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            menuCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
    }
}