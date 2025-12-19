using UnityEngine;
using UnityEngine.UI;

namespace NeonCore.Visuals
{
    [ExecuteAlways] // <--- Dòng này giúp script chạy ngay cả khi chưa bấm Play!
    [RequireComponent(typeof(Image))]
    public class NeonImage : MonoBehaviour
    {
        [Header("Neon Settings")]
        [ColorUsage(true, true)] // Cho phép chọn màu HDR (Có thanh Intensity)
        public Color glowColor = Color.red;
        
        [Range(0f, 5f)]
        public float intensity = 1f;

        private Image targetImage;
        private Material uiMaterial;

        private void OnEnable()
        {
            targetImage = GetComponent<Image>();
            
            // Tạo material mới nếu chưa có hoặc nếu đang dùng mặc định
            if (targetImage.material == null || targetImage.material.name == "Default UI Material")
            {
                uiMaterial = new Material(Shader.Find("Sprites/Default"));
                targetImage.material = uiMaterial;
            }
            else
            {
                uiMaterial = targetImage.material;
            }
            
            UpdateColor();
        }

        private void Update()
        {
            // Cập nhật liên tục khi bạn kéo màu
            if (!Application.isPlaying)
            {
                UpdateColor();
            }
        }

        public void UpdateColor()
        {
            if (targetImage != null)
            {
                // Màu cuối cùng = Màu gốc * Cường độ
                // Ví dụ: Đỏ * 3 = Đỏ Rực (Gây chói Bloom)
                targetImage.color = glowColor * intensity;
            }
        }
    }
}
