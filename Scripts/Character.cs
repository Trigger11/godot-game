using Godot;
using System;
using System.Collections.Generic;

public partial class Character : Node
{
	// 角色基本属性
	public new string Name { get; set; }
	public int Level { get; set; }
	
	// 战斗属性
	public int MaxHealth { get; set; }
	public int CurrentHealth { get; set; }
	public int MaxQi { get; set; }
	public int CurrentQi { get; set; }
	public int Attack { get; set; }
	public int Defense { get; set; }
	public int Spirit { get; set; }
	
	// 战斗状态
	public bool IsDefending { get; set; }
	
	// 技能列表
	public List<Technique> Skills { get; set; } = new List<Technique>();
	
	// 元素属性
	public ElementType ElementalStrength { get; set; } = ElementType.None;
	public ElementType ElementalWeakness { get; set; } = ElementType.None;
	
	// 默认构造函数
	public Character()
	{
		Name = "修仙者";
		Level = 1;
		MaxHealth = 100;
		CurrentHealth = 100;
		MaxQi = 50;
		CurrentQi = 50;
		Attack = 10;
		Defense = 5;
		Spirit = 5;
		IsDefending = false;
		
		// 随机分配元素属性
		Random random = new Random();
		Array elements = Enum.GetValues(typeof(ElementType));
		ElementalStrength = (ElementType)elements.GetValue(random.Next(1, elements.Length));
		
		// 设置元素弱点
		switch (ElementalStrength)
		{
			case ElementType.Fire:
				ElementalWeakness = ElementType.Water;
				break;
			case ElementType.Water:
				ElementalWeakness = ElementType.Earth;
				break;
			case ElementType.Earth:
				ElementalWeakness = ElementType.Wind;
				break;
			case ElementType.Wind:
				ElementalWeakness = ElementType.Metal;
				break;
			case ElementType.Metal:
				ElementalWeakness = ElementType.Fire;
				break;
			default:
				ElementalWeakness = ElementType.None;
				break;
		}
	}
	
	// 根据玩家数据创建角色
	public static Character FromPlayerData(PlayerData playerData)
	{
		Character character = new Character();
		character.Name = "修仙者";
		character.Level = playerData.Level;
		character.MaxHealth = playerData.GetAttribute("体魄") * 10;
		character.CurrentHealth = character.MaxHealth;
		character.MaxQi = playerData.GetAttribute("气力") * 10;
		character.CurrentQi = character.MaxQi;
		character.Attack = playerData.GetAttribute("气力") * 2;
		character.Defense = playerData.GetAttribute("体魄");
		character.Spirit = playerData.GetAttribute("神识");
		
		// 加载战斗技能
		foreach (var techniqueName in playerData.GetLearnedTechniques())
		{
			var technique = playerData.GetTechnique(techniqueName);
			if (technique != null && technique.Type == TechniqueType.Combat)
			{
				character.Skills.Add(technique);
			}
		}
		
		return character;
	}
	
	// 使用技能
	public int UseSkill(Technique skill, Character target)
	{
		if (CurrentQi < skill.QiCost)
			return 0;
		
		CurrentQi -= skill.QiCost;
		
		// 计算基础伤害
		int damage = CalculateSkillDamage(skill, target);
		
		// 造成伤害
		target.TakeDamage(damage);
		
		return damage;
	}
	
	// 计算技能伤害
	private int CalculateSkillDamage(Technique skill, Character target)
	{
		int baseDamage = skill.Power;
		
		// 根据技能元素类型计算加成
		if (skill.ElementType == ElementType.Fire)
		{
			baseDamage += Spirit; // 火系技能受神识加成
		}
		else if (skill.ElementType == ElementType.Water)
		{
			baseDamage += Attack / 2 + Spirit / 2; // 水系技能平衡加成
		}
		else if (skill.ElementType == ElementType.Earth)
		{
			baseDamage += Defense; // 土系技能受防御加成
		}
		else
		{
			baseDamage += Attack; // 默认受攻击力加成
		}
		
		// 减去目标防御
		int finalDamage = baseDamage - target.Defense / 3;
		
		// 随机浮动20%
		float randomFactor = (float)new Random().NextDouble() * 0.4f + 0.8f; // 0.8-1.2范围的随机数
		finalDamage = (int)(finalDamage * randomFactor);
		
		// 确保最低伤害为1
		if (finalDamage < 1) finalDamage = 1;
		
		return finalDamage;
	}
	
	// 承受伤害
	public void TakeDamage(int damage)
	{
		// 如果处于防御状态，伤害减半
		if (IsDefending)
		{
			damage = (int)(damage * 0.5f);
		}
		
		CurrentHealth -= damage;
		
		// 确保生命值不会低于0
		if (CurrentHealth < 0)
			CurrentHealth = 0;
	}
	
	// 恢复生命值
	public void Heal(int amount)
	{
		CurrentHealth += amount;
		
		// 确保生命值不会超过最大值
		if (CurrentHealth > MaxHealth)
			CurrentHealth = MaxHealth;
	}
	
	// 恢复气力
	public void RecoverQi(int amount)
	{
		CurrentQi += amount;
		
		// 确保气力不会超过最大值
		if (CurrentQi > MaxQi)
			CurrentQi = MaxQi;
	}
	
	// 判断是否已死亡
	public bool IsDead()
	{
		return CurrentHealth <= 0;
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
				target.Heal(5 * Intensity);
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
