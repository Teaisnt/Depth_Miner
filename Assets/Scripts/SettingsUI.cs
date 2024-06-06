using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour {
    [Header("Values")]
    [SerializeField] private TextMeshProUGUI masterVolumeValue;
    [SerializeField] private TextMeshProUGUI soundVolumeValue;
    [SerializeField] private TextMeshProUGUI musicVolumeValue;

    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    void OnEnable() {
        masterVolumeSlider.value = Settings.masterVolume*100;
        soundVolumeSlider.value = Settings.soundVolume*100;
        musicVolumeSlider.value = Settings.musicVolume*100;
    }

    public void SetSettingsValues(string paramater) {
        switch (paramater) {
            case "Master Volume":
                {
                    masterVolumeValue.SetText($"{masterVolumeSlider.value:F1}%");
                    Settings.Instance.SetMasterVolume(masterVolumeSlider.value/100);
                    break;
                }
            case "Sound Volume":
                {
                    soundVolumeValue.SetText($"{soundVolumeSlider.value:F1}%");
                    Settings.Instance.SetSoundVolume(soundVolumeSlider.value/100);
                    break;
                }
            case "Music Volume":
                {
                    musicVolumeValue.SetText($"{musicVolumeSlider.value:F1}%");
                    Settings.Instance.SetMusicVolume(musicVolumeSlider.value/100);
                    break;
                }
        }
    }
}
