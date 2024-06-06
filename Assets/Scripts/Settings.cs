using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public class Settings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    public static float masterVolume { get; set; }
    public static float soundVolume { get; set; }
    public static float musicVolume { get; set; }

    public static Settings Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
            Instance = this;
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(this);
        SetMasterVolume(0.5f);
        SetSoundVolume(0.5f);
        SetMusicVolume(0.5f);
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        ApplyAudioSettings();
    }

    public void SetSoundVolume(float value)
    {
        soundVolume = value;
        ApplyAudioSettings();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
        mixer.SetFloat("SoundVolume", Mathf.Log10(soundVolume) * 20);
        mixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
    }

    public class GameSettings
    {
        public float masterVolume = 1;
        public float soundVolume = 1;
        public float musicVolume = 1;
    }
}