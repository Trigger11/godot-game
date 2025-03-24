using Godot;
using System;
using System.Collections.Generic;

public partial class Character : Resource
{
    // 基本属性
    public string Name { get; set; } = "未命名角色";
    public string Description { get; set; } = "这是一个角色。";
    public int Level { get; set; } = 1;
    public int CurrentHealth { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int CurrentQi { get; set; } = 50;
    public int MaxQi { get; set; } = 50;
    
    // 战斗属性
    public int Attack { get; set; } = 10;
    public int Defense { get; set; } = 5;
    public int Speed { get; set; } = 5;
    
    // 技能列表
    public List<Technique> Techniques { get; set; } = new List<Technique>();
    
    // 状态效果
    public List<StatusEffect> StatusEffects { get; set; } = new List<StatusEffect>();
    
    // 战斗中受到伤害
    public virtual int TakeDamage(int amount, TechniqueElement element = TechniqueElement.Neutral)
    {
        // 计算实际伤害
        int actualDamage = Math.Max(1, amount - Defense / 2);
        
        // 应用元素相克关系（如果有的话）
        // TODO: 实现元素相克系统
        
        // 应用伤害
        CurrentHealth = Math.Max(0, CurrentHealth - actualDamage);
        
        return actualDamage;
    }
    
    // 回复生命值
    public virtual int HealHealth(int amount)
    {
        int previousHealth = CurrentHealth;
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        return CurrentHealth - previousHealth;
    }
    
    // 回复气力
    public virtual int RestoreQi(int amount)
    {
        int previousQi = CurrentQi;
        CurrentQi = Math.Min(MaxQi, CurrentQi + amount);
        return CurrentQi - previousQi;
    }
    
    // 检查是否死亡
    public bool IsDead()
    {
        return CurrentHealth <= 0;
    }
    
    // 添加状态效果
    public void AddStatusEffect(StatusEffect effect)
    {
        // 检查是否已有同类效果
        StatusEffect existingEffect = StatusEffects.Find(e => e.Type == effect.Type);
        if (existingEffect != null)
        {
            // 更新持续时间或叠加效果
            existingEffect.Duration = Math.Max(existingEffect.Duration, effect.Duration);
            existingEffect.Intensity += effect.Intensity;
        }
        else
        {
            StatusEffects.Add(effect);
        }
    }
    
    // 应用状态效果
    public void ApplyStatusEffects()
    {
        for (int i = StatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = StatusEffects[i];
            
            // 应用效果
            effect.Apply(this);
            
            // 减少持续时间
            effect.Duration--;
            
            // 移除已过期效果
            if (effect.Duration <= 0)
            {
                StatusEffects.RemoveAt(i);
            }
        }
    }
    
    // 使用技能
    public virtual int UseTechnique(int techniqueIndex, Character target)
    {
        if (techniqueIndex < 0 || techniqueIndex >= Techniques.Count)
            return 0;
            
        Technique technique = Techniques[techniqueIndex];
        
        // 检查气力是否足够
        if (CurrentQi < technique.QiCost)
            return 0;
            
        // 消耗气力
        CurrentQi -= technique.QiCost;
        
        // 使用技能并返回造成的伤害
        return technique.UseInBattle(this, target);
    }
}

// 状态效果类
public partial class StatusEffect
{
    public StatusEffectType Type { get; set; } = StatusEffectType.Buff;
    public int Duration { get; set; } = 3;
    public int Intensity { get; set; } = 1;
    
    public virtual void Apply(Character target)
    {
        // 根据状态类型应用不同效果
        switch (Type)
        {
            case StatusEffectType.Poison:
                // 中毒伤害
                target.TakeDamage(5 * Intensity);
                break;
            case StatusEffectType.Heal:
                // 回复效果
                target.HealHealth(5 * Intensity);
                break;
            case StatusEffectType.AttackBuff:
                // 攻击增益（临时实现）
                break;
            // 可以添加更多效果类型
        }
    }
}

// 状态效果类型
public enum StatusEffectType
{
    Buff,       // 增益效果
    Debuff,     // 减益效果
    Poison,     // 中毒效果
    Burn,       // 燃烧效果
    Freeze,     // 冰冻效果
    Stun,       // 眩晕效果
    Heal,       // 治疗效果
    AttackBuff, // 攻击增益
    DefenseBuff // 防御增益
} 