using Godot;
using System;
using System.Collections.Generic;

public partial class ExplorationManager : Control
{
    // 地点定义
    private class LocationInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int DifficultyLevel { get; set; }
        public Dictionary<string, int> ResourceProbability { get; set; }
        public Dictionary<string, int> EnemyProbability { get; set; }
    }

    // 地点字典
    private Dictionary<string, LocationInfo> _locations;
    
    // 节点引用
    private Label _infoLabel;
    private Button _backButton;
    private Dictionary<string, Button> _exploreButtons = new Dictionary<string, Button>();
    
    // 玩家数据引用
    private PlayerData _playerData;
    
    // 场景引用
    private string _gameScenePath = "res://Scenes/Game.tscn";
    private string _battleScenePath = "res://Scenes/Battle.tscn";
    
    public override void _Ready()
    {
        // 获取节点引用
        _infoLabel = GetNode<Label>("InfoPanel/InfoLabel");
        _backButton = GetNode<Button>("BackButton");
        
        // 获取所有探索按钮
        _exploreButtons["青云峰"] = GetNode<Button>("MapContainer/GridContainer/Location1/VBoxContainer/ExploreButton");
        _exploreButtons["灵溪谷"] = GetNode<Button>("MapContainer/GridContainer/Location2/VBoxContainer/ExploreButton");
        _exploreButtons["妖兽林"] = GetNode<Button>("MapContainer/GridContainer/Location3/VBoxContainer/ExploreButton");
        _exploreButtons["百草园"] = GetNode<Button>("MapContainer/GridContainer/Location4/VBoxContainer/ExploreButton");
        
        // 连接信号
        _backButton.Pressed += OnBackButtonPressed;
        foreach (var button in _exploreButtons)
        {
            button.Value.Pressed += () => OnExploreButtonPressed(button.Key);
        }
        
        // 初始化地点信息
        InitLocations();
        
        // 获取玩家数据
        _playerData = GetNode<GameManager>("/root/GameManager").PlayerData;
        
        // 更新界面
        UpdateLocationInfo();
    }
    
    private void InitLocations()
    {
        _locations = new Dictionary<string, LocationInfo>
        {
            ["青云峰"] = new LocationInfo
            {
                Name = "青云峰",
                Description = "此处灵气浓郁，适合修炼，可能找到珍贵的修炼资源。",
                DifficultyLevel = 1,
                ResourceProbability = new Dictionary<string, int>
                {
                    ["低级灵石"] = 60,
                    ["中级灵石"] = 20,
                    ["灵药草"] = 40,
                    ["修炼秘籍"] = 10
                },
                EnemyProbability = new Dictionary<string, int>
                {
                    ["灵兽幼崽"] = 30,
                    ["修炼者"] = 10
                }
            },
            ["灵溪谷"] = new LocationInfo
            {
                Name = "灵溪谷",
                Description = "此处有灵气溪流，可能找到水属性修炼资源和丹药材料。",
                DifficultyLevel = 2,
                ResourceProbability = new Dictionary<string, int>
                {
                    ["低级灵石"] = 40,
                    ["中级灵石"] = 30,
                    ["水灵果"] = 50,
                    ["灵溪泉水"] = 60,
                    ["炼丹炉"] = 5
                },
                EnemyProbability = new Dictionary<string, int>
                {
                    ["水灵兽"] = 40,
                    ["水系修炼者"] = 20
                }
            },
            ["妖兽林"] = new LocationInfo
            {
                Name = "妖兽林",
                Description = "危险的妖兽栖息地，有可能获得妖兽材料，但战斗几率较高。",
                DifficultyLevel = 3,
                ResourceProbability = new Dictionary<string, int>
                {
                    ["妖兽皮毛"] = 70,
                    ["妖丹"] = 30,
                    ["高级灵石"] = 20,
                    ["妖兽功法"] = 10
                },
                EnemyProbability = new Dictionary<string, int>
                {
                    ["一阶妖兽"] = 50,
                    ["二阶妖兽"] = 30,
                    ["三阶妖兽"] = 10
                }
            },
            ["百草园"] = new LocationInfo
            {
                Name = "百草园",
                Description = "各种灵草药材的生长地，适合采集丹药材料，较少遇到危险。",
                DifficultyLevel = 1,
                ResourceProbability = new Dictionary<string, int>
                {
                    ["普通灵草"] = 80,
                    ["珍稀灵草"] = 40,
                    ["百年灵芝"] = 20,
                    ["炼丹秘方"] = 10
                },
                EnemyProbability = new Dictionary<string, int>
                {
                    ["草灵"] = 20,
                    ["守园者"] = 10
                }
            }
        };
    }
    
    private void UpdateLocationInfo()
    {
        // 根据玩家等级和境界，更新地点信息
        int playerLevel = _playerData?.Level ?? 1;
        
        foreach (var location in _locations)
        {
            // 检查地点难度是否适合玩家等级
            bool isAccessible = location.Value.DifficultyLevel <= playerLevel;
            string locationName = location.Key;
            
            if (_exploreButtons.ContainsKey(locationName))
            {
                _exploreButtons[locationName].Disabled = !isAccessible;
            }
        }
    }
    
    private void OnLocationPanelHover(string locationName)
    {
        // 当鼠标悬停在地点面板上时，显示地点信息
        if (_locations.ContainsKey(locationName))
        {
            var location = _locations[locationName];
            _infoLabel.Text = $"{location.Name}：{location.Description}\n难度等级：{location.DifficultyLevel}";
        }
    }
    
    private void OnExploreButtonPressed(string locationName)
    {
        // 随机确定探索结果
        Random random = new Random();
        int eventType = random.Next(100);
        
        if (_locations.ContainsKey(locationName))
        {
            var location = _locations[locationName];
            
            // 根据概率决定是遇到敌人还是获得资源
            if (eventType < GetEnemyEncounterChance(location))
            {
                // 遇到敌人，触发战斗
                StartBattle(location);
            }
            else
            {
                // 获得资源
                GainResource(location);
            }
        }
    }
    
    private int GetEnemyEncounterChance(LocationInfo location)
    {
        // 根据地点难度和玩家等级，计算遇敌概率
        int baseChance = location.DifficultyLevel * 15;
        int playerLevel = _playerData?.Level ?? 1;
        
        // 玩家等级越高，遇敌概率越低
        return Math.Max(10, baseChance - (playerLevel - location.DifficultyLevel) * 5);
    }
    
    private void StartBattle(LocationInfo location)
    {
        // 选择一个敌人
        string enemyType = SelectRandomEnemy(location);
        
        // 保存敌人信息到GameManager
        var gameManager = GetNode<GameManager>("/root/GameManager");
        gameManager.SetBattleInfo(enemyType, location.Name);
        
        // 切换到战斗场景
        GetTree().ChangeSceneToFile(_battleScenePath);
    }
    
    private string SelectRandomEnemy(LocationInfo location)
    {
        Random random = new Random();
        int totalWeight = 0;
        
        foreach (var enemy in location.EnemyProbability)
        {
            totalWeight += enemy.Value;
        }
        
        int randomValue = random.Next(totalWeight);
        int currentWeight = 0;
        
        foreach (var enemy in location.EnemyProbability)
        {
            currentWeight += enemy.Value;
            if (randomValue < currentWeight)
            {
                return enemy.Key;
            }
        }
        
        // 默认返回第一个敌人
        return location.EnemyProbability.Keys.GetEnumerator().Current;
    }
    
    private void GainResource(LocationInfo location)
    {
        Random random = new Random();
        
        // 可能获得的资源列表
        List<string> possibleResources = new List<string>();
        foreach (var resource in location.ResourceProbability)
        {
            if (random.Next(100) < resource.Value)
            {
                possibleResources.Add(resource.Key);
            }
        }
        
        if (possibleResources.Count > 0)
        {
            // 随机选择一个资源
            string gainedResource = possibleResources[random.Next(possibleResources.Count)];
            
            // 更新玩家物品栏
            _playerData.AddInventoryItem(gainedResource, 1);
            
            // 显示获得资源信息
            _infoLabel.Text = $"在{location.Name}探索中，你发现了{gainedResource}！";
        }
        else
        {
            // 没有获得资源
            _infoLabel.Text = $"在{location.Name}探索了一番，但什么也没发现。";
        }
    }
    
    private void OnBackButtonPressed()
    {
        GD.Print("返回游戏主场景");
        // 返回游戏主场景
        GetTree().ChangeSceneToFile(_gameScenePath);
    }
} 