using Godot;
using System;

public partial class MainMenu : Control
{
	// 节点引用
	private Button _startButton;
	private Button _loadGameButton;
	private Button _optionsButton;
	private Button _exitButton;
	
	// 加载时调用
	public override void _Ready()
	{
		// 获取按钮引用
		_startButton = GetNode<Button>("%StartButton");
		_loadGameButton = GetNode<Button>("%LoadGameButton");
		_optionsButton = GetNode<Button>("%OptionsButton");
		_exitButton = GetNode<Button>("%ExitButton");
		
		// 连接信号
		_startButton.Pressed += OnStartButtonPressed;
		_loadGameButton.Pressed += OnLoadGameButtonPressed;
		_optionsButton.Pressed += OnOptionsButtonPressed;
		_exitButton.Pressed += OnExitButtonPressed;
	}
	
	// 开始游戏按钮处理
	private void OnStartButtonPressed()
	{
		// 切换到角色创建场景
		GetTree().ChangeSceneToFile("res://Scenes/CharacterCreation.tscn");
	}
	
	// 加载游戏按钮处理
	private void OnLoadGameButtonPressed()
	{
		// 切换到加载游戏场景
		GetTree().ChangeSceneToFile("res://Scenes/LoadGameUI.tscn");
	}
	
	// 选项按钮处理
	private void OnOptionsButtonPressed()
	{
		// 显示选项菜单
		// 这里可以是弹出菜单或者切换到选项场景
		GetTree().ChangeSceneToFile("res://Scenes/Options.tscn");
	}
	
	// 退出按钮处理
	private void OnExitButtonPressed()
	{
		// 退出游戏
		GetTree().Quit();
	}
} 
