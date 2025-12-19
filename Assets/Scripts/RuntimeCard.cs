using System.Collections.Generic;
using UnityEngine;

namespace NeonCore
{
    // Class này chứa thông tin thẻ bài được sinh ra trong lúc chơi (không phải file asset gốc)
    [System.Serializable]
    public class RuntimeCard
    {
        public UpgradeCardData template; // Thẻ gốc làm nền
        public CardRarity rarity;        // Phẩm chất được random ra
        public float finalValue;         // Giá trị cuối cùng (đã nhân hệ số)
        
        // Danh sách các thuộc tính phụ (Bonus)
        public List<BonusAttribute> bonuses = new List<BonusAttribute>();

        public RuntimeCard(UpgradeCardData template, CardRarity rarity)
        {
            this.template = template;
            this.rarity = rarity;
            
            // Tính toán giá trị dựa trên phẩm chất
            float multiplier = GetMultiplier(rarity);
            this.finalValue = template.value * multiplier;
            
            // Logic sinh thuộc tính phụ sẽ làm sau
        }

        public string GetDisplayName()
        {
            string prefix = "";
            switch (rarity)
            {
                case CardRarity.Uncommon: prefix = "Good "; break;
                case CardRarity.Rare: prefix = "Rare "; break;
                case CardRarity.Epic: prefix = "Epic "; break;
                case CardRarity.Legendary: prefix = "Legendary "; break;
                case CardRarity.Mythic: prefix = "GODLY "; break;
            }
            return $"{prefix}{template.cardName}";
        }

        public Color GetBorderColor()
        {
            // Bảng màu Neon rực rỡ
            switch (rarity)
            {
                case CardRarity.Common: return new Color(0.8f, 0.8f, 0.8f); // Trắng Xám
                case CardRarity.Uncommon: return new Color(0f, 1f, 0.2f);   // Neon Green (Xanh lá sáng)
                case CardRarity.Rare: return new Color(0f, 0.8f, 1f);       // Cyan (Xanh lơ)
                case CardRarity.Epic: return new Color(0.8f, 0f, 1f);       // Magenta (Tím hồng) - Epic thường tím đẹp hơn vàng
                case CardRarity.Legendary: return new Color(1f, 0.6f, 0f);  // Neon Orange (Cam cháy)
                case CardRarity.Mythic: return new Color(1f, 0f, 0f);       // Red (Đỏ tươi)
                default: return Color.white;
            }
        }

        private float GetMultiplier(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Common: return 1.0f;
                case CardRarity.Uncommon: return 1.2f;
                case CardRarity.Rare: return 1.5f;
                case CardRarity.Epic: return 2.0f;
                case CardRarity.Legendary: return 3.0f;
                case CardRarity.Mythic: return 5.0f;
                default: return 1.0f;
            }
        }
    }

    [System.Serializable]
    public struct BonusAttribute
    {
        public UpgradeType type;
        public float value;
    }
}
