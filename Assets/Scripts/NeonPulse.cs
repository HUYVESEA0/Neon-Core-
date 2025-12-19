using UnityEngine;
using TMPro;

namespace NeonCore
{
    public class NeonPulse : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private float speed = 2f;
        [SerializeField] private float minIntensity = 0.5f;
        [SerializeField] private float maxIntensity = 1f;

        private Material materialInstance;

        private void Start()
        {
            if (targetText == null)
            {
                targetText = GetComponent<TextMeshProUGUI>();
            }

            // Create a material instance so we don't change all text in the game
            if (targetText != null)
            {
                materialInstance = new Material(targetText.fontSharedMaterial);
                targetText.fontMaterial = materialInstance;
            }
        }

        private void Update()
        {
            if (materialInstance != null)
            {
                // PingPong oscillation
                float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
                
                // Animate Glow Power (0 to 1 range usually works for Glow Power/Outer)
                float currentPower = Mathf.Lerp(minIntensity, maxIntensity, t);
                
                // "_GlowPower" enables the glow brightness falloff
                materialInstance.SetFloat("_GlowPower", currentPower);
                
                // Optional: Slightly pulsate the outline width or face dilation as well
                // materialInstance.SetFloat("_FaceDilate", Mathf.Lerp(-0.1f, 0f, t));
            }
        }
    }
}
