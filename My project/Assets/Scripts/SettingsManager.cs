using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Элементы")]
    public Slider volumeSlider;
    public Text volumeLabel; 
    public Toggle fullscreenToggle;

    void Start()
    {
        // Загружаем сохранённые значения
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        // Применяем к UI
        if (volumeSlider != null) volumeSlider.value = savedVolume;
        if (fullscreenToggle != null) fullscreenToggle.isOn = savedFullscreen;
        UpdateVolumeLabel(savedVolume);

        // Применяем к игре
        ApplyAudio(savedVolume);
        ApplyFullscreen(savedFullscreen);
    }

    // Вызываем слайдером
    public void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        ApplyAudio(value);
        UpdateVolumeLabel(value);
    }

    // Вызываем переключателем
    public void OnFullscreenChanged(bool isOn)
    {
        PlayerPrefs.SetInt("Fullscreen", isOn ? 1 : 0);
        ApplyFullscreen(isOn);
    }

    private void ApplyAudio(float volume)
    {
        AudioListener.volume = volume;
    }

    private void ApplyFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        //Debug.Log($"Fullscreen: {Screen.fullScreen}");
    }

    private void UpdateVolumeLabel(float value)
    {
        if (volumeLabel != null)
            volumeLabel.text = $"Громкость: {Mathf.RoundToInt(value * 100)}%";
    }
}