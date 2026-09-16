using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Панели")]
    public GameObject pauseButton;
    public GameObject pausePanel;
    public GameObject settingsPanel;

    private bool isPaused = false;

    void Update()
    {
        // Пауза по Escape
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel != null && settingsPanel.activeSelf) return;
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (pauseButton != null) pauseButton.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);

        //Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);

        //Cursor.visible = false;
    }

    public void OpenSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void QuitToMainMenu()
    {
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }
}