using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NeonCore
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Scene Config")]
        [Tooltip("The Build Index of the gameplay scene.")]
        [SerializeField] private int gameplaySceneIndex = 1;

        // Static configuration to pass to Gameplay Scene
        public static bool IsEndlessMode = false;
        public static int SelectedLevelDifficulty = 1; // 1=Easy, 2=Hard 

        [Header("Audio")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        [Header("Visuals")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private UnityEngine.UI.Button btnLevel2;
        [SerializeField] private UnityEngine.UI.Button btnLevel3;
        [SerializeField] private UnityEngine.UI.Button btnLevel4;
        [SerializeField] private UnityEngine.UI.Button btnLevel5;
        [SerializeField] private UnityEngine.UI.Toggle endlessToggle; // Checkbox for Endless
        [SerializeField] private TMPro.TextMeshProUGUI highScoreText;

        private void Start()
        {
            // Auto-play BGM if assigned
            if (bgmSource != null && !bgmSource.isPlaying)
            {
                bgmSource.loop = true;
                bgmSource.Play();
            }

            // Display High Score
            if (highScoreText != null)
            {
                int bestScore = PlayerPrefs.GetInt("HighScore", 0);
                highScoreText.text = $"BEST: {bestScore}";
            }
        }

        public void PlayHoverSound()
        {
            if (sfxSource != null && hoverSound != null)
            {
                sfxSource.PlayOneShot(hoverSound);
            }
        }

        public void PlayClickSound()
        {
            if (sfxSource != null && clickSound != null)
            {
                sfxSource.PlayOneShot(clickSound);
            }
        }

        public void OpenLevelSelect()
        {
            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(true);
                UpdateLevelButtons();
            }
        }

        public void CloseLevelSelect()
        {
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        }

        private void UpdateLevelButtons()
        {
            // Progress: 1 = Beat Lvl 1, 2 = Beat Lvl 2 ... 5 = Beat Lvl 5 (Endless Unlocked)
            int progress = PlayerPrefs.GetInt("CareerProgress", 0);

            if (btnLevel2) btnLevel2.interactable = (progress >= 1);
            if (btnLevel3) btnLevel3.interactable = (progress >= 2);
            if (btnLevel4) btnLevel4.interactable = (progress >= 3);
            if (btnLevel5) btnLevel5.interactable = (progress >= 4);
            
            // Endless Toggle only available if you beat at least Mission 1
            if (endlessToggle) 
            {
                endlessToggle.interactable = (progress >= 1);
                if (progress < 1) endlessToggle.isOn = false;
            }
        }

        public void PlayLevel(int difficulty)
        {
            // Read the toggle state
            IsEndlessMode = (endlessToggle != null && endlessToggle.isOn);
            SelectedLevelDifficulty = difficulty;
            
            SceneManager.LoadScene(gameplaySceneIndex);
        }

        // Removed PlayEndless as it is now integrated into PlayLevel via Toggle
        /* public void PlayEndless() ... */

        // Old PlayGame method redirects to Level Select now
        public void PlayGame()
        {
            OpenLevelSelect(); 
        }

        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void QuitGame()
        {
            PlayClickSound();
            Debug.Log("Quit Game triggered.");
            Application.Quit();

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }
}
