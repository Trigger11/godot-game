using Godot;
using System;

public partial class CharacterCreationUI : Control
{
	// 节点引用
	private LineEdit _nameInput;
	private OptionButton _genderOption;
	private Button _confirmButton;
	private Button _backButton;
	
	// 加载时调用
	public override void _Ready()
	{
		// 获取UI元素引用
		_nameInput = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/NameContainer/NameInput");
		_genderOption = GetNode<OptionButton>("Panel/MarginContainer/VBoxContainer/GenderContainer/GenderOption");
		_confirmButton = GetNode<Button>("Panel/MarginContainer/VBoxContainer/ConfirmButton");
		_backButton = GetNode<Button>("Panel/MarginContainer/VBoxContainer/BackButton");
		
		// 设置默认角色名称
		_nameInput.Text = "修仙者";
		
		// 初始化性别选项
		_genderOption.AddItem("男");
		_genderOption.AddItem("女");
		_genderOption.Selected = 0;
		
		// 连接信号
		_confirmButton.Pressed += OnConfirmButtonPressed;
		_backButton.Pressed += OnBackButtonPressed;
		_nameInput.TextChanged += OnNameInputChanged;
		
		// 初始检查名称输入
		_confirmButton.Disabled = string.IsNullOrWhiteSpace(_nameInput.Text);
	}
	
	// 处理名称输入变化
	private void OnNameInputChanged(string newText)
	{
		// 名称不能为空
		_confirmButton.Disabled = string.IsNullOrWhiteSpace(newText);
	}
	
	// 处理确认按钮点击
	private void OnConfirmButtonPressed()
	{
		string characterName = _nameInput.Text.Trim();
		int genderIndex = _genderOption.Selected;
		
		// 验证名称不为空
		if (string.IsNullOrEmpty(characterName))
		{
			GD.Print("角色名称不能为空");
			return;
		}
		
		// 创建新角色
		CreateNewCharacter(characterName, genderIndex);
		
		// 切换到游戏主场景
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}
	
	// 创建新角色
	private void CreateNewCharacter(string name, int genderIndex)
	{
		GD.Print($"创建新角色: {name}, 性别: {(genderIndex == 0 ? "男" : "女")}");
		
		// 这里需要调用GameManager或类似的单例来处理角色创建和数据保存
		// GameManager.Instance.CreateNewCharacter(name, genderIndex);
		
		// 临时代码，仅用于演示
		// 创建角色数据并保存到全局状态
	}
	
	// 处理返回按钮点击
	private void OnBackButtonPressed()
	{
		// 返回主菜单
		GetTree().ChangeSceneToFile("res://UI/MainMenuUI.tscn");
	}
} 
