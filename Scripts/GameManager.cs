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
                // 注意：这种方式只在GameManager已经作为AutoLoad添加后有效
                _instance = Engine.GetSingleton("GameManager") as GameManager;
                
                if (_instance == null)
                {
                    GD.PrintErr("GameManager单例未找到，请确保已在项目设置中添加为AutoLoad");
                }
            }
            return _instance;
        }
    }
    
    // 游戏数据
    public PlayerData PlayerData { get; private set; }
    public DivinationSystem DivinationSystem { get; private set; }
    
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
    private const string DivinationScene = "res://Scenes/Divination.tscn";
    private const string InventoryScene = "res://Scenes/Inventory.tscn";
    private const string ExplorationScene = "res://Scenes/Exploration.tscn";
    
    // 初始化
    public override void _Ready()
    {
        GD.Print("GameManager初始化中...");
        
        // 使GameManager在场景切换时不会被销毁
        ProcessMode = ProcessModeEnum.Always;
        
        // 设置单例实例
        if (_instance == null)
        {
            _instance = this;
            GD.Print("GameManager单例已设置");
        }
        else if (_instance != this)
        {
            GD.PushWarning("GameManager单例已存在，销毁当前实例");
            QueueFree();
            return;
        }
        
        // 检查必要资源
        CheckResources();
        
        // 初始化游戏数据
        PlayerData = new PlayerData("修仙者");
        DivinationSystem = new DivinationSystem();
        DivinationSystem.InitializeDeck();
        
        // 初始化技能列表
        InitTechniques();
        
        // 初始化物品列表
        InitItems();
        
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
    
    // 开始新游戏
    public void StartNewGame(string playerName)
    {
        try
        {
            // 创建新的玩家数据
            PlayerData = new PlayerData(playerName);
            
            // 初始化基础功法和物品
            InitTechniques();
            InitItems();
            
            // 重置占卜系统
            DivinationSystem = new DivinationSystem();
            DivinationSystem.InitializeDeck();
            DivinationSystem.ShuffleDeck();
            
            // 切换到游戏场景
            var tree = GetSceneTreeSafe();
            if (tree != null)
            {
                tree.ChangeSceneToFile("res://Scenes/Game.tscn");
            }
            else
            {
                GD.PrintErr("无法获取SceneTree，场景切换失败");
            }
            
            // 发送玩家数据更新信号
            EmitSignal(SignalName.PlayerDataUpdated);
        }
        catch (Exception ex)
        {
            GD.PrintErr("开始新游戏时发生错误: " + ex.Message);
        }
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
    
    // 占卜系统
    public List<DivinationCard> PerformDivination(int cardCount = 3)
    {
        // 洗牌
        DivinationSystem.ShuffleDeck();
        
        // 抽取指定数量的牌
        List<DivinationCard> drawnCards = DivinationSystem.DrawCards(cardCount);
        
        return drawnCards;
    }
    
    // 选择占卜牌并应用效果
    public void SelectDivinationCard(DivinationCard card)
    {
        DivinationSystem.ApplyCardEffect(card, PlayerData);
        EmitSignal(SignalName.PlayerDataUpdated);
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
            string targetScene = "";
            
            switch (sceneName)
            {
                case "MainMenu":
                    targetScene = MainMenuScene;
                    break;
                case "Game":
                    targetScene = GameScene;
                    break;
                case "Battle":
                    targetScene = BattleScene;
                    break;
                case "Divination":
                    targetScene = DivinationScene;
                    break;
                case "Inventory":
                    targetScene = InventoryScene;
                    break;
                case "Exploration":
                    targetScene = ExplorationScene;
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
    
    // 通过占卜获得属性提升
    public void ApplyDivinationEffect(string cardName)
    {
        switch (cardName)
        {
            case "太极":
                PlayerData.AddAttributePoints("气力", 2);
                PlayerData.AddAttributePoints("神识", 2);
                break;
            case "天道":
                PlayerData.AddAttributePoints("命运", 3);
                break;
            case "红莲":
                PlayerData.AddAttributePoints("体魄", 4);
                break;
            default:
                // 默认随机增加一项属性
                string[] attributes = new string[] { "气力", "神识", "体魄", "命运" };
                Random random = new Random();
                PlayerData.AddAttributePoints(attributes[random.Next(attributes.Length)], 2);
                break;
        }
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
    
    public void ShowExplorationMap()
    {
        // 导航到探索地图场景
        NavigateToScene("Exploration");
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
}

// 游戏状态枚举
public enum GameState
{
    MainMenu,      // 主菜单
    Story,         // 故事模式
    Battle,        // 战斗
    Cultivation,   // 修炼
    Divination,    // 占卜
    Inventory,     // 背包
    Map,           // 地图
    Options        // 选项
} 