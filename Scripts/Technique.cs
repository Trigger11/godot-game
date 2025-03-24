using Godot;
using System;
using System.Collections.Generic;

// 技能类型枚举
public enum TechniqueType
{
	QiCultivation,  // 气修功法
	BodyRefining,   // 炼体功法
	SpiritCultivation, // 神识修炼
	Combat,         // 战斗技能
	Special         // 特殊能力
}

// 元素类型枚举
public enum ElementType
{
	None,   // 无元素
	Fire,   // 火元素
	Water,  // 水元素
	Earth,  // 土元素
	Wind,   // 风元素
	Metal   // 金元素
}

public partial class Technique : Resource
{
	// 基本信息
	public string Name { get; private set; }
	public string Description { get; private set; }
	public TechniqueType Type { get; private set; }
	public int RequiredLevel { get; private set; }
	
	// 属性加成
	public Dictionary<string, int> AttributeBonus { get; private set; } = new Dictionary<string, int>();
	
	// 战斗相关属性（仅战斗技能有效）
	public int BaseDamage { get; private set; } = 0;
	public int QiCost { get; private set; } = 0;
	public bool IsAOE { get; private set; } = false;
	
	// 新增战斗属性
	public int Power { get; private set; } = 10; // 技能威力
	public ElementType ElementType { get; private set; } = ElementType.None; // 元素类型
	
	// 构造函数
	public Technique(string name, string description, TechniqueType type, int requiredLevel, Dictionary<string, int> attributeBonus)
	{
		Name = name;
		Description = description;
		Type = type;
		RequiredLevel = requiredLevel;
		AttributeBonus = attributeBonus ?? new Dictionary<string, int>();
	}
	
	// 战斗技能专用构造函数
	public Technique(string name, string description, int baseDamage, int qiCost, bool isAOE, int requiredLevel)
	{
		Name = name;
		Description = description;
		Type = TechniqueType.Combat;
		BaseDamage = baseDamage;
		QiCost = qiCost;
		IsAOE = isAOE;
		RequiredLevel = requiredLevel;
	}
	
	// 获取修炼时的描述
	public string GetCultivationDescription()
	{
		string result = $"{Name}: {Description}\n所需等级: {RequiredLevel}\n";
		
		result += "属性提升: ";
		foreach (var bonus in AttributeBonus)
		{
			result += $"{bonus.Key} +{bonus.Value} ";
		}
		
		return result;
	}
	
	// 获取战斗技能描述
	public string GetCombatDescription()
	{
		if (Type != TechniqueType.Combat)
		{
			return $"{Name}: 非战斗技能";
		}
		
		string result = $"{Name}: {Description}\n基础伤害: {BaseDamage}\n气力消耗: {QiCost}\n";
		
		if (IsAOE)
		{
			result += "范围攻击: 是";
		}
		else
		{
			result += "范围攻击: 否";
		}
		
		return result;
	}
	
	// 计算战斗技能实际伤害
	public int CalculateDamage(PlayerData player)
	{
		if (Type != TechniqueType.Combat)
		{
			return 0;
		}
		
		// 伤害计算公式：基础伤害 + (气力 * 0.5) + (体魄 * 0.3)
		int damage = BaseDamage;
		damage += (int)(player.GetAttribute("气力") * 0.5f);
		damage += (int)(player.GetAttribute("体魄") * 0.3f);
		
		return damage;
	}
	
	// 检查能否使用战斗技能（气力是否足够）
	public bool CanUse(PlayerData player)
	{
		if (Type != TechniqueType.Combat)
		{
			return false;
		}
		
		// 检查玩家等级和气力
		return player.Level >= RequiredLevel && player.GetAttribute("气力") >= QiCost;
	}
	
	// 使用战斗技能消耗气力
	public void Use(PlayerData player)
	{
		if (Type == TechniqueType.Combat && player.GetAttribute("气力") >= QiCost)
		{
			// 气力属性减少
			player.AddAttributePoints("气力", -QiCost);
		}
	}
	
	// 在战斗中使用技能
	public int UseInBattle(Character user, Character target)
	{
		if (Type != TechniqueType.Combat || target == null)
		{
			return 0;
		}
		
		// 计算基础伤害
		int damage = BaseDamage;
		
		// 应用使用者的攻击加成
		if (user != null)
		{
			damage += (int)(user.Attack * 0.5f);
		}
		
		// 应用技能效果
		target.TakeDamage(damage);
		return damage; // 返回造成的伤害值
	}

	// 计算元素伤害加成
	public int CalculateElementalDamage(Character user, Character target, int baseDamage)
	{
		float damageMultiplier = 1.0f;
		
		// 根据元素相克关系计算伤害倍率
		switch (ElementType)
		{
			case ElementType.Fire:
				if (target.ElementalWeakness == ElementType.Metal)
					damageMultiplier = 1.5f;
				else if (target.ElementalWeakness == ElementType.Water)
					damageMultiplier = 0.75f;
				break;
				
			case ElementType.Water:
				if (target.ElementalWeakness == ElementType.Fire)
					damageMultiplier = 1.5f;
				else if (target.ElementalWeakness == ElementType.Earth)
					damageMultiplier = 0.75f;
				break;
				
			case ElementType.Earth:
				if (target.ElementalWeakness == ElementType.Water)
					damageMultiplier = 1.5f;
				else if (target.ElementalWeakness == ElementType.Wind)
					damageMultiplier = 0.75f;
				break;
				
			case ElementType.Wind:
				if (target.ElementalWeakness == ElementType.Earth)
					damageMultiplier = 1.5f;
				else if (target.ElementalWeakness == ElementType.Fire)
					damageMultiplier = 0.75f;
				break;
				
			case ElementType.Metal:
				if (target.ElementalWeakness == ElementType.Wind)
					damageMultiplier = 1.5f;
				else if (target.ElementalWeakness == ElementType.Metal)
					damageMultiplier = 0.75f;
				break;
		}
		
		// 应用元素伤害加成
		int finalDamage = (int)(baseDamage * damageMultiplier);
		
		// 记录特殊效果
		if (damageMultiplier > 1.0f)
		{
			// 这里可以添加元素相克造成的特殊效果
		}
		
		return finalDamage;
	}
}

// 功法元素枚举
public enum TechniqueElement
{
	Neutral,      // 无属性
	Fire,         // 火属性
	Water,        // 水属性
	Wood,         // 木属性
	Metal,        // 金属性
	Earth,        // 土属性
	Lightning,    // 雷属性
	Wind,         // 风属性
	Ice,          // 冰属性
	Dark,         // 暗属性
	Light         // 光属性
} 
