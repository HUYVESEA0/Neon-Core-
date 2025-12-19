using UnityEngine;
using System.Collections.Generic;

namespace NeonCore.Visuals
{
    // Script nhỏ gắn vào ô sáng để xử lý hiệu ứng mờ dần
    public class GridHighlight : MonoBehaviour
    {
        private SpriteRenderer sr;
        private float lifeTime;
        private float currentLife;
        private Color baseColor;

        public void Setup(float duration, Color color, float size)
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            
            // Tạo texture 1x1 màu trắng nếu chưa có (để tint màu)
            if (sr.sprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                // PPU = 1 means 1 pixel = 1 unit. So 1x1 texture = 1x1 unit size.
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }

            baseColor = color;
            lifeTime = duration;
            currentLife = 0;
            transform.localScale = Vector3.one * size;

            UpdateColor(0);
        }

        private void Update()
        {
            currentLife += Time.deltaTime;
            float progress = currentLife / lifeTime;

            if (progress >= 1f)
            {
                Destroy(gameObject); // Hoặc return pool
                return;
            }

            // Hiệu ứng: Fade In nhanh -> Fade Out chậm
            float alpha = 0f;
            if (progress < 0.2f)
                alpha = progress / 0.2f; // Fade In
            else
                alpha = 1f - ((progress - 0.2f) / 0.8f); // Fade Out

            UpdateColor(alpha * 0.5f); // Max alpha = 0.5 để không quá chói
        }

        void UpdateColor(float a)
        {
            Color c = baseColor;
            c.a = a;
            sr.color = c;
        }
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class ProceduralGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int textureWidth = 256;
        public int textureHeight = 256;
        public int cellSize = 32; // Pixels per grid cell
        public int lineWidth = 2; // Pixels for line thickness
        
        [Header("Colors")]
        public Color backgroundColor = new Color(0.02f, 0.02f, 0.06f); // #050510
        public Color gridColor = new Color(0.12f, 0.12f, 0.21f);       // #1F1F35
        public Color highlightColor = new Color(0.2f, 0.8f, 1f);       // Cyan Neon

        [Header("Animation")]
        public float spawnInterval = 0.1f;
        public float glowDuration = 2.0f;
        public int density = 5; // Spawn bao nhiêu ô mỗi lần

        private float timer;

        // Lưu thông số world space để tính vị trí spawn
        private float worldCellSize;

        private void Start()
        {
            GenerateGridTexture();
            ResizeToScreen();
            
            // Tính kích thước ô lưới trong World Space
            // Texture pixel to unit ratio is 100 by default.
            // cellSize pixels = cellSize / 100 units.
            worldCellSize = (float)cellSize / 100f; // Giả sử PPU 100
        }

        private void Update()
        {
            // Tự động thay đổi kích thước Grid nếu Camera Zoom
            ResizeToScreen();

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SpawnRandomGlows();
                timer = spawnInterval;
            }
        }

        void SpawnRandomGlows()
        {
            Camera cam = Camera.main;
            float height = cam.orthographicSize;
            float width = height * cam.aspect;

            for (int i = 0; i < density; i++)
            {
                // Random vị trí trong màn hình
                float randX = Random.Range(-width, width);
                float randY = Random.Range(-height, height);

                // Snap vào lưới (Grid Snapping)
                // Cần offset một chút để ô sáng nằm chính giữa ô lưới
                // Giả sử lưới bắt đầu từ 0,0
                float snappedX = Mathf.Floor(randX / worldCellSize) * worldCellSize + (worldCellSize / 2f);
                float snappedY = Mathf.Floor(randY / worldCellSize) * worldCellSize + (worldCellSize / 2f);

                Vector3 pos = new Vector3(snappedX, snappedY, 0.1f); // Z > 0 chút để nằm sau Grid nếu Grid ở Z=0

                // Grid background đang vẽ đè lên tất cả ở Z=0?
                // SpriteRenderer Grid đang ở Layer mặc định. Ta nên để Glow thấp hơn hoặc cao hơn tùy OrderInLayer.
                // Ở đây ta tạo GameObject con.
                
                CreateGlowCell(pos);
            }
        }

        void CreateGlowCell(Vector3 pos)
        {
            GameObject cell = new GameObject("GlowCell");
            cell.transform.SetParent(transform);
            cell.transform.position = pos;

            // Chỉnh Order in Layer cao hơn background chút (Background thường là 0 hoặc -10)
            // Ta set 1 để nó sáng lên trên nền đen
            var hl = cell.AddComponent<GridHighlight>();
            
            // Kích thước ô sáng phải nhỏ hơn ô lưới 1 xíu để lộ đường kẻ
            float size = worldCellSize * 0.9f; 
            
            hl.Setup(glowDuration, highlightColor, size);
            
            // Set sorting layer if need
            var sr = cell.GetComponent<SpriteRenderer>();
            sr.sortingOrder = -5; // Nằm sau Tower (Tower 0), nằm trên nền (-10)
        }

        private void GenerateGridTexture()
        {
            Texture2D texture = new Texture2D(textureWidth, textureHeight);
            texture.filterMode = FilterMode.Point; // Keep sharp lines
            texture.wrapMode = TextureWrapMode.Repeat;

            // Fill background
            Color[] pixels = new Color[textureWidth * textureHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = backgroundColor;
            }

            // Draw Grid Lines
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    // Draw vertical lines every cellSize
                    bool isVerticalLine = x % cellSize < lineWidth;
                    // Draw horizontal lines every cellSize
                    bool isHorizontalLine = y % cellSize < lineWidth;

                    if (isVerticalLine || isHorizontalLine)
                    {
                        pixels[y * textureWidth + x] = gridColor;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            // Assign to SpriteRenderer
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            
            // Set background sorting layer to very low
            sr.sortingOrder = -10;
        }

        private void ResizeToScreen()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Camera cam = Camera.main;
            float height = 2f * cam.orthographicSize;
            float width = height * cam.aspect;
            sr.size = new Vector2(width * 2, height * 2);
        }
    }
}
