using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonCore
{
    public class PauseManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;

        private bool isPaused = false;

        private void Update()

        {
            // Debugging input
            // Debug.Log("PauseManager Update running..."); 

            bool escapePressed = false;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("ESC pressed via New Input System");
                escapePressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("ESC pressed via Legacy Input");
                escapePressed = true;
            }
#endif

            if (escapePressed)
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f; // Freeze game time
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f; // Resume game time

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f; // Ensure time is normal before leaving
            SceneManager.LoadScene(0); // Assuming Main Menu is index 0
        }

        public void QuitGame()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
        }
    }
}
