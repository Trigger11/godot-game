using Godot;
using System;

public partial class PlayerCharacter : Character
{
    // 经验值系统
    public int Experience { get; private set; } = 0;
    public int ExperienceToNextLevel { get; private set; } = 100;
    
    // 等级提升映射表
    private readonly int[] _experienceThresholds = {
        0,    // 等级1需要0经验值
        100,  // 等级2需要100经验值
        300,  // 等级3需要300经验值
        600,  // 等级4需要600经验值
        1000, // 等级5需要1000经验值
        1500, // 等级6需要1500经验值
        2200, // 等级7需要2200经验值
        3000, // 等级8需要3000经验值
        4000, // 等级9需要4000经验值
        5500  // 等级10需要5500经验值
    };
    
    // 构造函数
    public PlayerCharacter() : base()
    {
        IsPlayer = true;
        UpdateExperienceThreshold();
    }
    
    // 从玩家数据创建角色
    public static new PlayerCharacter FromPlayerData(PlayerData playerData)
    {
        PlayerCharacter character = new PlayerCharacter();
        character.Name = playerData.PlayerName;
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
    
    // 添加经验值
    public void AddExperience(int expAmount)
    {
        Experience += expAmount;
        GD.Print($"获得经验值 {expAmount}，当前经验值：{Experience}，升级所需：{ExperienceToNextLevel}");
        
        // 检查是否可以升级
        CheckLevelUp();
    }
    
    // 检查是否可以升级
    private void CheckLevelUp()
    {
        if (Level >= _experienceThresholds.Length)
            return; // 已到达最高等级
            
        if (Experience >= ExperienceToNextLevel)
        {
            // 升级
            Level++;
            GD.Print($"等级提升至 {Level} 级！");
            
            // 更新属性
            MaxHealth += 5;
            CurrentHealth = MaxHealth; // 升级时恢复全部生命值
            MaxQi += 3;
            CurrentQi = MaxQi; // 升级时恢复全部气力值
            Attack += 2;
            Defense += 1;
            Spirit += 1;
            
            // 更新下一级所需经验值
            UpdateExperienceThreshold();
            
            // 如果经验值仍然足够，继续检查升级
            CheckLevelUp();
        }
    }
    
    // 更新升级所需经验值
    private void UpdateExperienceThreshold()
    {
        if (Level < _experienceThresholds.Length)
        {
            ExperienceToNextLevel = _experienceThresholds[Level];
        }
        else
        {
            // 已到达最高等级，设置一个无法达到的值
            ExperienceToNextLevel = int.MaxValue;
        }
    }
} 