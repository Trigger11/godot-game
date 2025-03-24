using Godot;
using System;
using System.Collections.Generic;

public partial class InventoryItem : Resource
{
    // 物品基本属性
    public string Name { get; set; } = "未命名物品";
    public string Description { get; set; } = "这是一个物品。";
    public ItemType Type { get; set; } = ItemType.Other;
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public int Quantity { get; set; } = 1;
    public string IconPath { get; set; } = "res://Resources/Images/default_item.png";
    
    // 物品效果
    public int QiRestore { get; set; } = 0;  // 恢复气力值
    public int SpiritBoost { get; set; } = 0;  // 提升神识值
    public int BodyBoost { get; set; } = 0;   // 提升体魄值
    public int FateBoost { get; set; } = 0;   // 提升命运值
    public int ExpBoost { get; set; } = 0;    // 提升经验值
    
    // 使用物品
    public void Use(PlayerData player)
    {
        // 应用物品效果
        if (QiRestore > 0)
        {
            player.AddAttributePoints("气力", QiRestore);
        }
        
        if (ExpBoost > 0)
        {
            player.AddExperience(ExpBoost);
        }
        
        if (SpiritBoost > 0)
        {
            player.AddAttributePoints("神识", SpiritBoost);
        }
        
        if (BodyBoost > 0)
        {
            player.AddAttributePoints("体魄", BodyBoost);
        }
        
        if (FateBoost > 0)
        {
            player.AddAttributePoints("命运", FateBoost);
        }
        
        // 物品使用后，减少数量
        Quantity--;
    }
    
    // 获取物品详细描述
    public string GetDetailedDescription()
    {
        string result = $"{Name}：{Description}\n";
        result += $"类型：{GetItemTypeString(Type)}\n";
        result += $"品质：{GetRarityString(Rarity)}\n";
        
        // 添加效果描述
        if (QiRestore > 0) result += $"恢复气力：+{QiRestore}\n";
        if (SpiritBoost > 0) result += $"提升神识：+{SpiritBoost}\n";
        if (BodyBoost > 0) result += $"提升体魄：+{BodyBoost}\n";
        if (FateBoost > 0) result += $"提升命运：+{FateBoost}\n";
        if (ExpBoost > 0) result += $"增加经验：+{ExpBoost}\n";
        
        return result;
    }
    
    // 获取物品类型文本
    private string GetItemTypeString(ItemType type)
    {
        switch (type)
        {
            case ItemType.Pill: return "丹药";
            case ItemType.Weapon: return "法器";
            case ItemType.Herb: return "灵草";
            case ItemType.Manual: return "秘籍";
            case ItemType.Material: return "材料";
            default: return "其他";
        }
    }
    
    // 获取物品品质文本
    private string GetRarityString(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "普通";
            case ItemRarity.Uncommon: return "优质";
            case ItemRarity.Rare: return "稀有";
            case ItemRarity.Epic: return "史诗";
            case ItemRarity.Legendary: return "传说";
            default: return "未知";
        }
    }
}

// 物品类型枚举
public enum ItemType
{
    Pill,       // 丹药
    Weapon,     // 法器
    Herb,       // 灵草
    Manual,     // 秘籍
    Material,   // 材料
    Other       // 其他
}

// 物品品质枚举
public enum ItemRarity
{
    Common,     // 普通
    Uncommon,   // 优质
    Rare,       // 稀有
    Epic,       // 史诗
    Legendary   // 传说
} 