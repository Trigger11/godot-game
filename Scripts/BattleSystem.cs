using Godot;
using System;
using System.Collections.Generic;

public partial class BattleSystem : Control
{
	// 游戏管理器引用
	private GameManager _gameManager;
	
	// 战斗状态枚举
	private enum BattleState
	{
		Start,
		PlayerTurn,
		EnemyTurn,
		Victory,
		Defeat
	}
	
	// 战斗状态
	private BattleState _currentState = BattleState.Start;
	
	// 战斗角色
	public Character Player { get; private set; }
	public Character Enemy { get; private set; }
	
	// 卡牌相关
	private List<Card> _drawPile = new List<Card>();    // 抽牌堆
	private List<Card> _hand = new List<Card>();        // 手牌
	private List<Card> _discardPile = new List<Card>(); // 弃牌堆
	private List<Card> _exhaustPile = new List<Card>(); // 消耗堆
	
	// 当前回合气力值
	private int _currentEnergy = 0;
	private int _maxEnergy = 3;
	
	// 公开当前气力值属性供其他类访问
	public int CurrentEnergy => _currentEnergy;
	public int MaxEnergy => _maxEnergy;
	
	// UI元素
	private Label _enemyNameLabel;
	private Label _enemyLevelLabel;
	private ProgressBar _enemyHealthBar;
	private Label _enemyHealthLabel;
	private ProgressBar _enemyQiBar;
	private Label _enemyQiLabel;
	
	private Label _playerNameLabel;
	private ProgressBar _playerHealthBar;
	private Label _playerHealthLabel;
	private ProgressBar _playerQiBar;
	private Label _playerQiLabel;
	
	private RichTextLabel _logText;
	private Button _endTurnButton;
	
	// 卡牌区域和能量显示
	private HBoxContainer _handContainer;
	private Label _energyLabel;
	private Button _drawPileButton;
	private Button _discardPileButton;
	
	// 定时器
	private Timer _timer;
	
	public override void _Ready()
	{
		// 获取GameManager引用
		_gameManager = GameManager.Instance;
		
		// 初始化UI引用
		_enemyNameLabel = GetNode<Label>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer/EnemyName");
		_enemyLevelLabel = GetNode<Label>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer/EnemyLevel");
		_enemyHealthBar = GetNode<ProgressBar>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer2/EnemyHealthBar");
		_enemyHealthLabel = GetNode<Label>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer2/EnemyHealthLabel");
		_enemyQiBar = GetNode<ProgressBar>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer3/EnemyQiBar");
		_enemyQiLabel = GetNode<Label>("BattleField/EnemyPanel/MarginContainer/VBoxContainer/HBoxContainer3/EnemyQiLabel");
		
		_playerNameLabel = GetNode<Label>("PlayerInfo/Panel/MarginContainer/VBoxContainer/PlayerName");
		_playerHealthBar = GetNode<ProgressBar>("PlayerInfo/Panel/MarginContainer/VBoxContainer/HBoxContainer/PlayerHealthBar");
		_playerHealthLabel = GetNode<Label>("PlayerInfo/Panel/MarginContainer/VBoxContainer/HBoxContainer/PlayerHealthLabel");
		_playerQiBar = GetNode<ProgressBar>("PlayerInfo/Panel/MarginContainer/VBoxContainer/HBoxContainer2/PlayerQiBar");
		_playerQiLabel = GetNode<Label>("PlayerInfo/Panel/MarginContainer/VBoxContainer/HBoxContainer2/PlayerQiLabel");
		
		_logText = GetNode<RichTextLabel>("BattleField/BattleLog/MarginContainer/LogText");
		
		// 获取卡牌区域引用
		_handContainer = GetNode<HBoxContainer>("CardArea/HandContainer");
		_energyLabel = GetNode<Label>("CardArea/EnergyPanel/EnergyLabel");
		_drawPileButton = GetNode<Button>("CardArea/DrawPileButton");
		_discardPileButton = GetNode<Button>("CardArea/DiscardPileButton");
		
		// 获取结束回合按钮引用
		_endTurnButton = GetNode<Button>("CardArea/EndTurnButton");
		
		_timer = GetNode<Timer>("Timer");
		
		// 绑定按钮事件
		_endTurnButton.Pressed += OnEndTurnPressed;
		_drawPileButton.Pressed += OnDrawPilePressed;
		_discardPileButton.Pressed += OnDiscardPilePressed;
		
		// 初始化战斗
		InitBattle();
	}
	
	// 初始化战斗
	private void InitBattle()
	{
		// 创建玩家
		Player = new Character
		{
			Name = _gameManager.PlayerData.PlayerName,
			MaxHealth = 80,
			Attack = 10,
			Defense = 5
		};
		Player.Heal(Player.MaxHealth); // 设置初始生命值为最大生命值
		
		// 创建敌人
		string enemyType = _gameManager.GetCurrentEnemyType();
		if (string.IsNullOrEmpty(enemyType))
		{
			enemyType = "妖狐"; // 默认敌人
		}
		
		Enemy = CreateEnemy(enemyType);
		
		// 初始化战斗UI
		UpdateUI();
		
		// 清空战斗日志
		_logText.Text = "";
		
		// 初始化卡组
		InitializeDeck();
		
		// 洗牌
		ShuffleDeck();
		
		// 开始战斗
		StartBattle();
	}
	
	// 创建敌人
	private Character CreateEnemy(string enemyType)
	{
		switch (enemyType)
		{
			case "妖狐":
				return new Character 
				{ 
					Name = "妖狐", 
					MaxHealth = 50, 
					Attack = 8, 
					Defense = 3 
				};
			case "灵兽幼崽":
				return new Character 
				{ 
					Name = "灵兽幼崽", 
					MaxHealth = 30, 
					Attack = 5, 
					Defense = 2 
				};
			case "修炼者":
				return new Character 
				{ 
					Name = "修炼者", 
					MaxHealth = 60, 
					Attack = 7, 
					Defense = 5 
				};
			case "水灵兽":
				return new Character 
				{ 
					Name = "水灵兽", 
					MaxHealth = 70, 
					Attack = 9, 
					Defense = 4 
				};
			default:
				return new Character 
				{ 
					Name = "未知敌人", 
					MaxHealth = 40, 
					Attack = 6, 
					Defense = 2 
				};
		}
	}
	
	// 开始战斗
	private void StartBattle()
	{
		AddBattleLog($"{Enemy.Name}向你袭来！", true);
		
		// 开始玩家回合
		StartPlayerTurn();
	}
	
	// 开始玩家回合
	private void StartPlayerTurn()
	{
		_currentState = BattleState.PlayerTurn;
		
		// 重置玩家气力值
		_currentEnergy = _maxEnergy;
		
		// 抽取初始手牌
		for (int i = 0; i < 5; i++)
		{
			DrawCard();
		}
		
		// 更新UI
		UpdateUI();
		AddBattleLog("你的回合！", true);
	}
	
	// 初始化卡组
	private void InitializeDeck()
	{
		_drawPile.Clear();
		_hand.Clear();
		_discardPile.Clear();
		
		// 添加基础攻击牌
		for (int i = 0; i < 5; i++)
		{
			var strikeCard = new Card
			{
				Name = "打击",
				Description = "造成6点伤害",
				Cost = 1,
				Type = CardType.Attack,
				ImagePath = "res://Resources/Images/Cards/strike.png",
				Effects = new List<CardEffect> { new DamageEffect { DamageAmount = 6 } }
			};
			_drawPile.Add(strikeCard);
		}
		
		// 添加基础防御牌
		for (int i = 0; i < 5; i++)
		{
			var defendCard = new Card
			{
				Name = "防御",
				Description = "获得5点格挡",
				Cost = 1,
				Type = CardType.Defense,
				ImagePath = "res://Resources/Images/Cards/defend.png",
				Effects = new List<CardEffect> { new BlockEffect { BlockAmount = 5 } }
			};
			_drawPile.Add(defendCard);
		}
		
		// 添加特殊技能牌
		var burstCard = new Card
		{
			Name = "气爆术",
			Description = "造成10点伤害",
			Cost = 2,
			Type = CardType.Attack,
			ImagePath = "res://Resources/Images/Cards/burst.png",
			Effects = new List<CardEffect> { new DamageEffect { DamageAmount = 10 } }
		};
		_drawPile.Add(burstCard);
		
		var qiFlowCard = new Card
		{
			Name = "五雷天心诀",
			Description = "抽2张牌",
			Cost = 1,
			Type = CardType.Skill,
			ImagePath = "res://Resources/Images/Cards/qiflow.png",
			Effects = new List<CardEffect> { new DrawCardEffect { CardCount = 2 } }
		};
		_drawPile.Add(qiFlowCard);
		
		var concentrateCard = new Card
		{
			Name = "凝神",
			Description = "获得2点气力",
			Cost = 0,
			Type = CardType.Skill,
			ImagePath = "res://Resources/Images/Cards/concentrate.png",
			Effects = new List<CardEffect> { new GainEnergyEffect { EnergyAmount = 2 } }
		};
		_drawPile.Add(concentrateCard);
		
		// 确保所有卡牌都有图像
		foreach (var card in _drawPile)
		{
			// 如果路径为空或文件不存在，设置默认图像
			if (string.IsNullOrEmpty(card.ImagePath) || !ResourceLoader.Exists(card.ImagePath))
			{
				// 设置默认图像路径，根据卡牌类型
				switch (card.Type)
				{
					case CardType.Attack:
						card.ImagePath = "res://Resources/Images/Cards/strike.png";
						break;
					case CardType.Defense:
						card.ImagePath = "res://Resources/Images/Cards/defend.png";
						break;
					case CardType.Skill:
						card.ImagePath = "res://Resources/Images/Cards/qiflow.png";
						break;
					case CardType.Power:
						card.ImagePath = "res://Resources/Images/Cards/concentrate.png";
						break;
					default:
						card.ImagePath = "res://Resources/Images/Cards/burst.png";
						break;
				}
			}
		}
	}
	
	// 洗牌
	private void ShuffleDeck()
	{
		// 使用Fisher-Yates洗牌算法
		Random random = new Random();
		int n = _drawPile.Count;
		
		while (n > 1)
		{
			n--;
			int k = random.Next(n + 1);
			Card temp = _drawPile[k];
			_drawPile[k] = _drawPile[n];
			_drawPile[n] = temp;
		}
	}
	
	// 抽牌
	public void DrawCard()
	{
		// 如果抽牌堆为空，将弃牌堆洗牌后放入抽牌堆
		if (_drawPile.Count == 0 && _discardPile.Count > 0)
		{
			_drawPile.AddRange(_discardPile);
			_discardPile.Clear();
			ShuffleDeck();
			AddBattleLog("弃牌堆已洗入抽牌堆。", false);
		}
		
		// 如果抽牌堆还有牌，抽一张到手上
		if (_drawPile.Count > 0)
		{
			Card card = _drawPile[0];
			_drawPile.RemoveAt(0);
			_hand.Add(card);
			
			// 创建并添加卡牌UI
			CreateCardUI(card);
			
			UpdateUI();
		}
	}
	
	// 使用预制体创建卡牌UI
	private void CreateCardUI(Card card)
	{
		// 如果手牌已满，不再创建新卡牌
		if (_handContainer.GetChildCount() >= 8)
		{
			AddBattleLog("手牌已满！", false);
			return;
		}
		
		GD.Print($"创建卡牌UI: {card.Name}");
		
		// 从预制体加载卡牌场景
		PackedScene cardScene = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/CardPrefab.tscn");
		if (cardScene == null)
		{
			GD.PrintErr("无法加载卡牌预制体");
			return;
		}
		
		// 实例化卡牌场景
		CardUI cardUI = cardScene.Instantiate<CardUI>();
		if (cardUI == null)
		{
			GD.PrintErr("无法实例化卡牌预制体");
			return;
		}
		
		// 设置卡牌数据
		cardUI.SetCardData(card);
		
		// 连接卡牌使用信号
		cardUI.CardPlayed += OnCardPlayed;
		
		// 添加到手牌区域
		_handContainer.AddChild(cardUI);
		GD.Print($"卡牌添加到手牌区域，当前手牌数量: {_handContainer.GetChildCount()}");
	}
	
	// 卡牌使用回调
	private void OnCardPlayed(CardUI cardUI)
	{
		// 获取卡牌数据
		Card card = cardUI.GetCardData();
		if (card == null) return;
		
		// 检查气力是否足够
		if (card.Cost > _currentEnergy)
		{
			AddBattleLog($"气力不足，无法使用「{card.Name}」！", false);
			// 返回原位（卡牌UI内部会处理）
			return;
		}
		
		// 使用卡牌
		UseCard(card);
		
		// 从手牌中移除
		_hand.Remove(card);
		
		// 将卡牌移到弃牌堆
		_discardPile.Add(card);
		
		// 移除卡牌UI
		cardUI.QueueFree();
		
		// 消耗气力
		_currentEnergy -= card.Cost;
		
		// 更新UI
		UpdateUI();
		
		// 检查战斗是否结束
		CheckBattleEnd();
	}
	
	// 重新排列卡牌
	private void RearrangeCards()
	{
		// 清空手牌区域并重新添加所有卡牌
		var cards = new List<Node>();
		foreach (var child in _handContainer.GetChildren())
		{
			cards.Add(child);
			_handContainer.RemoveChild(child);
		}
		
		foreach (var card in cards)
		{
			_handContainer.AddChild(card);
		}
	}
	
	// 使用卡牌
	private void UseCard(Card card)
	{
		// 执行卡牌效果
		card.Play(this, Enemy);
		
		// 更新UI
		UpdateUI();
	}
	
	// 添加气力
	public void AddEnergy(int amount)
	{
		_currentEnergy += amount;
		UpdateUI();
	}
	
	// 更新UI
	private void UpdateUI()
	{
		// 更新敌人信息
		_enemyNameLabel.Text = Enemy.Name;
		_enemyHealthBar.Value = Enemy.GetHealthPercentage() * 100;
		_enemyHealthLabel.Text = $"{Enemy.CurrentHealth}/{Enemy.MaxHealth}";
		
		// 更新玩家信息
		_playerNameLabel.Text = Player.Name;
		_playerHealthBar.Value = Player.GetHealthPercentage() * 100;
		_playerHealthLabel.Text = $"{Player.CurrentHealth}/{Player.MaxHealth}";
		
		// 更新格挡值
		_playerQiLabel.Text = $"格挡: {Player.Block}";
		
		// 更新气力值
		_energyLabel.Text = $"气力: {_currentEnergy}/{_maxEnergy}";
		
		// 更新牌堆信息
		_drawPileButton.Text = $"抽牌堆: {_drawPile.Count}";
		_discardPileButton.Text = $"弃牌堆: {_discardPile.Count}";
	}
	
	// 添加战斗日志
	public void AddBattleLog(string text, bool isImportant = false)
	{
		if (isImportant)
		{
			_logText.Text += $"\n[color=yellow]{text}[/color]";
		}
		else
		{
			_logText.Text += $"\n{text}";
		}
	}
	
	// 结束回合按钮点击
	private void OnEndTurnPressed()
	{
		EndPlayerTurn();
	}
	
	// 结束玩家回合
	private void EndPlayerTurn()
	{
		// 将手牌全部放入弃牌堆
		foreach (var card in _hand)
		{
			_discardPile.Add(card);
		}
		_hand.Clear();
		
		// 清空手牌UI
		foreach (var child in _handContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		// 玩家回合结束处理
		Player.OnEndTurn();
		
		// 更新UI
		UpdateUI();
		
		// 开始敌人回合
		StartEnemyTurn();
	}
	
	// 开始敌人回合
	private void StartEnemyTurn()
	{
		_currentState = BattleState.EnemyTurn;
		
		AddBattleLog($"{Enemy.Name}的回合！", true);
		
		// 延迟一段时间后执行敌人行动
		_timer.WaitTime = 1.0f;
		_timer.Timeout += EnemyAction;
		_timer.Start();
	}
	
	// 敌人行动
	private void EnemyAction()
	{
		// 解除计时器事件绑定，避免多次触发
		_timer.Timeout -= EnemyAction;
		
		// 敌人攻击玩家
		int damage = Enemy.Attack;
		AddBattleLog($"{Enemy.Name}使用了「普通攻击」，造成 {damage} 点伤害！");
		Player.TakeDamage(damage);
		
		// 更新UI
		UpdateUI();
		
		// 延迟一段时间后结束敌人回合
		_timer.WaitTime = 1.0f;
		_timer.Timeout += EndEnemyTurn;
		_timer.Start();
	}
	
	// 结束敌人回合
	private void EndEnemyTurn()
	{
		// 解除计时器事件绑定，避免多次触发
		_timer.Timeout -= EndEnemyTurn;
		
		// 检查战斗是否结束
		if (CheckBattleEnd())
		{
			return;
		}
		
		// 回合结束处理
		Enemy.OnEndTurn();
		
		// 开始新的玩家回合
		StartPlayerTurn();
	}
	
	// 查看抽牌堆
	private void OnDrawPilePressed()
	{
		string cards = "";
		foreach (var card in _drawPile)
		{
			cards += $"{card.Name}({card.Cost})\n";
		}
		
		if (string.IsNullOrEmpty(cards))
		{
			AddBattleLog("抽牌堆为空！");
		}
		else
		{
			AddBattleLog($"抽牌堆：\n{cards}");
		}
	}
	
	// 查看弃牌堆
	private void OnDiscardPilePressed()
	{
		string cards = "";
		foreach (var card in _discardPile)
		{
			cards += $"{card.Name}({card.Cost})\n";
		}
		
		if (string.IsNullOrEmpty(cards))
		{
			AddBattleLog("弃牌堆为空！");
		}
		else
		{
			AddBattleLog($"弃牌堆：\n{cards}");
		}
	}
	
	// 检查战斗是否结束
	private bool CheckBattleEnd()
	{
		if (Enemy.IsDead())
		{
			// 玩家胜利
			_currentState = BattleState.Victory;
			AddBattleLog("你击败了敌人！", true);
			
			// 延迟一段时间后返回
			_timer.WaitTime = 2.0f;
			_timer.Timeout += OnBattleVictory;
			_timer.Start();
			
			return true;
		}
		else if (Player.IsDead())
		{
			// 玩家失败
			_currentState = BattleState.Defeat;
			AddBattleLog("你被击败了！", true);
			
			// 延迟一段时间后返回
			_timer.WaitTime = 2.0f;
			_timer.Timeout += OnBattleDefeat;
			_timer.Start();
			
			return true;
		}
		
		return false;
	}
	
	// 战斗胜利处理
	private void OnBattleVictory()
	{
		// 解除计时器事件绑定
		_timer.Timeout -= OnBattleVictory;
		
		// 处理战斗胜利结果
		_gameManager.HandleBattleResult(true);
		
		// 返回到游戏主场景
		_gameManager.NavigateToScene("Game");
	}
	
	// 战斗失败处理
	private void OnBattleDefeat()
	{
		// 解除计时器事件绑定
		_timer.Timeout -= OnBattleDefeat;
		
		// 处理战斗失败结果
		_gameManager.HandleBattleResult(false);
		
		// 返回到游戏主场景
		_gameManager.NavigateToScene("Game");
	}

	// 获取默认卡牌图像路径
	private string GetDefaultImagePathForCardType(CardType type)
	{
		switch (type)
		{
			case CardType.Attack:
				return "res://Resources/Images/Cards/strike.png";
			case CardType.Defense:
				return "res://Resources/Images/Cards/defend.png";
			case CardType.Skill:
				return "res://Resources/Images/Cards/qiflow.png";
			case CardType.Power:
				return "res://Resources/Images/Cards/concentrate.png";
			default:
				return "res://Resources/Images/Cards/burst.png";
		}
	}
} 
