using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace NeonCore
{
    public class SettingsManager : MonoBehaviour
    {
        [Header("Audio Settings")]
        [Tooltip("Create an AudioMixer in Project > Create > Audio > Audio Mixer named 'MainMixer'")]
        [SerializeField] private AudioMixer mainMixer;
        
        [Header("UI References")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private GameObject settingsPanel;

        private void Start()
        {
            // --- Load Saved Values ---
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

            // --- Apply to Sliders ---
            if (masterSlider)
            {
                masterSlider.value = masterVol;
                masterSlider.onValueChanged.AddListener(SetMasterVolume);
            }
            if (musicSlider)
            {
                musicSlider.value = musicVol;
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }
            if (sfxSlider)
            {
                sfxSlider.value = sfxVol;
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            // --- Apply to Mixer (wait 1 frame or set immediately) ---
            SetMasterVolume(masterVol);
            SetMusicVolume(musicVol);
            SetSFXVolume(sfxVol);
        }

        public void SetMasterVolume(float value)
        {
            // Logarithmic conversion for Mixer (0.0001 to 1 -> -80db to 0db)
            float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
            if (mainMixer) mainMixer.SetFloat("MasterVolume", db);
            
            PlayerPrefs.SetFloat("MasterVolume", value);
        }

        public void SetMusicVolume(float value)
        {
            float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
            if (mainMixer) mainMixer.SetFloat("MusicVolume", db);

            PlayerPrefs.SetFloat("MusicVolume", value);
        }

        public void SetSFXVolume(float value)
        {
            float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
            if (mainMixer) mainMixer.SetFloat("SFXVolume", db);

            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        public void CloseSettings()
        {
            if (settingsPanel) settingsPanel.SetActive(false);
            PlayerPrefs.Save();
        }
    }
}
