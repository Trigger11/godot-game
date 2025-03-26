using Godot;
using System;

public partial class VictoryScreen : Control
{
    private PlayerCharacter _playerCharacter;
    private Label _expValue;
    private Label _gainedLevel;
    private VBoxContainer _itemsList;
    private Button _continueButton;
    private int _baseExpReward = 50;
    private int _levelMultiplier = 10;
    private int _finalExpReward;
    private int _enemyLevel;

    [Signal]
    public delegate void BattleCompletedEventHandler();

    public override void _Ready()
    {
        _expValue = GetNode<Label>("CenterContainer/Panel/VBoxContainer/RewardsPanel/MarginContainer/VBoxContainer/ExpContainer/ExpValue");
        _gainedLevel = GetNode<Label>("CenterContainer/Panel/VBoxContainer/RewardsPanel/MarginContainer/VBoxContainer/GainedLevel");
        _itemsList = GetNode<VBoxContainer>("CenterContainer/Panel/VBoxContainer/RewardsPanel/MarginContainer/VBoxContainer/ItemsContainer/ItemsList");
        _continueButton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/ContinueButton");

        _continueButton.Pressed += OnContinueButtonPressed;
    }

    public void Initialize(PlayerCharacter player, int enemyLevel)
    {
        _playerCharacter = player;
        _enemyLevel = enemyLevel;
        CalculateRewards();
        DisplayRewards();
    }

    private void CalculateRewards()
    {
        // 计算经验值奖励（基础经验值 + 敌人等级 * 等级乘数）
        _finalExpReward = _baseExpReward + _enemyLevel * _levelMultiplier;
        
        // 生成随机物品
        // 这里可以根据敌人等级和随机性生成不同的物品
    }

    private void DisplayRewards()
    {
        // 显示经验值奖励
        _expValue.Text = _finalExpReward.ToString();
        
        // 检查玩家等级是否提升
        int oldLevel = _playerCharacter.Level;
        _playerCharacter.AddExperience(_finalExpReward);
        
        if (_playerCharacter.Level > oldLevel)
        {
            _gainedLevel.Visible = true;
            _gainedLevel.Text = $"突破境界！等级提升至 {_playerCharacter.Level} 级";
        }
        else
        {
            _gainedLevel.Visible = false;
        }
        
        // 显示物品奖励
        // 清空现有物品列表
        foreach (Node child in _itemsList.GetChildren())
        {
            if (child.Name != "ItemLabel") // 保留模板标签
            {
                child.QueueFree();
            }
        }
        
        // 获取第一个子节点作为模板
        Label itemTemplate = _itemsList.GetChild<Label>(0);
        
        // 根据敌人等级生成随机物品
        int itemCount = 1 + (_enemyLevel / 5); // 每5级额外增加一个物品
        
        for (int i = 0; i < itemCount; i++)
        {
            if (i == 0)
            {
                // 使用已有模板
                itemTemplate.Text = GenerateRandomItem();
            }
            else
            {
                // 创建新的物品标签
                Label newItem = new Label();
                newItem.Theme = itemTemplate.Theme;
                newItem.AddThemeColorOverride("font_color", itemTemplate.GetThemeColor("font_color"));
                newItem.AddThemeFontOverride("font", itemTemplate.GetThemeFont("font"));
                newItem.AddThemeFontSizeOverride("font_size", itemTemplate.GetThemeFontSize("font_size"));
                newItem.HorizontalAlignment = HorizontalAlignment.Right;
                newItem.Text = GenerateRandomItem();
                
                _itemsList.AddChild(newItem);
            }
        }
    }
    
    private string GenerateRandomItem()
    {
        string[] itemTypes = { "低级灵石", "中级灵石", "灵草", "丹药", "符箓" };
        string[] rarityPrefix = { "", "优质", "精致", "珍稀", "极品" };
        
        int itemIndex = new Random().Next(0, itemTypes.Length);
        string itemName = itemTypes[itemIndex];
        
        // 根据敌人等级可能增加物品稀有度
        if (_enemyLevel >= 5 && new Random().Next(0, 100) < _enemyLevel * 5)
        {
            int rarityIndex = Math.Min(new Random().Next(1, rarityPrefix.Length), _enemyLevel / 4);
            itemName = rarityPrefix[rarityIndex] + itemName;
        }
        
        int quantity = 1;
        if (itemName == "低级灵石" || itemName == "灵草")
        {
            quantity = new Random().Next(1, 3 + _enemyLevel / 2);
        }
        
        return $"{itemName} x{quantity}";
    }
    
    private void OnContinueButtonPressed()
    {
        EmitSignal(SignalName.BattleCompleted);
        QueueFree();
    }
} 