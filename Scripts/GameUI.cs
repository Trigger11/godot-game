using Godot;
using System;

public partial class GameUI : Control
{
	// 控件引用
	private Label _playerNameLabel;
	private Label _realmLabel;
	private Label _levelLabel;
	private Label _qiPowerLabel;
	private Label _spiritLabel;
	private Label _bodyLabel;
	private Label _fateLabel;
	private RichTextLabel _storyText;
	
	// 按钮引用
	private Button _cultivateButton;
	private Button _battleButton;
	private Button _divinationButton;
	private Button _inventoryButton;
	private Button _exploreButton;
	
	// GameManager引用
	private GameManager _gameManager;
	
	public override void _Ready()
	{
		// 获取GameManager引用 - 使用单例
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
		
		// 获取标签引用
		_playerNameLabel = GetNode<Label>("MainContent/TopPanel/HBoxContainer/PlayerName");
		_realmLabel = GetNode<Label>("MainContent/TopPanel/HBoxContainer/RealmLabel");
		_levelLabel = GetNode<Label>("MainContent/TopPanel/HBoxContainer/LevelLabel");
		
		_qiPowerLabel = GetNode<Label>("MainContent/LeftPanel/VBoxContainer/QiPowerLabel");
		_spiritLabel = GetNode<Label>("MainContent/LeftPanel/VBoxContainer/SpiritLabel");
		_bodyLabel = GetNode<Label>("MainContent/LeftPanel/VBoxContainer/BodyLabel");
		_fateLabel = GetNode<Label>("MainContent/LeftPanel/VBoxContainer/FateLabel");
		
		_storyText = GetNode<RichTextLabel>("MainContent/ContentPanel/MarginContainer/StoryText");
		
		// 获取按钮引用
		_cultivateButton = GetNode<Button>("MainContent/ActionPanel/HBoxContainer/CultivateButton");
		_battleButton = GetNode<Button>("MainContent/ActionPanel/HBoxContainer/BattleButton");
		_divinationButton = GetNode<Button>("MainContent/ActionPanel/HBoxContainer/DivinationButton");
		_inventoryButton = GetNode<Button>("MainContent/ActionPanel/HBoxContainer/InventoryButton");
		_exploreButton = GetNode<Button>("MainContent/ActionPanel/HBoxContainer/ExploreButton");
		
		// 连接按钮信号
		_cultivateButton.Pressed += OnCultivateButtonPressed;
		_battleButton.Pressed += OnBattleButtonPressed;
		_divinationButton.Pressed += OnDivinationButtonPressed;
		_inventoryButton.Pressed += OnInventoryButtonPressed;
		_exploreButton.Pressed += OnExploreButtonPressed;
		
		// 更新界面
		UpdateUI();
	}
	
	public override void _Process(double delta)
	{
		// 处理游戏逻辑
		UpdateUI();
	}
	
	private void UpdateUI()
	{
		// 确保GameManager存在
		if (_gameManager == null || _gameManager.PlayerData == null)
			return;
			
		var playerData = _gameManager.PlayerData;
		
		// 更新基本信息
		_playerNameLabel.Text = playerData.PlayerName;
		_realmLabel.Text = $"境界：{playerData.Realm}";
		_levelLabel.Text = $"等级：{playerData.Level}";
		
		// 更新属性
		_qiPowerLabel.Text = playerData.GetAttribute("气力").ToString();
		_spiritLabel.Text = playerData.GetAttribute("神识").ToString();
		_bodyLabel.Text = playerData.GetAttribute("体魄").ToString();
		_fateLabel.Text = playerData.GetAttribute("命运").ToString();
		
		// 更新已学功法列表
		UpdateTechniquesList(playerData);
	}
	
	private void UpdateTechniquesList(PlayerData playerData)
	{
		var techniquesList = GetNode<VBoxContainer>("MainContent/LeftPanel/VBoxContainer/TechniqueList");
		
		// 清空现有列表
		foreach (Node child in techniquesList.GetChildren())
		{
			child.QueueFree();
		}
		
		// 添加已学功法
		foreach (string techniqueName in playerData.GetLearnedTechniques())
		{
			var label = new Label();
			label.Text = techniqueName;
			label.AddThemeColorOverride("font_color", new Color(0.839f, 0.73f, 0.549f));
			
			techniquesList.AddChild(label);
		}
	}
	
	private void OnCultivateButtonPressed()
	{
		// 使用GameManager的方法切换到修炼界面
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
		// 使用GameManager的方法切换到战斗界面
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
		// 使用GameManager的方法切换到占卜界面
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
		// 使用GameManager的方法切换到物品界面
		if (_gameManager != null)
		{
			_gameManager.NavigateToScene("Inventory");
		}
		else
		{
			GD.PrintErr("GameManager不可用，无法切换到物品界面");
		}
	}
	
	private void OnExploreButtonPressed()
	{
		// 切换到探索界面
		_gameManager.ShowExplorationMap();
	}
} 
