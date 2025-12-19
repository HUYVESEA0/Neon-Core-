using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace NeonCore
{
    public class CardManager : MonoBehaviour
    {
        public static CardManager Instance;

        [Header("Data")]
        public List<UpgradeCardData> allCards; 

        [Header("UI References")]
        public GameObject cardSelectionPanel; 
        public Button[] cardButtons; 
        
        public PlayerWeapon playerWeapon;
        public CoreHealth coreHealth;

        [Header("Summon Settings")]
        public GameObject turretPrefab; 
        public GameObject laserTurretPrefab; 
        public GameObject blastTurretPrefab; 
        public GameObject teslaTurretPrefab; 
        public float orbitRadius = 3f; 

        private TurretAI activeNormalTurret;
        private TurretAI activeLaserTurret;
        private TurretAI activeBlastTurret;
        private TurretAI activeTeslaTurret;

        private List<DroneOrbit> allActiveDrones = new List<DroneOrbit>();

        private void Awake()
        {
            Instance = this;
            if (cardSelectionPanel != null) cardSelectionPanel.SetActive(false);
            
            // Clear danh sách để tránh lưu vết từ lần chơi trước
            allActiveDrones.Clear(); 
        }

        private void Start()
        {
            InjectVirtualCards();
            
            if (cardSelectionPanel != null) cardSelectionPanel.SetActive(false);
            
            if (playerWeapon == null) playerWeapon = FindFirstObjectByType<PlayerWeapon>();
            if (coreHealth == null) coreHealth = FindFirstObjectByType<CoreHealth>();
        }

        private void InjectVirtualCards()
        {
            void CreateCard(string name, string desc, UpgradeType type, float val)
            {
                UpgradeCardData newCard = ScriptableObject.CreateInstance<UpgradeCardData>();
                newCard.cardName = name;
                newCard.description = desc;
                newCard.upgradeType = type;
                newCard.value = val;
                allCards.Add(newCard);
            }

            CreateCard("Critical Chance", "Increase Crit Chance", UpgradeType.CritChanceUp, 5f);
            CreateCard("Critical Damage", "Increase Crit Damage", UpgradeType.CritDamageUp, 20f);
            CreateCard("Drone Booster", "Buff Normal Drone Damage", UpgradeType.DroneNormal_Damage, 3f);
            CreateCard("Fast Gears", "Normal Drone shoots faster", UpgradeType.DroneNormal_FireRate, 0.2f); // Thẻ mới cho Normal
            
            CreateCard("Laser Overclock", "Buff Laser Drone Damage", UpgradeType.DroneLaser_Damage, 5f);
            // Laser chủ yếu là damage to, có thể thêm Size nếu muốn visual đẹp hơn
           
            CreateCard("Blast Radius", "Expand Blast Radius", UpgradeType.DroneBlast_Radius, 2f); 
            CreateCard("Tesla Voltage", "Buff Tesla Damage", UpgradeType.DroneTesla_Damage, 10f);
            CreateCard("Tesla Overload", "Add +1 Chain Jump", UpgradeType.DroneTesla_Chain, 1f); // Thẻ mới
            CreateCard("Ricochet", "Bullets bounce to targets", UpgradeType.Player_Bounce, 1f);
            CreateCard("Cluster Bomb", "Bullets split on impact", UpgradeType.Player_Split, 1f);
            CreateCard("Piercing Shot", "Bullets pierce through enemies", UpgradeType.Player_Piercing, 1f);
            
            // --- 4. Nhóm Utility ---
            CreateCard("Vampire Fangs", "Life Steal on hit", UpgradeType.LifeSteal, 5f);
            CreateCard("Ninja Cloak", "Chance to Dodge attacks", UpgradeType.DodgeChance, 5f);
            CreateCard("Executioner", "Instantly kill low HP enemies", UpgradeType.ExecutionThreshold, 0.05f); // 5%
            CreateCard("Frost Bullet", "Slow enemies on hit", UpgradeType.SlowEffect, 0.1f); // 10%

            // Dọn dẹp thẻ rác (Value <= 0)
            allCards.RemoveAll(x => x.value <= 0);

            Debug.Log($"✅ Injected virtual cards! Clean Pool Size: {allCards.Count}");
        }

        public void ShowLevelUpCards()
        {
            if (cardSelectionPanel == null) return;

            Time.timeScale = 0f; 
            cardSelectionPanel.SetActive(true);

            if (allCards == null || allCards.Count == 0) return;

            List<RuntimeCard> selectedCards = GenerateRandomCards(3);

            for (int i = 0; i < cardButtons.Length; i++)
            {
                if (i < selectedCards.Count)
                {
                    RuntimeCard card = selectedCards[i];
                    SetupCardButton(cardButtons[i], card);
                    cardButtons[i].gameObject.SetActive(true);
                }
                else
                {
                    cardButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private List<RuntimeCard> GenerateRandomCards(int count)
        {
            // Lọc ra các thẻ hợp lệ (Valid)
            List<UpgradeCardData> validPool = new List<UpgradeCardData>();
            foreach (var card in allCards)
            {
                if (IsCardValid(card.upgradeType))
                {
                    validPool.Add(card);
                }
            }

            List<RuntimeCard> chosen = new List<RuntimeCard>();
            List<UpgradeCardData> tempPool = new List<UpgradeCardData>(validPool);

            for (int i = 0; i < count; i++)
            {
                if (tempPool.Count == 0) break;
                
                int randomIndex = Random.Range(0, tempPool.Count);
                UpgradeCardData template = tempPool[randomIndex];
                
                CardRarity rarity = RollRarity();
                RuntimeCard newCard = new RuntimeCard(template, rarity);
                RollBonuses(newCard);

                chosen.Add(newCard);
                tempPool.RemoveAt(randomIndex); 
            }
            return chosen;
        }

        private bool IsCardValid(UpgradeType type)
        {
            // Helper để kiểm tra xem có drone loại này đang hoạt động không
            bool HasDrone(TurretIdentity identity)
            {
                if (allActiveDrones == null) return false;
                foreach (var drone in allActiveDrones)
                {
                    if (drone == null) continue;
                    var ai = drone.GetComponent<TurretAI>();
                    if (ai != null && ai.identity == identity) return true;
                }
                return false;
            }

            // Kiểm tra điều kiện xuất hiện của thẻ
            switch (type)
            {
                // Chỉ hiện thẻ nâng cấp Normal Drone nếu đã có
                case UpgradeType.DroneNormal_Damage:
                case UpgradeType.DroneNormal_FireRate:
                    return HasDrone(TurretIdentity.Normal);

                // Chỉ hiện thẻ nâng cấp Laser nếu đã có
                case UpgradeType.DroneLaser_Damage:
                case UpgradeType.DroneLaser_Duration:
                    return HasDrone(TurretIdentity.Laser);

                // Chỉ hiện thẻ Blast nếu đã có
                case UpgradeType.DroneBlast_Damage:
                case UpgradeType.DroneBlast_Radius:
                    return HasDrone(TurretIdentity.Blast);

                // Chỉ hiện thẻ Tesla nếu đã có
                case UpgradeType.DroneTesla_Damage:
                case UpgradeType.DroneTesla_Chain:
                    bool b = HasDrone(TurretIdentity.Tesla);
                    // if (b) Debug.Log("DEBUG: Tesla Card Allowed because HasDrone(Tesla) is TRUE");
                    return b;

                // Các thẻ Summon và Player Stats luôn hiện
                default: 
                    return true;
            }
        }

        private CardRarity RollRarity()
        {
            float roll = Random.Range(0f, 100f);
            if (roll < 50f) return CardRarity.Common;    
            if (roll < 75f) return CardRarity.Uncommon;  
            if (roll < 90f) return CardRarity.Rare;      
            if (roll < 97f) return CardRarity.Epic;      
            if (roll < 99.5f) return CardRarity.Legendary; 
            return CardRarity.Mythic;                    
        }

        private void RollBonuses(RuntimeCard card)
        {
            int bonusCount = 0;
            switch (card.rarity)
            {
                case CardRarity.Rare: if (Random.value < 0.3f) bonusCount = 1; break; 
                case CardRarity.Epic: bonusCount = 1; break;
                case CardRarity.Legendary: bonusCount = (Random.value < 0.5f) ? 1 : 2; break; 
                case CardRarity.Mythic: bonusCount = 3; break; 
            }

            if (bonusCount > 0)
            {
                UpgradeType[] possibleBonuses = { 
                    UpgradeType.DamageUp, UpgradeType.FireRateUp, UpgradeType.CritChanceUp, UpgradeType.MoveSpeedUp, UpgradeType.MaxHealthUp 
                };

                for (int i = 0; i < bonusCount; i++)
                {
                    UpgradeType type = possibleBonuses[Random.Range(0, possibleBonuses.Length)];
                    float val = 0;
                    switch (type)
                    {
                        case UpgradeType.DamageUp: val = 2f; break;
                        case UpgradeType.FireRateUp: val = 0.05f; break;
                        case UpgradeType.CritChanceUp: val = 2f; break;
                        case UpgradeType.MaxHealthUp: val = 10f; break;
                        case UpgradeType.MoveSpeedUp: val = 0.5f; break;
                    }

                    if (val > 0) card.bonuses.Add(new BonusAttribute { type = type, value = val });
                }
            }
        }

        private void SetupCardButton(Button btn, RuntimeCard card)
        {
            TextMeshProUGUI titleTxt = btn.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleTxt) 
            {
                titleTxt.text = card.GetDisplayName();
                titleTxt.color = card.GetBorderColor();
                titleTxt.textWrappingMode = TextWrappingModes.Normal; // Fix obsolete warning
            }

            TextMeshProUGUI descTxt = btn.transform.Find("Desc")?.GetComponent<TextMeshProUGUI>();
            if (descTxt) 
            {
                descTxt.color = Color.white; // Ép màu trắng cho dễ đọc
                
                // Format giá trị chính
                string valStr = (card.finalValue < 1f) ? $"{card.finalValue * 100:F0}%" : $"{card.finalValue:F1}";
                string finalText = $"{card.template.description}\n<color=yellow>(+{valStr})</color>";
                
                // Hiển thị từng dòng phụ
                foreach (var bonus in card.bonuses)
                {
                    string bonusName = FormatBonusName(bonus.type);
                    
                    // Format giá trị phụ (Nếu < 1 thì hiển thị %)
                    string bonusValStr = (bonus.value < 1f) ? $"{bonus.value * 100:F0}%" : $"{bonus.value:F1}";
                    
                    finalText += $"\n<color=green>+ {bonusName}: {bonusValStr}</color>";
                }
                
                descTxt.text = finalText;
                descTxt.richText = true; // Bắt buộc bật RichText
            }
            
            Image btnImg = btn.GetComponent<Image>();
            if (btnImg != null) btnImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Nền tối hơn chút

            Outline outline = btn.GetComponent<Outline>();
            if (outline != null)
            {
                // Tắt logic cũ của Outline vì NeonButtonEffect sẽ lo
                // outline.effectColor = card.GetBorderColor();
            }

            // Gắn hiệu ứng Neon
            NeonButtonEffect neonFX = btn.GetComponent<NeonButtonEffect>();
            if (neonFX == null) neonFX = btn.gameObject.AddComponent<NeonButtonEffect>();
            
            neonFX.SetColor(card.GetBorderColor());

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectCard(card));
        }

        private string FormatBonusName(UpgradeType type)
        {
            // Làm đẹp tên hiển thị
            switch (type)
            {
                case UpgradeType.FireRateUp: return "Fire Rate";
                case UpgradeType.DamageUp: return "Damage";
                case UpgradeType.MoveSpeedUp: return "Speed";
                case UpgradeType.CritChanceUp: return "Crit Rate";
                case UpgradeType.MaxHealthUp: return "Max HP";
                default: return type.ToString().Replace("Up", "").Replace("Player_", "").Replace("Drone", "");
            }
        }

        public void SelectCard(RuntimeCard card)
        {
            // Phát âm thanh chọn thẻ
            if (SoundManager.Instance != null && SoundManager.Instance.clickSound != null)
            {
               SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSound, 1f);
            }

            ApplySingleStat(card.template.upgradeType, card.finalValue, card);
            foreach (var bonus in card.bonuses)
            {
                ApplySingleStat(bonus.type, bonus.value, card);
            }
            cardSelectionPanel.SetActive(false);
            Time.timeScale = 1f;
        }
        
        private void ApplySingleStat(UpgradeType type, float value, RuntimeCard cardContext)
        {
            switch (type)
            {
                case UpgradeType.DamageUp: playerWeapon.damage += value; break;
                case UpgradeType.FireRateUp: playerWeapon.fireRate = Mathf.Max(0.05f, playerWeapon.fireRate - value); break;
                case UpgradeType.RotationSpeedUp: playerWeapon.rotationSpeed += value; break;
                case UpgradeType.MultiShot: playerWeapon.projectileCount += (int)value; break;
                case UpgradeType.KnockbackUp: playerWeapon.knockbackForce += value; break;
                
                case UpgradeType.HealCore: if (coreHealth) coreHealth.Heal(value); break;
                case UpgradeType.MaxHealthUp: if (coreHealth) { coreHealth.maxHealth += value; coreHealth.Heal(value); } break;
                
                case UpgradeType.CritChanceUp: playerWeapon.critChance += value; break;
                case UpgradeType.CritDamageUp: playerWeapon.critDamagePercent += value; break;

                // --- Utility Upgrades ---
                case UpgradeType.LifeSteal: playerWeapon.lifeSteal += value; break;
                case UpgradeType.DodgeChance: if (coreHealth) coreHealth.dodgeChance += value; break;
                case UpgradeType.ExecutionThreshold: playerWeapon.executeThreshold += value; break;
                case UpgradeType.SlowEffect: playerWeapon.slowAmount += value; break;
                case UpgradeType.MoveSpeedUp: /* Logic MoveSpeed */ break;

                case UpgradeType.DroneNormal_Damage: UpgradeAllDrones(TurretIdentity.Normal, "damage", value); break;
                case UpgradeType.DroneNormal_FireRate: UpgradeAllDrones(TurretIdentity.Normal, "firerate", value); break;
                case UpgradeType.DroneLaser_Damage: UpgradeAllDrones(TurretIdentity.Laser, "damage", value); break;
                case UpgradeType.DroneBlast_Damage: UpgradeAllDrones(TurretIdentity.Blast, "damage", value); break;
                case UpgradeType.DroneBlast_Radius: UpgradeAllDrones(TurretIdentity.Blast, "radius", value); break;
                case UpgradeType.DroneTesla_Damage: UpgradeAllDrones(TurretIdentity.Tesla, "damage", value); break;
                case UpgradeType.DroneTesla_Chain: UpgradeAllDrones(TurretIdentity.Tesla, "chain", value); break;

                case UpgradeType.Player_Bounce: playerWeapon.bounceCount += (int)Mathf.Max(1, value); break;
                case UpgradeType.Player_Split: playerWeapon.hasSplit = true; break;
                case UpgradeType.Player_Piercing: playerWeapon.piercingCount += (int)Mathf.Max(1, value); break;

                case UpgradeType.SummonTurret: SpawnOrUpgradeTurret(UpgradeType.SummonTurret, turretPrefab); break;
                case UpgradeType.SummonLaser: SpawnOrUpgradeTurret(UpgradeType.SummonLaser, laserTurretPrefab); break;
                case UpgradeType.SummonBlast: SpawnOrUpgradeTurret(UpgradeType.SummonBlast, blastTurretPrefab); break;
                case UpgradeType.SummonTesla: SpawnOrUpgradeTurret(UpgradeType.SummonTesla, teslaTurretPrefab); break;
            }
        }

        private void SpawnOrUpgradeTurret(UpgradeType type, GameObject prefabToSpawn)
        {
            TurretAI existingTurret = null;
            if (type == UpgradeType.SummonTurret) existingTurret = activeNormalTurret;
            else if (type == UpgradeType.SummonLaser) existingTurret = activeLaserTurret;
            else if (type == UpgradeType.SummonBlast) existingTurret = activeBlastTurret;
            else if (type == UpgradeType.SummonTesla) existingTurret = activeTeslaTurret;

            if (existingTurret != null)
            {
                existingTurret.LevelUp();
            }
            else if (prefabToSpawn != null)
            {
                Vector3 spawnPos = playerWeapon ? playerWeapon.transform.position : Vector3.zero;
                GameObject newObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                TurretAI aiScript = newObj.GetComponent<TurretAI>();
                DroneOrbit orbitScript = newObj.GetComponent<DroneOrbit>();
                
                // Debug Identity
                if(aiScript) Debug.Log($"🔥 SUMMONED: {newObj.name} | Identity: {aiScript.identity}");

                if (type == UpgradeType.SummonTurret) activeNormalTurret = aiScript;
                else if (type == UpgradeType.SummonLaser) activeLaserTurret = aiScript;
                else if (type == UpgradeType.SummonBlast) activeBlastTurret = aiScript;
                else if (type == UpgradeType.SummonTesla) activeTeslaTurret = aiScript;

                if (orbitScript != null)
                {
                    allActiveDrones.Add(orbitScript);
                    RecalculateFormation();
                }
            }
        }

        private void UpgradeAllDrones(TurretIdentity targetType, string stat, float amount)
        {
            foreach (var drone in allActiveDrones)
            {
                if (drone == null) continue;
                TurretAI ai = drone.GetComponent<TurretAI>();
                if (ai != null && ai.identity == targetType)
                {
                    switch (stat)
                    {
                        case "damage": ai.damage += amount; break;
                        case "firerate": ai.fireRate = Mathf.Max(0.05f, ai.fireRate - amount); break;
                        case "radius": /* handled in TurretAI shoot logic via level/radius prop? Nope, need to store it */ 
                            // TurretAI cần biến extraRadius nếu muốn upgrade radius động
                            // Hiện tại TurretAI dùng công thức level để tính radius.
                            // Ta có thể tăng level giả? Hoặc thêm biến splashRadiusMod
                            ai.level += (int)amount; // Tạm thời tăng level để tăng radius
                            break;
                        case "chain":
                            // Tăng level để tăng số lần nảy (công thức bounce = 3 + level)
                             ai.level += (int)amount; 
                             break;
                    }
                    ai.LevelUp(); 
                }
            }
        }

        private void RecalculateFormation()
        {
            if (allActiveDrones.Count == 0) return;
            float angleStep = 360f / allActiveDrones.Count;
            for (int i = 0; i < allActiveDrones.Count; i++)
            {
                if (allActiveDrones[i] != null) allActiveDrones[i].SetAngle(i * angleStep);
            }
        }
    }
}
