using Godot;
using System;
using System.Collections.Generic;

// 卡牌基础类
public partial class CardInfo : Resource
{
    public string Name { get; set; } = "未命名卡牌";
    public string Description { get; set; } = "这是一张卡牌。";
    public int Cost { get; set; } = 1; // 卡牌消耗的气力值
    public CardType Type { get; set; } = CardType.Attack;
    public CardRarity Rarity { get; set; } = CardRarity.Common; // 新增：卡牌稀有度
    public string ImagePath { get; set; } = "res://Resources/Images/Cards/default_card.png";
    public List<CardEffect> Effects { get; set; } = new List<CardEffect>();
    public bool Upgradable { get; set; } = true; // 新增：是否可升级
    public bool IsUpgraded { get; set; } = false; // 新增：是否已升级

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
    public CardInfo Clone()
    {
        return new CardInfo
        {
            Name = this.Name,
            Description = this.Description,
            Cost = this.Cost,
            Type = this.Type,
            Rarity = this.Rarity,
            ImagePath = this.ImagePath,
            Upgradable = this.Upgradable,
            IsUpgraded = this.IsUpgraded,
            Effects = new List<CardEffect>(this.Effects)
        };
    }
    
    // 升级卡牌 (新增)
    public virtual CardInfo Upgrade()
    {
        if (!Upgradable || IsUpgraded)
            return this;
            
        CardInfo upgradedCard = Clone();
        upgradedCard.IsUpgraded = true;
        upgradedCard.Name = $"{Name}+";
        
        // 根据卡牌类型加强效果
        switch (Type)
        {
            case CardType.Attack:
                // 攻击卡提高伤害
                foreach (var effect in upgradedCard.Effects)
                {
                    if (effect is DamageEffect damageEffect)
                    {
                        damageEffect.DamageAmount += 3;
                    }
                }
                upgradedCard.Description = upgradedCard.Description.Replace(
                    $"{((DamageEffect)Effects.Find(e => e is DamageEffect)).DamageAmount}点伤害", 
                    $"{((DamageEffect)upgradedCard.Effects.Find(e => e is DamageEffect)).DamageAmount}点伤害");
                break;
                
            case CardType.Defense:
                // 防御卡提高格挡
                foreach (var effect in upgradedCard.Effects)
                {
                    if (effect is BlockEffect blockEffect)
                    {
                        blockEffect.BlockAmount += 3;
                    }
                }
                upgradedCard.Description = upgradedCard.Description.Replace(
                    $"{((BlockEffect)Effects.Find(e => e is BlockEffect)).BlockAmount}点格挡", 
                    $"{((BlockEffect)upgradedCard.Effects.Find(e => e is BlockEffect)).BlockAmount}点格挡");
                break;
                
            case CardType.Skill:
                // 技能卡减少费用或增加效果
                if (upgradedCard.Cost > 0)
                {
                    upgradedCard.Cost -= 1;
                    upgradedCard.Description = $"(消耗{upgradedCard.Cost}) " + upgradedCard.Description.Substring(upgradedCard.Description.IndexOf(')') + 1);
                }
                else
                {
                    // 如果费用已经是0，则增强效果
                    foreach (var effect in upgradedCard.Effects)
                    {
                        if (effect is DrawCardEffect drawEffect)
                        {
                            drawEffect.CardCount += 1;
                            upgradedCard.Description = upgradedCard.Description.Replace(
                                $"抽{((DrawCardEffect)Effects.Find(e => e is DrawCardEffect)).CardCount}张牌", 
                                $"抽{drawEffect.CardCount}张牌");
                        }
                        else if (effect is GainEnergyEffect energyEffect)
                        {
                            energyEffect.EnergyAmount += 1;
                            upgradedCard.Description = upgradedCard.Description.Replace(
                                $"获得{((GainEnergyEffect)Effects.Find(e => e is GainEnergyEffect)).EnergyAmount}点气力", 
                                $"获得{energyEffect.EnergyAmount}点气力");
                        }
                    }
                }
                break;
                
            case CardType.Power:
                // 能力卡增强持续效果
                foreach (var effect in upgradedCard.Effects)
                {
                    if (effect is ApplyStatusEffect statusEffect)
                    {
                        statusEffect.StatusAmount += 1;
                        upgradedCard.Description = upgradedCard.Description.Replace(
                            $"{((ApplyStatusEffect)Effects.Find(e => e is ApplyStatusEffect)).StatusAmount}层{statusEffect.StatusName}", 
                            $"{statusEffect.StatusAmount}层{statusEffect.StatusName}");
                    }
                }
                break;
        }
        
        return upgradedCard;
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

// 卡牌稀有度 (新增)
public enum CardRarity
{
    Common,     // 普通
    Uncommon,   // 罕见
    Rare,       // 稀有
    Legendary   // 传说
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
    public bool IgnoreBlock { get; set; } = false; // 新增：是否无视格挡
    
    public override void Apply(BattleSystem battleSystem, Character target = null)
    {
        // 对目标造成伤害
        if (target != null)
        {
            if (IgnoreBlock)
            {
                // 直接扣除生命值，无视格挡
                target.CurrentHealth = Math.Max(0, target.CurrentHealth - DamageAmount);
                battleSystem.AddBattleLog($"无视格挡对 {target.Name} 造成了 {DamageAmount} 点伤害！");
            }
            else
            {
                target.TakeDamage(DamageAmount);
                battleSystem.AddBattleLog($"对 {target.Name} 造成了 {DamageAmount} 点伤害！");
            }
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

// 新增：多重攻击效果
public partial class MultiAttackEffect : CardEffect
{
    public int AttackCount { get; set; }
    public int DamagePerHit { get; set; }
    
    public override void Apply(BattleSystem battleSystem, Character target = null)
    {
        if (target != null)
        {
            battleSystem.AddBattleLog($"对 {target.Name} 发动 {AttackCount} 次攻击！");
            
            for (int i = 0; i < AttackCount; i++)
            {
                target.TakeDamage(DamagePerHit);
                battleSystem.AddBattleLog($"第 {i+1} 击造成 {DamagePerHit} 点伤害！");
            }
        }
    }
}

// 新增：恢复生命效果
public partial class HealEffect : CardEffect
{
    public int HealAmount { get; set; }
    
    public override void Apply(BattleSystem battleSystem, Character target = null)
    {
        Character character = target ?? battleSystem.Player;
        character.Heal(HealAmount);
        battleSystem.AddBattleLog($"{character.Name} 恢复了 {HealAmount} 点生命值！");
    }
}

// 新增：弃牌效果
public partial class DiscardEffect : CardEffect
{
    public int DiscardCount { get; set; }
    
    public override void Apply(BattleSystem battleSystem, Character target = null)
    {
        // 实现弃牌效果
        // 这需要在BattleSystem中添加一个DiscardRandomCards方法
        battleSystem.AddBattleLog($"弃置了 {DiscardCount} 张牌！");
    }
}