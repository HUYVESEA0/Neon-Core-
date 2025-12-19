using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace NeonCore
{
    public class NeonButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Settings")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float transitionSpeed = 10f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.cyan;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private MainMenuManager menuManager;

        private Vector3 targetScale;
        private Color targetColor;

        private void Start()
        {
            if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
            if (menuManager == null) menuManager = FindFirstObjectByType<MainMenuManager>();

            targetScale = Vector3.one;
            targetColor = normalColor;
            
            if (buttonText != null) buttonText.color = normalColor;
        }

        private void Update()
        {
            // Smooth Scaling
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
            
            // Smooth Color
            if (buttonText != null)
            {
                buttonText.color = Color.Lerp(buttonText.color, targetColor, Time.deltaTime * transitionSpeed);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = Vector3.one * hoverScale;
            targetColor = hoverColor;

            // Play Sound
            if (menuManager != null) menuManager.PlayHoverSound();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = Vector3.one;
            targetColor = normalColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetScale = Vector3.one * 0.95f; // Click effect (Press in)
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetScale = Vector3.one * hoverScale; // Return to hover state
        }
    }
}
