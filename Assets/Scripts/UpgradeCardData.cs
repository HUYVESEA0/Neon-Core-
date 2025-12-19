using UnityEngine;

namespace NeonCore
{
    public enum UpgradeType
    {
        DamageUp,
        FireRateUp,
        RotationSpeedUp,
        HealCore,
        MultiShot, // Ví dụ: Bắn 2-3 tia
        SummonTurret, // Drone thường
        KnockbackUp, 
        SummonLaser,  // Tháp Laser
        SummonBlast,   // Tháp Pháo
        SummonTesla,   // Tháp Sấm Sét
        MaxHealthUp,   // Tăng máu tối đa
        CritChanceUp,  // Tỉ lệ bạo kích
        CritDamageUp,   // Sát thương bạo kích
        
        // --- Nâng cấp Drone cụ thể ---
        DroneNormal_FireRate,
        DroneNormal_Damage,
        
        DroneLaser_Duration, // Laser bắn lâu hơn hoặc xuyên
        DroneLaser_Damage,
        
        DroneBlast_Radius, // Nổ to hơn
        DroneBlast_Damage,
        
        DroneTesla_Chain, // Giật sét lan nhiều con hơn
        DroneTesla_Damage,

        // --- Nâng cấp Đạn Player ---
        Player_Bounce,    // Đạn nảy
        Player_Piercing,  // Đạn xuyên
        Player_Split,      // Đạn phân mảnh
        
        MoveSpeedUp,       // Tốc độ di chuyển Player
        
        // --- Advanced Stats ---
        LifeSteal,         // Hút máu
        DodgeChance,       // Né tránh
        ExecutionThreshold,// Ngưỡng máu tử thần (VD: 15%)
        SlowEffect         // Làm chậm
    }

    public enum CardRarity
    {
        Common,     // Xám
        Uncommon,   // Xanh Lá
        Rare,       // Xanh Lam
        Epic,       // Vàng
        Legendary,  // Cam
        Mythic      // Đỏ
    }

    [CreateAssetMenu(fileName = "NewUpgradeCard", menuName = "NeonCore/Upgrade Card")]
    public class UpgradeCardData : ScriptableObject
    {
        public string cardName;
        [TextArea] public string description;
        public Sprite icon; // Hình ảnh thẻ bài
        
        public UpgradeType upgradeType;
        public float value; // Giá trị tăng thêm (VD: 5 Damage, 0.1 Speed)
    }
}
