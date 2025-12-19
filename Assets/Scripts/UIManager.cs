using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NeonCore
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("Health Bar")]
        public Slider healthSlider;
        public Image healthFill; // Để đổi màu nếu máu thấp (Option)

        [Header("XP Bar")]
        public Slider xpSlider;
        public TextMeshProUGUI levelText;

        private void Awake()
        {
            Instance = this;
        }

        public void UpdateHealth(float current, float max)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }
        }

        public void UpdateXP(int current, int target, int level)
        {
            if (xpSlider != null)
            {
                xpSlider.maxValue = target;
                xpSlider.value = current;
            }

            if (levelText != null)
            {
                levelText.text = "LV " + level;
            }
        }
    }
}
