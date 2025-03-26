using Godot;
using System;
using System.Collections.Generic;

// 游戏管理器，使用单例模式
public partial class GameManager : Node
{
	// 单例实例 - 使用属性而不是字段+属性组合
	private static GameManager _instance;
	public static GameManager Instance
	{
		get
		{
			// 如果实例为空，尝试从场景树获取
			if (_instance == null)
			{
				// 尝试从根节点获取
				if (Engine.GetMainLoop() is SceneTree tree && tree.Root != null)
				{
					_instance = tree.Root.GetNodeOrNull<GameManager>("GameManager");
				}
				
				if (_instance == null)
				{
					GD.PrintErr("GameManager单例未找到，请确保场景中包含GameManager节点");
				}
			}
			return _instance;
		}
	}
	
	// 音频播放器
	private AudioStreamPlayer _musicPlayer;
	private AudioStreamPlayer _sfxPlayer;
	
	// 音量设置
	private float _musicVolume = 0.8f;
	private float _sfxVolume = 1.0f;
	private bool _musicEnabled = true;
	private bool _sfxEnabled = true;
	
	// 当前播放的背景音乐
	private string _currentMusic = "";
	
	// 游戏数据
	public PlayerData PlayerData { get; private set; }
	
	// 游戏状态
	public GameState CurrentGameState { get; private set; } = GameState.MainMenu;
	
	// 资源管理
	private bool _resourcesReady = false;
	
	// 事件系统
	[Signal]
	public delegate void GameStateChangedEventHandler(GameState newState);
	
	[Signal]
	public delegate void PlayerDataUpdatedEventHandler();
	
	[Signal]
	public delegate void BattleStartedEventHandler(Character enemy);
	
	[Signal]
	public delegate void BattleEndedEventHandler(bool playerWon);
	
	[Signal]
	public delegate void ResourceLoadErrorEventHandler(string error);
	
	// 战斗数据
	private string _currentEnemyType;
	private string _currentBattleLocation;
	
	// 场景路径
	private const string MainMenuScene = "res://UI/MainMenuUI.tscn";
	private const string GameScene = "res://Scenes/Game.tscn";
	private const string BattleScene = "res://Scenes/Battle.tscn";
	private const string InventoryScene = "res://Scenes/Inventory.tscn";
	
	// 初始化
	public override void _Ready()
	{
		GD.Print("GameManager初始化中...");
		
		// 使GameManager在场景切换时不会被销毁
		ProcessMode = ProcessModeEnum.Always;
		
		// 设置为持久节点
		MakePersistent(this);
		
		// 初始化音频播放器
		_musicPlayer = new AudioStreamPlayer();
		_musicPlayer.VolumeDb = LinearToDb(_musicVolume);
		_musicPlayer.Bus = "Music";
		AddChild(_musicPlayer);
		
		_sfxPlayer = new AudioStreamPlayer();
		_sfxPlayer.VolumeDb = LinearToDb(_sfxVolume);
		_sfxPlayer.Bus = "SFX";
		AddChild(_sfxPlayer);
		
		// 检查必要资源
		CheckResources();
		
		// 初始化游戏数据
		PlayerData = new PlayerData("修仙者");
		
		// 初始化技能列表
		InitTechniques();
		
		// 初始化物品列表
		InitItems();
		
		// 初始化卡牌牌组
		PlayerData.InitializeStarterDeck();
		
		// 播放默认背景音乐
		PlayBackgroundMusic("res://Resources/Audio/Music/main_theme.ogg");
		
		GD.Print("GameManager初始化完成");
	}
	
	// 确保GameManager不会在场景切换时被销毁
	public override void _EnterTree()
	{
		base._EnterTree();
		
		// 使节点在加载新场景时不被移除
		GetTree().SetAutoAcceptQuit(false);
	}
	
	// 处理应用程序退出
	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			// 在这里可以处理游戏退出前的保存操作
			GetTree().Quit();
		}
	}
	
	// 检查必要资源
	private void CheckResources()
	{
		// 检查字体文件
		if (!FileAccess.FileExists("res://Resources/Fonts/XiaoKeFont.ttf"))
		{
			EmitSignal(SignalName.ResourceLoadError, "缺少必要的字体文件：res://Resources/Fonts/XiaoKeFont.ttf");
			_resourcesReady = false;
		}
		else
		{
			_resourcesReady = true;
		}
	}
	
	// 切换游戏状态
	public void ChangeGameState(GameState newState)
	{
		// 如果资源未就绪且不是在主菜单，显示警告
		if (!_resourcesReady && newState != GameState.MainMenu)
		{
			GD.Print("警告：资源未完全加载，游戏可能无法正常运行。");
		}
		
		CurrentGameState = newState;
		EmitSignal(SignalName.GameStateChanged, (int)newState);
	}
	
	// 初始化新游戏
	public void StartNewGame()
	{
		// 创建新的玩家数据
		PlayerData = new PlayerData("修仙者");
		
		// 初始化技能和道具
		InitTechniques();
		InitItems();
		
		// 初始化卡牌
		PlayerData.InitializeStarterDeck();
		
		// 跳转到游戏主场景
		NavigateToScene("Game");
	}
	
	// 保存游戏
	public void SaveGame(string saveName = "save1")
	{
		// TODO: 实现保存逻辑
		GD.Print("保存游戏: " + saveName);
	}
	
	// 加载游戏
	public void LoadGame(string saveName = "save1")
	{
		// TODO: 实现加载逻辑
		GD.Print("加载游戏: " + saveName);
	}
	
	// 开始战斗
	public void StartBattle(Character enemy)
	{
		ChangeGameState(GameState.Battle);
		EmitSignal(SignalName.BattleStarted, enemy);
	}
	
	// 结束战斗
	public void EndBattle(bool playerWon)
	{
		ChangeGameState(GameState.Story);
		EmitSignal(SignalName.BattleEnded, playerWon);
		
		if (playerWon)
		{
			// 获得战斗奖励
			GD.Print("玩家获胜，获得奖励");
		}
		else
		{
			// 处理玩家失败后果
			GD.Print("玩家失败");
		}
	}
	
	// 获取资源路径，如果资源不存在则使用默认资源
	public string GetResourcePath(string path, string defaultPath = "")
	{
		if (FileAccess.FileExists(path))
		{
			return path;
		}
		else
		{
			GD.PushWarning($"资源文件不存在: {path}，使用默认资源");
			return defaultPath;
		}
	}
	
	// 获取SceneTree的安全方法
	private SceneTree GetSceneTreeSafe()
	{
		try
		{
			return GetTree();
		}
		catch (Exception ex)
		{
			GD.PrintErr("获取SceneTree失败: " + ex.Message);
			return null;
		}
	}
	
	// 扩展功能：导航到不同场景
	public void NavigateToScene(string sceneName)
	{
		try
		{
			// 播放UI点击音效
			if (_sfxEnabled)
			{
				PlaySoundEffect("res://Resources/Audio/SFX/ui_click.wav");
			}
			
			string targetScene = "";
			
			switch (sceneName)
			{
				case "MainMenu":
					targetScene = MainMenuScene;
					PlayBackgroundMusic("res://Resources/Audio/Music/main_theme.ogg");
					break;
				case "Game":
					targetScene = GameScene;
					PlayBackgroundMusic("res://Resources/Audio/Music/peaceful_theme.mp3");
					break;
				case "Battle":
					targetScene = BattleScene;
					PlayBackgroundMusic("res://Resources/Audio/Music/battle_theme.mp3");
					break;
				case "Inventory":
					targetScene = InventoryScene;
					PlayBackgroundMusic("res://Resources/Audio/Music/peaceful_theme.mp3");
					break;
				case "Options":
					targetScene = "res://Scenes/Options.tscn";
					// 保持当前音乐，不切换
					break;
				default:
					GD.Print("未知场景名称：" + sceneName);
					return;
			}
			
			var tree = GetSceneTreeSafe();
			if (tree != null)
			{
				tree.ChangeSceneToFile(targetScene);
			}
			else
			{
				GD.PrintErr("无法获取SceneTree，场景切换失败");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("场景导航时发生错误: " + ex.Message);
		}
	}
	
	// 在GameManager类中添加打开修炼界面的方法
	public void OpenCultivationScene()
	{
		try
		{
			// 播放UI点击音效
			if (_sfxEnabled)
			{
				PlaySoundEffect("res://Resources/Audio/SFX/ui_click.wav");
			}
			
			// 播放修炼场景音乐
			PlayBackgroundMusic("res://Resources/Audio/Music/cultivation_theme.mp3");
			
			var tree = GetSceneTreeSafe();
			if (tree != null)
			{
				tree.ChangeSceneToFile("res://Scenes/Cultivation.tscn");
			}
			else
			{
				GD.PrintErr("无法获取SceneTree，无法打开修炼界面");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("打开修炼界面时发生错误: " + ex.Message);
		}
	}
	
	// 初始化所有可用功法
	private void InitTechniques()
	{
		// 基础功法
		PlayerData.AddTechnique(new Technique(
			"吐纳术", 
			"基础呼吸法，可提升气力。", 
			TechniqueType.QiCultivation, 
			1, 
			new Dictionary<string, int> { { "气力", 2 } }
		));
		
		// 添加一些初始功法
		PlayerData.LearnTechnique("吐纳术");
	}
	
	// 初始化所有可用物品
	private void InitItems()
	{
		// 给玩家一些初始物品
		PlayerData.AddInventoryItem("低级灵石", 3);
	}
	
	// 保存战斗信息
	public void SetBattleInfo(string enemyType, string location)
	{
		_currentEnemyType = enemyType;
		_currentBattleLocation = location;
	}
	
	// 获取战斗敌人信息
	public string GetCurrentEnemyType()
	{
		return _currentEnemyType;
	}
	
	// 获取战斗地点信息
	public string GetCurrentBattleLocation()
	{
		return _currentBattleLocation;
	}
	
	// 处理战斗结果
	public void HandleBattleResult(bool victory)
	{
		if (victory)
		{
			// 根据敌人类型给予奖励
			GiveRewardBasedOnEnemy(_currentEnemyType);
			
			// 增加经验值
			int expGain = CalculateExpGain(_currentEnemyType);
			PlayerData.AddExperience(expGain);
		}
		else
		{
			// 战斗失败
			// 可能会失去一些物品或受到惩罚
			// 这里暂不实现惩罚机制
		}
		
		// 清空战斗信息
		_currentEnemyType = null;
		_currentBattleLocation = null;
	}
	
	// 根据敌人类型计算经验值
	private int CalculateExpGain(string enemyType)
	{
		// 根据敌人类型返回不同的经验值
		switch (enemyType)
		{
			case "灵兽幼崽":
				return 10;
			case "修炼者":
				return 15;
			case "水灵兽":
				return 20;
			case "水系修炼者":
				return 25;
			case "一阶妖兽":
				return 30;
			case "二阶妖兽":
				return 50;
			case "三阶妖兽":
				return 80;
			case "草灵":
				return 15;
			case "守园者":
				return 20;
			default:
				return 10;
		}
	}
	
	// 根据敌人类型给予奖励
	private void GiveRewardBasedOnEnemy(string enemyType)
	{
		switch (enemyType)
		{
			case "灵兽幼崽":
				PlayerData.AddInventoryItem("低级灵石", 2);
				break;
			case "修炼者":
				PlayerData.AddInventoryItem("中级灵石", 1);
				if (new Random().Next(100) < 30)
				{
					PlayerData.AddInventoryItem("修炼秘籍", 1);
				}
				break;
			case "水灵兽":
				PlayerData.AddInventoryItem("水灵珠", 1);
				break;
			case "水系修炼者":
				PlayerData.AddInventoryItem("中级灵石", 2);
				if (new Random().Next(100) < 40)
				{
					PlayerData.AddInventoryItem("水系功法", 1);
				}
				break;
			case "一阶妖兽":
				PlayerData.AddInventoryItem("妖兽皮毛", 2);
				if (new Random().Next(100) < 30)
				{
					PlayerData.AddInventoryItem("妖丹", 1);
				}
				break;
			case "二阶妖兽":
				PlayerData.AddInventoryItem("妖兽皮毛", 3);
				PlayerData.AddInventoryItem("妖丹", 1);
				break;
			case "三阶妖兽":
				PlayerData.AddInventoryItem("妖兽皮毛", 5);
				PlayerData.AddInventoryItem("妖丹", 2);
				if (new Random().Next(100) < 20)
				{
					PlayerData.AddInventoryItem("妖兽功法", 1);
				}
				break;
			case "草灵":
				PlayerData.AddInventoryItem("普通灵草", 3);
				if (new Random().Next(100) < 40)
				{
					PlayerData.AddInventoryItem("珍稀灵草", 1);
				}
				break;
			case "守园者":
				PlayerData.AddInventoryItem("珍稀灵草", 2);
				if (new Random().Next(100) < 30)
				{
					PlayerData.AddInventoryItem("炼丹秘方", 1);
				}
				break;
			default:
				// 默认奖励
				PlayerData.AddInventoryItem("低级灵石", 1);
				break;
		}
	}
	
	// 在修炼场景，通过修炼时间和功法类型获得相应属性提升
	public void DoCultivation(string techniqueName, float duration)
	{
		if (PlayerData != null)
		{
			PlayerData.Cultivate(techniqueName, duration);
		}
	}

	public override void _Process(double delta)
	{
		// 更新临时加成
		PlayerData.UpdateTemporaryBonuses((float)delta);
		
		// 其他处理逻辑...
	}

	// 线性音量转换为分贝
	private float LinearToDb(float linear)
	{
		if (linear <= 0)
			return -80.0f; // 静音
		
		return Mathf.LinearToDb(linear);
	}
	
	// 播放背景音乐
	public void PlayBackgroundMusic(string path, bool loop = true)
	{
		try
		{
			// 如果已经在播放相同的音乐，则不重新开始
			if (_currentMusic == path && _musicPlayer.Playing)
				return;
				
			// 如果音乐被禁用，则不播放
			if (!_musicEnabled)
				return;
				
			// 加载音乐资源
			if (ResourceLoader.Exists(path))
			{
				var music = ResourceLoader.Load<AudioStream>(path);
				if (music != null)
				{
					_musicPlayer.Stream = music;
					_musicPlayer.Playing = true;
					_currentMusic = path;
					
					if (loop)
					{
						_musicPlayer.Finished += () => _musicPlayer.Play();
					}
					
					GD.Print($"正在播放背景音乐: {path}");
				}
				else
				{
					GD.PrintErr($"无法加载音乐资源: {path}");
				}
			}
			else
			{
				GD.PrintErr($"音乐文件不存在: {path}");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"播放背景音乐时出错: {ex.Message}");
		}
	}
	
	// 播放音效
	public void PlaySoundEffect(string path)
	{
		try
		{
			// 如果音效被禁用，则不播放
			if (!_sfxEnabled)
				return;
				
			// 加载音效资源
			if (ResourceLoader.Exists(path))
			{
				var sfx = ResourceLoader.Load<AudioStream>(path);
				if (sfx != null)
				{
					_sfxPlayer.Stream = sfx;
					_sfxPlayer.Play();
				}
				else
				{
					GD.PrintErr($"无法加载音效资源: {path}");
				}
			}
			else
			{
				GD.PrintErr($"音效文件不存在: {path}");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"播放音效时出错: {ex.Message}");
		}
	}
	
	// 停止背景音乐
	public void StopBackgroundMusic()
	{
		_musicPlayer.Stop();
		_currentMusic = "";
	}
	
	// 设置音乐音量
	public void SetMusicVolume(float volume)
	{
		_musicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
		_musicPlayer.VolumeDb = LinearToDb(_musicVolume);
	}
	
	// 设置音效音量
	public void SetSfxVolume(float volume)
	{
		_sfxVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
		_sfxPlayer.VolumeDb = LinearToDb(_sfxVolume);
	}
	
	// 启用/禁用音乐
	public void EnableMusic(bool enabled)
	{
		_musicEnabled = enabled;
		if (!enabled)
		{
			_musicPlayer.Stop();
		}
		else if (!string.IsNullOrEmpty(_currentMusic))
		{
			PlayBackgroundMusic(_currentMusic);
		}
	}
	
	// 启用/禁用音效
	public void EnableSfx(bool enabled)
	{
		_sfxEnabled = enabled;
	}
	
	// 获取当前音乐音量
	public float GetMusicVolume()
	{
		return _musicVolume;
	}
	
	// 获取当前音效音量
	public float GetSfxVolume()
	{
		return _sfxVolume;
	}
	
	// 是否启用音乐
	public bool IsMusicEnabled()
	{
		return _musicEnabled;
	}
	
	// 是否启用音效
	public bool IsSfxEnabled()
	{
		return _sfxEnabled;
	}

	// 手动使GameManager成为持久节点（替代AutoLoad）
	public static void MakePersistent(GameManager manager)
	{
		try
		{
			if (Engine.GetMainLoop() is SceneTree tree && tree.Root != null)
			{
				// 检查是否已经在root下
				if (manager.GetParent() == tree.Root && manager.Name == "GameManager")
				{
					GD.Print("GameManager已经是根节点的子节点，无需重新父化");
					_instance = manager;
					return;
				}
				
				// 如果已经存在实例，销毁此实例
				if (_instance != null && _instance != manager)
				{
					GD.Print("已存在GameManager实例，销毁当前实例");
					manager.QueueFree();
					return;
				}
				
				// 设置单例实例
				_instance = manager;
				
				// 设置节点名称和处理模式
				manager.Name = "GameManager";
				manager.ProcessMode = ProcessModeEnum.Always;
				
				// 检查节点是否正在场景树中
				if (manager.IsInsideTree())
				{
					// 从父节点移除并添加到根节点（使用延迟调用避免冲突）
					if (manager.GetParent() != null && manager.GetParent() != tree.Root)
					{
						// 设置一个延迟调用，避免在父节点忙时调用
						manager.CallDeferred("_ReparentToRoot");
					}
				}
				else
				{
					// 节点不在场景树中，直接添加到根节点
					tree.Root.CallDeferred("add_child", manager);
				}
				
				GD.Print("GameManager已手动设置为持久节点");
			}
			else
			{
				GD.PrintErr("无法获取SceneTree，不能设置GameManager为持久节点");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("设置GameManager为持久节点时出错: " + ex.Message);
		}
	}

	// 延迟调用的方法，将节点重新父化到根节点
	private void _ReparentToRoot()
	{
		GD.Print("正在重新父化GameManager到根节点");
		try
		{
			var parent = GetParent();
			if (parent != null)
			{
				parent.RemoveChild(this);
				
				var tree = GetTree();
				if (tree != null && tree.Root != null && !IsInsideTree())
				{
					tree.Root.AddChild(this);
					GD.Print("GameManager已成功移动到根节点");
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("重新父化GameManager时出错: " + ex.Message);
		}
	}
}

// 游戏状态枚举
public enum GameState
{
	MainMenu,      // 主菜单
	Story,         // 故事模式
	Battle,        // 战斗
	Cultivation,   // 修炼
	Inventory,     // 背包
	Map,           // 地图
	Options        // 选项
} 
