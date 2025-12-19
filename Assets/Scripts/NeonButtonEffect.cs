using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NeonCore
{
    [RequireComponent(typeof(Button))]
    public class NeonButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Color neonColor = Color.cyan;
        public float pulseSpeed = 2f;
        public float maxGlowAlpha = 1f;
        public float minGlowAlpha = 0.4f;
        
        private Outline outline;
        private Vector3 originalScale;
        private bool isHovered = false;

        private void Awake()
        {
            outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }
            originalScale = transform.localScale;
        }

        private void Update()
        {
            if (outline != null)
            {
                // Hiệu ứng thở (Pulse)
                float alpha = Mathf.Lerp(minGlowAlpha, maxGlowAlpha, (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) / 2f);
                
                Color c = neonColor;
                c.a = isHovered ? 1f : alpha; // Nếu Hover thì sáng rực, không thì thở
                
                outline.effectColor = c;
                
                // Nếu Hover thì Outline dày hơn chút
                outline.effectDistance = isHovered ? new Vector2(4, -4) : new Vector2(2, -2);
            }

            // Animation Scale mượt mà (UnscaledTime để chạy cả khi game Pause)
            float targetScale = isHovered ? 1.1f : 1f;
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * targetScale, Time.unscaledDeltaTime * 10f);
        }

        public void SetColor(Color color)
        {
            neonColor = color;
            if (outline != null) outline.effectColor = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            // Có thể thêm âm thanh hover ở đây nếu muốn
            // if (SoundManager.Instance) SoundManager.Instance.PlayHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
        }
    }
}
