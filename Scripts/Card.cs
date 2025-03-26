using Godot;
using System;
using System.Collections.Generic;

// 卡牌基础类
public partial class Card : Resource
{
    public string Name { get; set; } = "未命名卡牌";
    public string Description { get; set; } = "这是一张卡牌。";
    public int Cost { get; set; } = 1; // 卡牌消耗的气力值
    public CardType Type { get; set; } = CardType.Attack;
    public string ImagePath { get; set; } = "res://Resources/Images/Cards/default_card.png";
    public List<CardEffect> Effects { get; set; } = new List<CardEffect>();

    // 使用卡牌的方法
    public virtual void Play(BattleSystem battleSystem, Character target = null)
    {
        // 执行卡牌效果
        foreach (var effect in Effects)
        {
            effect.Apply(battleSystem, target);
        }
        
        // 记录使用卡牌的日志
        battleSystem.AddBattleLog($"使用了「{Name}」");
    }
    
    // 克隆卡牌
    public Card Clone()
    {
        return new Card
        {
            Name = this.Name,
            Description = this.Description,
            Cost = this.Cost,
            Type = this.Type,
            ImagePath = this.ImagePath,
            Effects = new List<CardEffect>(this.Effects)
        };
    }
}

// 卡牌类型
public enum CardType
{
    Attack,     // 攻击卡
    Defense,    // 防御卡
    Skill,      // 技能卡
    Power       // 能力卡（持续效果）
}

// 卡牌效果基类
public partial class CardEffect : Resource
{
    public virtual void Apply(BattleSystem battleSystem, Character target = null)
    {
        // 空实现，由子类重写
    }
}

// 伤害效果
public partial class DamageEffect : CardEffect
{
    public int DamageAmount { get; set; }
    
    public override void Apply(BattleSystem battleSystem, Character target = null)
    {
        // 对目标造成伤害
        if (target != null)
        {
            target.TakeDamage(DamageAmount);
            battleSystem.AddBattleLog($"对 {target.Name} 造成了 {DamageAmount} 点伤害！");
        }
    }
}

// 防御效果
public partial class BlockEffect : CardEffect
{
    public int BlockAmount { get; set; }
    
    public override void Apply(BattleSystem battleSystem, Character target = null)
    {
        // 增加防御值
        var player = battleSystem.Player;
        player.AddBlock(BlockAmount);
        battleSystem.AddBattleLog($"获得了 {BlockAmount} 点防御！");
    }
}

// 抽牌效果
public partial class DrawCardEffect : CardEffect
{
    public int CardCount { get; set; }
    
    public override void Apply(BattleSystem battleSystem, Character target = null)
    {
        // 抽取指定数量的卡牌
        for (int i = 0; i < CardCount; i++)
        {
            battleSystem.DrawCard();
        }
        battleSystem.AddBattleLog($"抽取了 {CardCount} 张卡牌！");
    }
}

// 获得气力效果
public partial class GainEnergyEffect : CardEffect
{
    public int EnergyAmount { get; set; }
    
    public override void Apply(BattleSystem battleSystem, Character target = null)
    {
        // 增加气力值
        battleSystem.AddEnergy(EnergyAmount);
        battleSystem.AddBattleLog($"获得了 {EnergyAmount} 点气力！");
    }
}

// 施加状态效果
public partial class ApplyStatusEffect : CardEffect
{
    public string StatusName { get; set; }
    public int StatusAmount { get; set; }
    
    public override void Apply(BattleSystem battleSystem, Character target = null)
    {
        if (target != null)
        {
            // 添加状态效果到目标
            target.AddStatus(StatusName, StatusAmount);
            battleSystem.AddBattleLog($"对 {target.Name} 施加了 {StatusAmount} 层 {StatusName}！");
        }
    }
} 