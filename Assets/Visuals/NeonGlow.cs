using UnityEngine;

namespace NeonCore.Visuals
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class NeonGlow : MonoBehaviour
    {
        [Header("Neon Settings")]
        [ColorUsage(true, true)] // Enables HDR Color picker in Inspector
        public Color neonColor = Color.cyan;
        
        [Range(0f, 10f)]
        public float intensity = 2f;

        private SpriteRenderer spriteRenderer;
        private Material instanceMaterial;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Create a material instance to avoid changing all objects
            // We use the default sprite material but boost the color
            instanceMaterial = new Material(Shader.Find("Sprites/Default"));
            spriteRenderer.material = instanceMaterial;
            
            UpdateGlow();
        }

        private void OnValidate()
        {
            if (spriteRenderer != null || TryGetComponent(out spriteRenderer))
            {
                // Only update in editor if we have the material instance or if we are just testing
                // Note: Instantiating material in OnValidate is bad practice, so we just skip until runtime
                // or we use PropertyBlock for better performance if this was a larger project.
            }
        }

        private void Update()
        {
            // Update every frame for testing (can be removed for optimization)
            UpdateGlow();
        }

        public void UpdateGlow()
        {
            if (spriteRenderer == null) return;

            // Simple scaling of color by intensity
            // For Sprites/Default, the main 'Color' property controls the tint.
            // Sending values > 1.0 creates HDR colors which Bloom picks up.
            Color finalColor = neonColor * intensity;
            
            // If using Material Property Block (Better for performance)
            // But for simplicity, we set the material color directly or sprite color
            spriteRenderer.color = finalColor;
        }

        private void OnDestroy()
        {
            if (instanceMaterial != null)
            {
                Destroy(instanceMaterial);
            }
        }
    }
}
