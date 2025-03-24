using Godot;
using System;
using System.Collections.Generic;

public partial class BattleSystem : Node
{
    // 引用节点
    private GameManager _gameManager;
    
    // UI元素
    private Label _enemyName;
    private Label _enemyLevel;
    private ProgressBar _enemyHealthBar;
    private Label _enemyHealthLabel;
    private ProgressBar _enemyQiBar;
    private Label _enemyQiLabel;
    
    private Label _playerName;
    private ProgressBar _playerHealthBar;
    private Label _playerHealthLabel;
    private ProgressBar _playerQiBar;
    private Label _playerQiLabel;
    
    private RichTextLabel _logText;
    
    // 战斗按钮
    private Button _attackButton;
    private Button _defendButton;
    private Button _skillButton;
    private Button _itemButton;
    private Button _escapeButton;
    
    // 技能面板
    private Panel _skillPanel;
    private VBoxContainer _skillList;
    
    // 战斗数据
    private Character _player;
    private Character _enemy;
    private bool _isPlayerTurn = true;
    private bool _isBattleOver = false;
    
    // 技能选择
    private bool _isSelectingSkill = false;
    
    // 战斗常量
    private const int BASE_ATTACK_DAMAGE = 5;
    private const float DEFEND_DAMAGE_REDUCTION = 0.5f;
    private const int QI_COST_ATTACK = 5;
    private const int QI_RECOVER_DEFEND = 10;
    
    public override void _Ready()
    {
        // 获取GameManager引用
        _gameManager = GetNode<GameManager>("/root/GameManager");
        
        // 获取UI元素引用
        // 敌人UI
        _enemyName = GetNode<Label>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer/EnemyName");
        _enemyLevel = GetNode<Label>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer/EnemyLevel");
        _enemyHealthBar = GetNode<ProgressBar>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer2/EnemyHealthBar");
        _enemyHealthLabel = GetNode<Label>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer2/EnemyHealthLabel");
        _enemyQiBar = GetNode<ProgressBar>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer3/EnemyQiBar");
        _enemyQiLabel = GetNode<Label>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer3/EnemyQiLabel");
        
        // 玩家UI
        _playerName = GetNode<Label>("PlayerPanel/MarginContainer/VBoxContainer/StatsContainer/PlayerName");
        _playerHealthBar = GetNode<ProgressBar>("PlayerPanel/MarginContainer/VBoxContainer/StatsContainer/VBoxContainer/HBoxContainer/PlayerHealthBar");
        _playerHealthLabel = GetNode<Label>("PlayerPanel/MarginContainer/VBoxContainer/StatsContainer/VBoxContainer/HBoxContainer/PlayerHealthLabel");
        _playerQiBar = GetNode<ProgressBar>("PlayerPanel/MarginContainer/VBoxContainer/StatsContainer/VBoxContainer/HBoxContainer2/PlayerQiBar");
        _playerQiLabel = GetNode<Label>("PlayerPanel/MarginContainer/VBoxContainer/StatsContainer/VBoxContainer/HBoxContainer2/PlayerQiLabel");
        
        // 战斗日志
        _logText = GetNode<RichTextLabel>("BattleField/BattleLog/MarginContainer/LogText");
        
        // 战斗按钮
        _attackButton = GetNode<Button>("PlayerPanel/MarginContainer/VBoxContainer/ActionButtons/AttackButton");
        _defendButton = GetNode<Button>("PlayerPanel/MarginContainer/VBoxContainer/ActionButtons/DefendButton");
        _skillButton = GetNode<Button>("PlayerPanel/MarginContainer/VBoxContainer/ActionButtons/SkillButton");
        _itemButton = GetNode<Button>("PlayerPanel/MarginContainer/VBoxContainer/ActionButtons/ItemButton");
        _escapeButton = GetNode<Button>("PlayerPanel/MarginContainer/VBoxContainer/ActionButtons/EscapeButton");
        
        // 技能面板
        _skillPanel = GetNode<Panel>("SkillPanel");
        _skillList = GetNode<VBoxContainer>("SkillPanel/MarginContainer/VBoxContainer/SkillsList");
        
        // 绑定按钮事件
        _attackButton.Pressed += OnAttackButtonPressed;
        _defendButton.Pressed += OnDefendButtonPressed;
        _skillButton.Pressed += OnSkillButtonPressed;
        _itemButton.Pressed += OnItemButtonPressed;
        _escapeButton.Pressed += OnEscapeButtonPressed;
        GetNode<Button>("SkillPanel/MarginContainer/VBoxContainer/BackButton").Pressed += OnSkillBackButtonPressed;
        
        // 初始化战斗
        InitializeBattle();
    }
    
    private void InitializeBattle()
    {
        // 获取玩家数据
        _player = Character.FromPlayerData(_gameManager.PlayerData);
        
        // 创建敌人数据
        _enemy = GenerateRandomEnemy();
        
        // 更新UI
        UpdateEnemyUI();
        UpdatePlayerUI();
        
        // 清空战斗日志
        _logText.Clear();
        AddBattleLog($"遭遇了 {_enemy.Name}！", true);
        AddBattleLog("战斗开始！", true);
        
        // 隐藏技能面板
        _skillPanel.Visible = false;
        
        // 设置为玩家回合
        _isPlayerTurn = true;
        _isBattleOver = false;
        EnablePlayerActions(true);
    }
    
    private Character GenerateRandomEnemy()
    {
        // 这里可以根据游戏进度生成不同的敌人
        string[] enemyNames = { "妖狐", "山精", "恶鬼", "邪修", "毒蛇精" };
        int randomIndex = new Random().Next(0, enemyNames.Length);
        
        int enemyLevel = _player.Level + new Random().Next(-1, 2); // 敌人等级随机在玩家等级-1到+1之间
        if (enemyLevel < 1) enemyLevel = 1;
        
        // 根据等级计算属性
        int baseStats = 5 + enemyLevel * 2;
        
        Character enemy = new Character();
        enemy.Name = enemyNames[randomIndex];
        enemy.Level = enemyLevel;
        enemy.MaxHealth = baseStats * 10;
        enemy.CurrentHealth = enemy.MaxHealth;
        enemy.MaxQi = baseStats * 5;
        enemy.CurrentQi = enemy.MaxQi;
        enemy.Attack = baseStats;
        enemy.Defense = baseStats / 2;
        enemy.Spirit = baseStats / 2;
        
        return enemy;
    }
    
    private void UpdateEnemyUI()
    {
        _enemyName.Text = _enemy.Name;
        _enemyLevel.Text = $"等级：{_enemy.Level}";
        
        _enemyHealthBar.MaxValue = _enemy.MaxHealth;
        _enemyHealthBar.Value = _enemy.CurrentHealth;
        _enemyHealthLabel.Text = $"{_enemy.CurrentHealth}/{_enemy.MaxHealth}";
        
        _enemyQiBar.MaxValue = _enemy.MaxQi;
        _enemyQiBar.Value = _enemy.CurrentQi;
        _enemyQiLabel.Text = $"{_enemy.CurrentQi}/{_enemy.MaxQi}";
    }
    
    private void UpdatePlayerUI()
    {
        _playerName.Text = "修仙者";
        
        _playerHealthBar.MaxValue = _player.MaxHealth;
        _playerHealthBar.Value = _player.CurrentHealth;
        _playerHealthLabel.Text = $"{_player.CurrentHealth}/{_player.MaxHealth}";
        
        _playerQiBar.MaxValue = _player.MaxQi;
        _playerQiBar.Value = _player.CurrentQi;
        _playerQiLabel.Text = $"{_player.CurrentQi}/{_player.MaxQi}";
    }
    
    private void AddBattleLog(string message, bool important = false)
    {
        if (important)
        {
            _logText.AppendText($"[color=#E5B96A]{message}[/color]\n\n");
        }
        else
        {
            _logText.AppendText($"{message}\n\n");
        }
    }
    
    private void EnablePlayerActions(bool enable)
    {
        _attackButton.Disabled = !enable || _player.CurrentQi < QI_COST_ATTACK;
        _defendButton.Disabled = !enable;
        _skillButton.Disabled = !enable || _player.Skills.Count == 0;
        _itemButton.Disabled = !enable; // 这里可以根据是否有可用物品来设置
        _escapeButton.Disabled = !enable;
    }
    
    private void OnAttackButtonPressed()
    {
        if (!_isPlayerTurn || _isBattleOver) return;
        
        // 使用气力
        if (_player.CurrentQi < QI_COST_ATTACK)
        {
            AddBattleLog("气力不足，无法攻击！");
            return;
        }
        _player.CurrentQi -= QI_COST_ATTACK;
        
        // 计算伤害
        int damage = CalculateDamage(_player, _enemy, BASE_ATTACK_DAMAGE);
        
        // 造成伤害
        _enemy.TakeDamage(damage);
        
        // 更新UI
        UpdatePlayerUI();
        UpdateEnemyUI();
        
        // 添加战斗日志
        AddBattleLog($"你对 {_enemy.Name} 发起攻击，造成了 {damage} 点伤害！");
        
        // 检查战斗是否结束
        if (_enemy.CurrentHealth <= 0)
        {
            EndBattle(true);
            return;
        }
        
        // 切换到敌人回合
        _isPlayerTurn = false;
        EnablePlayerActions(false);
        
        // 敌人AI在短暂延时后行动
        GetTree().CreateTimer(1.0f).Timeout += EnemyTurn;
    }
    
    private void OnDefendButtonPressed()
    {
        if (!_isPlayerTurn || _isBattleOver) return;
        
        // 防御可以恢复一些气力
        _player.RecoverQi(QI_RECOVER_DEFEND);
        
        // 设置防御状态
        _player.IsDefending = true;
        
        // 更新UI
        UpdatePlayerUI();
        
        // 添加战斗日志
        AddBattleLog("你摆出防御姿态，恢复了一些气力。下回合受到的伤害将减少。");
        
        // 切换到敌人回合
        _isPlayerTurn = false;
        EnablePlayerActions(false);
        
        // 敌人AI在短暂延时后行动
        GetTree().CreateTimer(1.0f).Timeout += EnemyTurn;
    }
    
    private void OnSkillButtonPressed()
    {
        if (!_isPlayerTurn || _isBattleOver) return;
        
        // 显示技能选择面板
        ShowSkillPanel();
    }
    
    private void ShowSkillPanel()
    {
        _isSelectingSkill = true;
        
        // 清空技能列表
        foreach (Node child in _skillList.GetChildren())
        {
            child.QueueFree();
        }
        
        // 填充技能列表
        foreach (var skill in _player.Skills)
        {
            Button skillButton = new Button();
            skillButton.Text = $"{skill.Name} (气力: {skill.QiCost})";
            skillButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            skillButton.Disabled = _player.CurrentQi < skill.QiCost;
            
            // 样式设置
            skillButton.AddThemeColorOverride("font_color", new Color(0.84f, 0.76f, 0.61f));
            skillButton.AddThemeColorOverride("font_hover_color", new Color(0.99f, 0.92f, 0.80f));
            skillButton.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://Resources/Fonts/XiaoKeFont.ttf"));
            skillButton.AddThemeFontSizeOverride("font_size", 18);
            
            // 添加事件
            skillButton.Pressed += () => UseSkill(skill);
            
            _skillList.AddChild(skillButton);
        }
        
        // 如果没有技能，添加提示
        if (_player.Skills.Count == 0)
        {
            Label noSkillLabel = new Label();
            noSkillLabel.Text = "你还没有学会任何战斗技能！";
            noSkillLabel.AddThemeColorOverride("font_color", new Color(0.84f, 0.76f, 0.61f));
            noSkillLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _skillList.AddChild(noSkillLabel);
        }
        
        // 显示技能面板
        _skillPanel.Visible = true;
    }
    
    private void OnSkillBackButtonPressed()
    {
        // 隐藏技能面板
        _skillPanel.Visible = false;
        _isSelectingSkill = false;
    }
    
    private void UseSkill(Technique skill)
    {
        if (!_isPlayerTurn || _isBattleOver) return;
        
        // 隐藏技能面板
        _skillPanel.Visible = false;
        _isSelectingSkill = false;
        
        // 检查气力
        if (_player.CurrentQi < skill.QiCost)
        {
            AddBattleLog($"气力不足，无法使用 {skill.Name}！");
            return;
        }
        
        // 使用技能
        int damage = _player.UseSkill(skill, _enemy);
        
        // 元素相克显示
        string elementalEffectText = "";
        if (skill.ElementType != ElementType.None)
        {
            if (_enemy.ElementalWeakness == skill.ElementType)
            {
                elementalEffectText = $"[color=#FF5733]元素克制！{GetElementName(skill.ElementType)}属性克制{_enemy.Name}的{GetElementName(_enemy.ElementalStrength)}属性！伤害提升！[/color]";
                AddBattleLog(elementalEffectText, true);
            }
            else if (_enemy.ElementalStrength == skill.ElementType)
            {
                elementalEffectText = $"[color=#33FFC7]{_enemy.Name}的{GetElementName(_enemy.ElementalStrength)}属性抵抗了你的{GetElementName(skill.ElementType)}攻击！伤害减少！[/color]";
                AddBattleLog(elementalEffectText, true);
            }
        }
        
        // 更新UI
        UpdatePlayerUI();
        UpdateEnemyUI();
        
        // 添加战斗日志
        AddBattleLog($"你使用了「{skill.Name}」，对 {_enemy.Name} 造成了 {damage} 点伤害！");
        
        // 检查战斗是否结束
        if (_enemy.IsDead())
        {
            EndBattle(true);
            return;
        }
        
        // 切换到敌人回合
        _isPlayerTurn = false;
        EnablePlayerActions(false);
        
        // 敌人AI在短暂延时后行动
        GetTree().CreateTimer(1.0f).Timeout += EnemyTurn;
    }
    
    // 获取元素名称
    private string GetElementName(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return "火";
            case ElementType.Water:
                return "水";
            case ElementType.Earth:
                return "土";
            case ElementType.Wind:
                return "风";
            case ElementType.Metal:
                return "金";
            default:
                return "无";
        }
    }
    
    private void OnItemButtonPressed()
    {
        if (!_isPlayerTurn || _isBattleOver) return;
        
        // 简单实现：使用药品恢复生命值
        _player.Heal(20);
        UpdatePlayerUI();
        
        AddBattleLog("使用了一个药品，恢复了20点生命值！");
        
        // 切换到敌人回合
        _isPlayerTurn = false;
        EnablePlayerActions(false);
        
        // 敌人AI在短暂延时后行动
        GetTree().CreateTimer(1.0f).Timeout += EnemyTurn;
    }
    
    private void OnEscapeButtonPressed()
    {
        if (!_isPlayerTurn || _isBattleOver) return;
        
        // 逃跑有50%的成功率
        if (new Random().NextDouble() < 0.5)
        {
            AddBattleLog("成功逃离战斗！", true);
            GetTree().CreateTimer(1.5f).Timeout += () => GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
        }
        else
        {
            AddBattleLog("逃跑失败！");
            
            // 切换到敌人回合
            _isPlayerTurn = false;
            EnablePlayerActions(false);
            
            // 敌人AI在短暂延时后行动
            GetTree().CreateTimer(1.0f).Timeout += EnemyTurn;
        }
    }
    
    private void EnemyTurn()
    {
        if (_isBattleOver) return;
        
        // 增强敌人AI逻辑
        // 1. 低血量时有几率使用治疗
        // 2. 根据气力情况决定是普通攻击还是防御
        // 3. 随机使用特殊攻击
        
        Random random = new Random();
        
        // 如果敌人血量低于30%，有50%几率选择防御和恢复
        if (_enemy.CurrentHealth < _enemy.MaxHealth * 0.3 && random.NextDouble() < 0.5)
        {
            // 防御，恢复气力和少量生命
            _enemy.RecoverQi(QI_RECOVER_DEFEND);
            _enemy.Heal(5);
            _enemy.IsDefending = true;
            
            // 更新UI
            UpdateEnemyUI();
            
            // 添加战斗日志
            AddBattleLog($"{_enemy.Name} 采取防御姿态，恢复了一些气力和生命值。");
        }
        // 正常气力状态下有70%几率选择攻击
        else if (_enemy.CurrentQi >= QI_COST_ATTACK && random.NextDouble() < 0.7)
        {
            // 攻击
            _enemy.CurrentQi -= QI_COST_ATTACK;
            
            // 计算伤害
            int damage = CalculateDamage(_enemy, _player, BASE_ATTACK_DAMAGE);
            
            // 如果玩家在防御状态，伤害减少
            if (_player.IsDefending)
            {
                damage = (int)(damage * DEFEND_DAMAGE_REDUCTION);
                AddBattleLog("你的防御减少了伤害！");
            }
            
            // 造成伤害
            _player.TakeDamage(damage);
            
            // 更新UI
            UpdateEnemyUI();
            UpdatePlayerUI();
            
            // 添加战斗日志
            AddBattleLog($"{_enemy.Name} 对你发起攻击，造成了 {damage} 点伤害！");
            
            // 检查战斗是否结束
            if (_player.IsDead())
            {
                EndBattle(false);
                return;
            }
        }
        // 气力不足或者随机选择防御
        else
        {
            // 防御，恢复气力
            _enemy.RecoverQi(QI_RECOVER_DEFEND);
            _enemy.IsDefending = true;
            
            // 更新UI
            UpdateEnemyUI();
            
            // 添加战斗日志
            AddBattleLog($"{_enemy.Name} 正在蓄力，恢复了一些气力。");
        }
        
        // 重置玩家防御状态
        _player.IsDefending = false;
        
        // 切换到玩家回合
        _isPlayerTurn = true;
        EnablePlayerActions(true);
    }
    
    private int CalculateDamage(Character attacker, Character defender, int baseDamage)
    {
        // 基础伤害 + 攻击者攻击力 - 防御者防御力
        int damage = baseDamage + attacker.Attack - defender.Defense / 2;
        
        // 随机浮动20%
        float randomFactor = (float)new Random().NextDouble() * 0.4f + 0.8f; // 0.8-1.2范围的随机数
        damage = (int)(damage * randomFactor);
        
        // 确保最低伤害为1
        if (damage < 1) damage = 1;
        
        return damage;
    }
    
    private void EndBattle(bool victory)
    {
        _isBattleOver = true;
        EnablePlayerActions(false);
        
        if (victory)
        {
            // 玩家胜利
            AddBattleLog($"战斗胜利！击败了 {_enemy.Name}！", true);
            
            // 计算奖励
            int expReward = _enemy.Level * 20 + new Random().Next(5, 15);
            
            // 添加经验
            _gameManager.PlayerData.AddExperience(expReward);
            
            AddBattleLog($"获得了 {expReward} 点经验值！");
            
            // 属性点奖励
            Random random = new Random();
            if (random.NextDouble() < 0.3) // 30%几率获得额外属性点
            {
                int statPoints = random.Next(1, 3);
                // 随机选择加强的属性
                string[] stats = { "气力", "体魄", "神识" };
                string statName = stats[random.Next(0, stats.Length)];
                
                _gameManager.PlayerData.AddAttributePoints(statName, statPoints);
                AddBattleLog($"你的{statName}提升了{statPoints}点！");
            }
            
            // 掉落物品
            int itemChance = random.Next(0, 100);
            if (itemChance < 40) // 40%几率掉落物品
            {
                string[] items = { "灵草", "培元丹", "妖兽内丹", "破损的护符", "修士手札", "灵石" };
                string droppedItem = items[random.Next(0, items.Length)];
                
                // 这里应该添加物品到玩家背包
                AddBattleLog($"获得了物品: {droppedItem}！");
            }
            
            // 极低几率学习新技能
            if (random.NextDouble() < 0.05) // 5%几率获得新技能
            {
                string[] techniqueNames = { "飞剑术", "五行遁法", "元磁神光", "碧海潮生诀", "玄冰真气" };
                string newTechnique = techniqueNames[random.Next(0, techniqueNames.Length)];
                
                AddBattleLog($"你从战斗中领悟了新技能：{newTechnique}！", true);
                // 这里应该添加技能到玩家的已学技能列表
            }
            
            // 战斗后修炼效率提升
            _gameManager.PlayerData.SetTemporaryBonus("修炼效率", 0.2f, 600); // 提升20%的修炼效率，持续10分钟
            AddBattleLog("战斗激发了你的潜能，短时间内修炼效率提升20%！", true);
            
            // 2秒后返回主界面
            GetTree().CreateTimer(2.0f).Timeout += () => GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
        }
        else
        {
            // 玩家失败
            AddBattleLog("你被击败了...", true);
            AddBattleLog("修为受损，损失了一些经验值。", true);
            
            // 失败惩罚 - 损失10%的经验
            int expLoss = (int)(_gameManager.PlayerData.Experience * 0.1f);
            if (expLoss > 0)
            {
                _gameManager.PlayerData.AddExperience(-expLoss);
                AddBattleLog($"损失了 {expLoss} 点经验值。");
            }
            
            // 2秒后返回主界面
            GetTree().CreateTimer(2.0f).Timeout += () => GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
        }
    }
} 