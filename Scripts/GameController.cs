using Godot;
using System;

public partial class GameController : Node
{
    // 引用GameManager
    private GameManager _gameManager;
    
    // UI元素
    private Label _playerNameLabel;
    private Label _realmLabel;
    private Label _levelLabel;
    private Label _qiPowerLabel;
    private Label _spiritLabel;
    private Label _bodyLabel;
    private Label _fateLabel;
    private VBoxContainer _techniqueList;
    
    // 按钮
    private Button _cultivateButton;
    private Button _battleButton;
    private Button _divinationButton;
    private Button _inventoryButton;
    
    public override void _Ready()
    {
        // 获取GameManager引用 - 使用单例模式
        _gameManager = GameManager.Instance;
        
        // 如果GameManager实例不存在，通过节点获取
        if (_gameManager == null)
        {
            GD.PushWarning("GameManager单例不存在，尝试通过节点获取");
            _gameManager = GetNode<GameManager>("/root/GameManager");
        }
        
        // 如果仍然为空，输出错误
        if (_gameManager == null)
        {
            GD.PrintErr("无法获取GameManager实例，请确保已设置为自动加载");
            return;
        }
        
        // 初始化UI引用
        _playerNameLabel = GetNode<Label>("../MainContent/TopPanel/HBoxContainer/PlayerName");
        _realmLabel = GetNode<Label>("../MainContent/TopPanel/HBoxContainer/RealmLabel");
        _levelLabel = GetNode<Label>("../MainContent/TopPanel/HBoxContainer/LevelLabel");
        
        _qiPowerLabel = GetNode<Label>("../MainContent/LeftPanel/VBoxContainer/QiPowerLabel");
        _spiritLabel = GetNode<Label>("../MainContent/LeftPanel/VBoxContainer/SpiritLabel");
        _bodyLabel = GetNode<Label>("../MainContent/LeftPanel/VBoxContainer/BodyLabel");
        _fateLabel = GetNode<Label>("../MainContent/LeftPanel/VBoxContainer/FateLabel");
        
        _techniqueList = GetNode<VBoxContainer>("../MainContent/LeftPanel/VBoxContainer/TechniqueList");
        
        // 获取按钮引用
        _cultivateButton = GetNode<Button>("../MainContent/ActionPanel/HBoxContainer/CultivateButton");
        _battleButton = GetNode<Button>("../MainContent/ActionPanel/HBoxContainer/BattleButton");
        _divinationButton = GetNode<Button>("../MainContent/ActionPanel/HBoxContainer/DivinationButton");
        _inventoryButton = GetNode<Button>("../MainContent/ActionPanel/HBoxContainer/InventoryButton");
        
        // 绑定按钮事件
        _cultivateButton.Pressed += OnCultivateButtonPressed;
        _battleButton.Pressed += OnBattleButtonPressed;
        _divinationButton.Pressed += OnDivinationButtonPressed;
        _inventoryButton.Pressed += OnInventoryButtonPressed;
        
        // 更新UI显示
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        var player = _gameManager.PlayerData;
        
        // 更新基本信息
        _playerNameLabel.Text = player.PlayerName;
        _realmLabel.Text = $"境界：{player.Realm}";
        _levelLabel.Text = $"等级：{player.Level}";
        
        // 更新属性
        _qiPowerLabel.Text = $"气力：{player.GetAttribute("气力")}";
        _spiritLabel.Text = $"神识：{player.GetAttribute("神识")}";
        _bodyLabel.Text = $"体魄：{player.GetAttribute("体魄")}";
        _fateLabel.Text = $"命运：{player.GetAttribute("命运")}";
        
        // 更新功法列表
        UpdateTechniqueList();
    }
    
    private void UpdateTechniqueList()
    {
        // 清空当前列表
        foreach (Node child in _techniqueList.GetChildren())
        {
            child.QueueFree();
        }
        
        // 添加功法
        foreach (var techniqueName in _gameManager.PlayerData.GetLearnedTechniques())
        {
            var technique = _gameManager.PlayerData.GetTechnique(techniqueName);
            if (technique != null) 
            {
                var label = new Label();
                label.Text = technique.Name;
                label.AddThemeColorOverride("font_color", new Color(0.87f, 0.71f, 0.42f));
                label.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://Resources/Fonts/XiaoKeFont.ttf"));
                label.AddThemeFontSizeOverride("font_size", 16);
                
                _techniqueList.AddChild(label);
            }
        }
    }
    
    private void OnCultivateButtonPressed()
    {
        // 打开修炼界面 - 使用GameManager安全方法
        if (_gameManager != null)
        {
            _gameManager.OpenCultivationScene();
        }
        else
        {
            GD.PrintErr("GameManager不可用，无法切换到修炼界面");
        }
    }
    
    private void OnBattleButtonPressed()
    {
        // 切换到战斗场景 - 使用GameManager安全方法
        if (_gameManager != null)
        {
            _gameManager.NavigateToScene("Battle");
        }
        else
        {
            GD.PrintErr("GameManager不可用，无法切换到战斗界面");
        }
    }
    
    private void OnDivinationButtonPressed()
    {
        // 切换到占卜场景 - 使用GameManager安全方法
        if (_gameManager != null)
        {
            _gameManager.NavigateToScene("Divination");
        }
        else
        {
            GD.PrintErr("GameManager不可用，无法切换到占卜界面");
        }
    }
    
    private void OnInventoryButtonPressed()
    {
        // 切换到背包场景 - 使用GameManager安全方法
        if (_gameManager != null)
        {
            _gameManager.NavigateToScene("Inventory");
        }
        else
        {
            GD.PrintErr("GameManager不可用，无法切换到背包界面");
        }
    }
} 