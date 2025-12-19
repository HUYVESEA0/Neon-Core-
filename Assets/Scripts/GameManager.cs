using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace NeonCore
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool IsGameOver { get; private set; }
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int Credits { get; private set; } 
        
        [Header("Level System")]
        public int Level { get; private set; } = 1;
        public int CurrentXP { get; private set; }
        public int TargetXP { get; private set; } = 100;

        [Header("Game Settings")]
        public float gameSpeed = 1f;

        [Header("UI References")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI endScoreText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadHighScore();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void GameOver()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            Debug.Log("Game Over! Score: " + Score);
            Time.timeScale = 0f; // Stop game
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                
                // Check High Score
                if (Score > HighScore)
                {
                    HighScore = Score;
                    SaveHighScore();
                }

                if (endScoreText != null)
                {
                    endScoreText.text = $"GAME OVER\n\nSCORE: {Score}\nHIGH SCORE: {HighScore}\nLEVEL: {Level}";
                }
            }
        }

        public void Victory()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            Debug.Log("VICTORY!");
            Time.timeScale = 0.5f; // Slow motion celebration
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                // Save High Score
                if (Score > HighScore)
                {
                    HighScore = Score;
                    SaveHighScore();
                }

                if (endScoreText != null)
                {
                    endScoreText.text = $"<color=yellow>VICTORY!</color>\n\nYOU DEFEATED THE MEGA BOSS!\n\nSCORE: {Score}\nLEVEL: {Level}";
                }
            }
        }

        private void LoadHighScore()
        {
            HighScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("HighScore", HighScore);
            PlayerPrefs.Save();
        }

        public void AddScore(int amount = 1)
        {
            Score += amount;
        }

        public void AddXP(int amount)
        {
            CurrentXP += amount;
            
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateXP(CurrentXP, TargetXP, Level);

            if (CurrentXP >= TargetXP)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            CurrentXP -= TargetXP;
            Level++;
            TargetXP = Mathf.RoundToInt(TargetXP * 1.2f); 
            
            // Cập nhật lại UI sau khi lên cấp (thanh XP về 0)
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateXP(CurrentXP, TargetXP, Level);

            Debug.Log($"Level Up! New Level: {Level}");
            
            // Gọi hiển thị thẻ bài
            if (CardManager.Instance != null)
            {
                CardManager.Instance.ShowLevelUpCards();
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f; // Phải trả lại thời gian về 1 trước khi load
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0); // Assuming MainMenu is at index 0
        }
    }
}
