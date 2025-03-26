using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerData : Resource
{
    // 基本信息
    public string PlayerName { get; private set; }
    public int Level { get; private set; } = 1;
    public string Realm { get; private set; } = "凡人";
    public int Experience { get; private set; } = 0;
    
    // 玩家属性
    private Dictionary<string, int> _attributes = new Dictionary<string, int>();
    
    // 已学习的功法
    private List<string> _learnedTechniques = new List<string>();
    private Dictionary<string, Technique> _allTechniques = new Dictionary<string, Technique>();
    
    // 物品栏
    private Dictionary<string, int> _inventory = new Dictionary<string, int>();
    
    // 升级所需经验值
    private Dictionary<int, int> _experienceRequirements = new Dictionary<int, int>()
    {
        {1, 100},
        {2, 300},
        {3, 600},
        {4, 1000},
        {5, 1500},
        {6, 2200},
        {7, 3000},
        {8, 4000},
        {9, 5500},
        {10, 7000}
    };
    
    // 境界等级对应表
    private Dictionary<int, string> _realmMapping = new Dictionary<int, string>()
    {
        {1, "凡人"},
        {2, "练气期"},
        {3, "筑基期"},
        {4, "金丹期"},
        {5, "元婴期"},
        {6, "化神期"},
        {7, "炼虚期"},
        {8, "合体期"},
        {9, "大乘期"},
        {10, "渡劫期"}
    };
    
    // 临时加成数据结构
    private Dictionary<string, (float value, float duration, float elapsed)> _temporaryBonuses = new Dictionary<string, (float, float, float)>();
    
    // 玩家卡牌收集
    public List<Card> CardCollection { get; private set; } = new List<Card>();
    
    // 玩家当前牌组
    public List<Card> Deck { get; private set; } = new List<Card>();
    
    // 构造函数
    public PlayerData(string playerName = "修仙者")
    {
        PlayerName = playerName;
        
        // 初始化属性
        _attributes["气力"] = 10;
        _attributes["神识"] = 10;
        _attributes["体魄"] = 10;
        _attributes["命运"] = 10;
    }
    
    // 添加技能到可学习列表
    public void AddTechnique(Technique technique)
    {
        if (!_allTechniques.ContainsKey(technique.Name))
        {
            _allTechniques[technique.Name] = technique;
        }
    }
    
    // 学习功法
    public bool LearnTechnique(string techniqueName)
    {
        if (_allTechniques.ContainsKey(techniqueName) && !_learnedTechniques.Contains(techniqueName))
        {
            // 检查等级要求
            if (_allTechniques[techniqueName].RequiredLevel <= Level)
            {
                _learnedTechniques.Add(techniqueName);
                return true;
            }
        }
        return false;
    }
    
    // 获取已学功法列表
    public List<string> GetLearnedTechniques()
    {
        return _learnedTechniques;
    }
    
    // 获取所有可学功法
    public Dictionary<string, Technique> GetAllTechniques()
    {
        return _allTechniques;
    }
    
    // 获取指定功法
    public Technique GetTechnique(string techniqueName)
    {
        if (_allTechniques.ContainsKey(techniqueName))
        {
            return _allTechniques[techniqueName];
        }
        return null;
    }
    
    // 获取属性值
    public int GetAttribute(string attributeName)
    {
        if (_attributes.ContainsKey(attributeName))
        {
            return _attributes[attributeName];
        }
        return 0;
    }
    
    // 增加属性点
    public void AddAttributePoints(string attributeName, int points)
    {
        if (_attributes.ContainsKey(attributeName))
        {
            _attributes[attributeName] += points;
        }
    }
    
    // 修炼得到属性提升
    public void Cultivate(string techniqueName, float cultivationTime)
    {
        if (_learnedTechniques.Contains(techniqueName) && _allTechniques.ContainsKey(techniqueName))
        {
            var technique = _allTechniques[techniqueName];
            
            // 计算修炼时间带来的收益
            float timeBonus = Mathf.Floor(cultivationTime / 60f); // 每分钟一次收益
            
            // 应用属性提升
            foreach (var attributeBonus in technique.AttributeBonus)
            {
                if (_attributes.ContainsKey(attributeBonus.Key))
                {
                    int bonus = (int)(attributeBonus.Value * timeBonus);
                    _attributes[attributeBonus.Key] += bonus;
                }
            }
            
            // 增加经验值
            AddExperience((int)(10 * timeBonus * technique.RequiredLevel));
        }
    }
    
    // 增加经验值并处理升级
    public void AddExperience(int exp)
    {
        Experience += exp;
        CheckLevelUp();
    }
    
    // 检查是否可以升级
    private void CheckLevelUp()
    {
        if (_experienceRequirements.ContainsKey(Level))
        {
            int requiredExp = _experienceRequirements[Level];
            
            if (Experience >= requiredExp)
            {
                // 升级
                Level++;
                Experience -= requiredExp;
                
                // 更新境界
                UpdateRealm();
                
                // 递归检查是否可以连续升级
                CheckLevelUp();
            }
        }
    }
    
    // 更新境界
    private void UpdateRealm()
    {
        if (_realmMapping.ContainsKey(Level))
        {
            Realm = _realmMapping[Level];
        }
    }
    
    // 获取升级所需经验
    public int GetRequiredExperienceForNextLevel()
    {
        if (_experienceRequirements.ContainsKey(Level))
        {
            return _experienceRequirements[Level];
        }
        
        // 如果超过表中最高等级，就使用最后一级的经验乘以1.5
        int maxLevel = 0;
        foreach (var level in _experienceRequirements.Keys)
        {
            if (level > maxLevel)
            {
                maxLevel = level;
            }
        }
        
        if (maxLevel > 0 && Level > maxLevel)
        {
            return (int)(_experienceRequirements[maxLevel] * 1.5f);
        }
        
        return 999999; // 默认值
    }
    
    // 添加物品到背包
    public void AddInventoryItem(string itemName, int quantity)
    {
        if (_inventory.ContainsKey(itemName))
        {
            _inventory[itemName] += quantity;
        }
        else
        {
            _inventory[itemName] = quantity;
        }
    }
    
    // 从背包移除物品
    public bool RemoveInventoryItem(string itemName, int quantity)
    {
        if (_inventory.ContainsKey(itemName) && _inventory[itemName] >= quantity)
        {
            _inventory[itemName] -= quantity;
            
            // 如果数量为零，移除该物品条目
            if (_inventory[itemName] <= 0)
            {
                _inventory.Remove(itemName);
            }
            
            return true;
        }
        return false;
    }
    
    // 检查物品数量
    public int GetInventoryItemCount(string itemName)
    {
        if (_inventory.ContainsKey(itemName))
        {
            return _inventory[itemName];
        }
        return 0;
    }
    
    // 获取整个物品栏
    public Dictionary<string, int> GetInventory()
    {
        return _inventory;
    }
    
    // 使用物品效果
    public bool UseItem(string itemName)
    {
        // 检查物品是否存在
        if (!_inventory.ContainsKey(itemName) || _inventory[itemName] <= 0)
        {
            return false;
        }
        
        // 应用物品效果
        bool effectApplied = ApplyItemEffect(itemName);
        
        if (effectApplied)
        {
            // 使用物品后移除一个
            RemoveInventoryItem(itemName, 1);
            return true;
        }
        
        return false;
    }
    
    // 应用物品效果
    private bool ApplyItemEffect(string itemName)
    {
        switch (itemName)
        {
            case "低级灵石":
                AddAttributePoints("气力", 3);
                return true;
            case "中级灵石":
                AddAttributePoints("气力", 6);
                return true;
            case "高级灵石":
                AddAttributePoints("气力", 10);
                return true;
            case "灵药草":
                AddAttributePoints("体魄", 2);
                return true;
            case "水灵果":
                AddAttributePoints("神识", 3);
                return true;
            case "灵溪泉水":
                AddAttributePoints("神识", 4);
                return true;
            case "妖兽皮毛":
                AddAttributePoints("体魄", 1);
                return true;
            case "妖丹":
                AddAttributePoints("气力", 5);
                AddAttributePoints("体魄", 5);
                return true;
            case "普通灵草":
                AddAttributePoints("体魄", 2);
                return true;
            case "珍稀灵草":
                AddAttributePoints("体魄", 5);
                return true;
            case "百年灵芝":
                AddAttributePoints("体魄", 10);
                return true;
            default:
                // 秘籍类物品，需要学习而不是直接使用
                if (itemName.Contains("秘籍") || itemName.Contains("功法"))
                {
                    // 这里可以添加学习功法的逻辑
                    return false; // 返回false因为不消耗物品
                }
                return false;
        }
    }
    
    // 设置临时加成
    public void SetTemporaryBonus(string bonusType, float value, float duration)
    {
        _temporaryBonuses[bonusType] = (value, duration, 0);
    }
    
    // 获取临时加成值
    public float GetTemporaryBonus(string bonusType)
    {
        if (_temporaryBonuses.ContainsKey(bonusType) && _temporaryBonuses[bonusType].elapsed < _temporaryBonuses[bonusType].duration)
        {
            return _temporaryBonuses[bonusType].value;
        }
        return 0f;
    }
    
    // 更新临时加成状态 (在游戏循环中调用)
    public void UpdateTemporaryBonuses(float delta)
    {
        List<string> expiredBonuses = new List<string>();
        
        foreach (var key in _temporaryBonuses.Keys)
        {
            var (value, duration, elapsed) = _temporaryBonuses[key];
            float newElapsed = elapsed + delta;
            
            if (newElapsed >= duration)
            {
                expiredBonuses.Add(key);
            }
            else
            {
                _temporaryBonuses[key] = (value, duration, newElapsed);
            }
        }
        
        // 移除过期的加成
        foreach (var key in expiredBonuses)
        {
            _temporaryBonuses.Remove(key);
        }
    }
    
    // 判断是否有指定的临时加成
    public bool HasTemporaryBonus(string bonusType)
    {
        return _temporaryBonuses.ContainsKey(bonusType) && 
               _temporaryBonuses[bonusType].elapsed < _temporaryBonuses[bonusType].duration;
    }
    
    // 突破境界相关方法可以在这里添加
    
    // 初始化牌组
    public void InitializeStarterDeck()
    {
        // 清空当前牌组
        Deck.Clear();
        
        // 添加基础攻击牌
        for (int i = 0; i < 5; i++)
        {
            var strikeCard = new Card
            {
                Name = "打击",
                Description = "造成6点伤害",
                Cost = 1,
                Type = CardType.Attack,
                ImagePath = "res://Resources/Images/Cards/strike.png",
                Effects = new List<CardEffect> { new DamageEffect { DamageAmount = 6 } }
            };
            Deck.Add(strikeCard);
        }
        
        // 添加基础防御牌
        for (int i = 0; i < 5; i++)
        {
            var defendCard = new Card
            {
                Name = "防御",
                Description = "获得5点格挡",
                Cost = 1,
                Type = CardType.Defense,
                ImagePath = "res://Resources/Images/Cards/defend.png",
                Effects = new List<CardEffect> { new BlockEffect { BlockAmount = 5 } }
            };
            Deck.Add(defendCard);
        }
        
        // 添加初始特殊牌
        var qiFlowCard = new Card
        {
            Name = "气流",
            Description = "抽2张牌",
            Cost = 1,
            Type = CardType.Skill,
            ImagePath = "res://Resources/Images/Cards/qiflow.png",
            Effects = new List<CardEffect> { new DrawCardEffect { CardCount = 2 } }
        };
        Deck.Add(qiFlowCard);
        
        // 同时添加到收集中
        foreach (var card in Deck)
        {
            if (!CardCollection.Any(c => c.Name == card.Name))
            {
                CardCollection.Add(card.Clone());
            }
        }
    }
    
    // 添加卡牌到收集
    public void AddCardToCollection(Card card)
    {
        // 检查是否已经收集
        if (!CardCollection.Any(c => c.Name == card.Name))
        {
            CardCollection.Add(card.Clone());
        }
    }
    
    // 添加卡牌到牌组
    public void AddCardToDeck(Card card)
    {
        Deck.Add(card.Clone());
    }
    
    // 从牌组中移除卡牌
    public void RemoveCardFromDeck(string cardName)
    {
        var card = Deck.FirstOrDefault(c => c.Name == cardName);
        if (card != null)
        {
            Deck.Remove(card);
        }
    }
    
    // 获取玩家牌组中某种类型的卡牌
    public List<Card> GetCardsByType(CardType type)
    {
        return Deck.Where(c => c.Type == type).ToList();
    }
}

// 修仙境界枚举
public enum CultivationRealm
{
    Mortal,         // 凡人
    QiRefining,     // 炼气期
    Foundation,     // 筑基期
    CoreFormation,  // 结丹期
    NascentSoul,    // 元婴期
    SpiritSevering, // 化神期
    Ascension       // 飞升期
} 