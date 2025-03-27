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
	private List<CardInfo> _drawPile = new List<CardInfo>();    // 抽牌堆
	private List<CardInfo> _hand = new List<CardInfo>();        // 手牌
	private List<CardInfo> _discardPile = new List<CardInfo>(); // 弃牌堆
	private List<CardInfo> _exhaustPile = new List<CardInfo>(); // 消耗堆
	
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
	
	// 音效相关
	private AudioStreamPlayer _cardPlaySound;
	private AudioStreamPlayer _cardNoEnergySound;
	private AudioStreamPlayer _playerHitSound;
	private AudioStreamPlayer _enemyHitSound;
	private AudioStreamPlayer _victorySound;
	
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
		
		// 初始化音效
		InitializeSounds();
		
		// 初始化战斗
		InitBattle();
	}
	
	// 初始化音效
	private void InitializeSounds()
	{
		// 创建音效播放器
		_cardPlaySound = new AudioStreamPlayer();
		_cardNoEnergySound = new AudioStreamPlayer();
		_playerHitSound = new AudioStreamPlayer();
		_enemyHitSound = new AudioStreamPlayer();
		_victorySound = new AudioStreamPlayer();
		
		// 加载音效
		_cardPlaySound.Stream = ResourceLoader.Load<AudioStream>("res://Resources/Audio/SFX/Combat/card_play.wav");
		_cardNoEnergySound.Stream = ResourceLoader.Load<AudioStream>("res://Resources/Audio/SFX/Combat/no_energy.wav");
		_playerHitSound.Stream = ResourceLoader.Load<AudioStream>("res://Resources/Audio/SFX/Combat/player_hit.ogg");
		_enemyHitSound.Stream = ResourceLoader.Load<AudioStream>("res://Resources/Audio/SFX/Combat/enemy_hit.ogg");
		_victorySound.Stream = ResourceLoader.Load<AudioStream>("res://Resources/Audio/SFX/Combat/victory.wav");
		
		// 设置音量
		_cardPlaySound.VolumeDb = -5.0f;
		_cardNoEnergySound.VolumeDb = -5.0f;
		_playerHitSound.VolumeDb = -5.0f;
		_enemyHitSound.VolumeDb = -5.0f;
		_victorySound.VolumeDb = -3.0f;
		
		// 添加到场景
		AddChild(_cardPlaySound);
		AddChild(_cardNoEnergySound);
		AddChild(_playerHitSound);
		AddChild(_enemyHitSound);
		AddChild(_victorySound);
	}
	
	// 播放卡牌使用音效
	public void PlayCardSound()
	{
		if (_cardPlaySound != null && _cardPlaySound.Stream != null)
		{
			_cardPlaySound.Play();
		}
	}
	
	// 播放气力不足音效
	public void PlayNoEnergySound()
	{
		if (_cardNoEnergySound != null && _cardNoEnergySound.Stream != null)
		{
			_cardNoEnergySound.Play();
		}
	}
	
	// 播放玩家受击音效
	public void PlayPlayerHitSound()
	{
		if (_playerHitSound != null && _playerHitSound.Stream != null)
		{
			_playerHitSound.Play();
		}
	}
	
	// 播放敌人受击音效
	public void PlayEnemyHitSound()
	{
		if (_enemyHitSound != null && _enemyHitSound.Stream != null)
		{
			_enemyHitSound.Play();
		}
	}
	
	// 播放胜利音效
	public void PlayVictorySound()
	{
		if (_victorySound != null && _victorySound.Stream != null)
		{
			_victorySound.Play();
		}
	}
	
	// 初始化战斗
	private void InitBattle()
	{
		// 初始化战斗数据
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		
		// 初始化玩家角色 - 使用PlayerCharacter而不是Character
		if (gameManager != null && gameManager.PlayerData != null)
		{
			// 从玩家数据创建PlayerCharacter实例
			Player = PlayerCharacter.FromPlayerData(gameManager.PlayerData);
		}
		else
		{
			// 创建默认PlayerCharacter
			Player = new PlayerCharacter
			{
				Name = "修仙者",
				Level = 1,
				MaxHealth = 20,
				CurrentHealth = 20,
				Attack = 5,
				Defense = 3,
			};
		}
		
		// 更新玩家属性显示
		UpdatePlayerInfo();
		
		// 创建敌人并显示敌人信息
		CreateEnemy();
		UpdateEnemyInfo();
		
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
	private void CreateEnemy()
	{
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		
		// 根据玩家等级计算敌人等级，确保不超过玩家等级3级
		int playerLevel = gameManager.PlayerData.Level;
		int maxEnemyLevel = playerLevel + 3;
		int minEnemyLevel = Math.Max(1, playerLevel - 1);
		int enemyLevel = GD.RandRange(minEnemyLevel, maxEnemyLevel);
		
		// 计算敌人属性（随着等级提高而增强）
		float levelMultiplier = 1.0f + (enemyLevel - 1) * 0.2f; // 每级提升20%的属性
		int enemyHealth = (int)(20 * levelMultiplier); // 基础生命值20
		int enemyAttack = (int)(4 * levelMultiplier); // 基础攻击力4
		int enemyDefense = (int)(2 * levelMultiplier); // 基础防御力2
		
		// 随机选择敌人名称
		string[] enemyNames = { "邪修", "妖兽", "魔头", "邪灵", "山贼", "妖狐", "鬼魅", "邪道弟子" };
		string enemyName = enemyNames[GD.RandRange(0, enemyNames.Length - 1)];
		
		// 根据等级添加前缀
		string prefix = "";
		if (enemyLevel > playerLevel + 2)
			prefix = "强大的 ";
		else if (enemyLevel > playerLevel)
			prefix = "资深 ";
		else if (enemyLevel < playerLevel)
			prefix = "弱小的 ";
		
		// 创建敌人角色
		Enemy = new Character
		{
			Name = prefix + enemyName,
			Level = enemyLevel,
			MaxHealth = enemyHealth,
			CurrentHealth = enemyHealth,
			Attack = enemyAttack,
			Defense = enemyDefense,
			IsPlayer = false
		};
		
		// 为高等级敌人添加特殊技能
		if (enemyLevel > playerLevel)
		{
			// 可以根据等级差异添加不同的状态效果
			// 例如：高等级敌人有初始格挡或增益状态
			if (enemyLevel >= playerLevel + 2)
			{
				Enemy.AddBlock(5);
				GD.Print($"强大的敌人 {Enemy.Name} 有初始格挡！");
			}
		}
		
		GD.Print($"创建了敌人：{Enemy.Name}，等级：{enemyLevel}，生命值：{enemyHealth}");
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
		
		// 计算需要抽取的卡牌数量（最多抽到5张）
		int cardsToDrawCount = 5 - _hand.Count;
		
		// 抽取卡牌直到手牌达到5张
		for (int i = 0; i < cardsToDrawCount && i < 5; i++)
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
		for (int i = 0; i < 10; i++)
		{
			var strikeCard = new CardInfo
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
			var defendCard = new CardInfo
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
		var burstCard = new CardInfo
		{
			Name = "气爆术",
			Description = "造成10点伤害",
			Cost = 2,
			Type = CardType.Attack,
			ImagePath = "res://Resources/Images/Cards/burst.png",
			Effects = new List<CardEffect> { new DamageEffect { DamageAmount = 10 } }
		};
		_drawPile.Add(burstCard);
		
		var qiFlowCard = new CardInfo
		{
			Name = "五雷天心诀",
			Description = "抽2张牌",
			Cost = 1,
			Type = CardType.Skill,
			ImagePath = "res://Resources/Images/Cards/qiflow.png",
			Effects = new List<CardEffect> { new DrawCardEffect { CardCount = 2 } }
		};
		_drawPile.Add(qiFlowCard);
		
		var concentrateCard = new CardInfo
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
			CardInfo temp = _drawPile[k];
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
			CardInfo card = _drawPile[0];
			_drawPile.RemoveAt(0);
			_hand.Add(card);
			
			// 创建并添加卡牌UI
			CreateCardUI(card);
			
			UpdateUI();
		}
	}
	
	// 使用预制体创建卡牌UI
	private void CreateCardUI(CardInfo card)
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
		CardInfo card = cardUI.GetCardData();
		if (card == null) return;
		
		// 检查气力是否足够
		if (card.Cost > _currentEnergy)
		{
			AddBattleLog($"气力不足，无法使用「{card.Name}」！", false);
			// 返回原位（卡牌UI内部会处理）
			return;
		}
		
		// 使用卡牌
		UseCard(cardUI);
		
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
	private void UseCard(CardUI cardUI)
	{
		CardInfo card = cardUI.GetCardData();
		
		if (card == null || _currentEnergy < card.Cost)
		{
			GD.Print("不能使用卡牌：无效卡牌或气力不足");
			return;
		}
		
		// 扣除气力值
		_currentEnergy -= card.Cost;
		
		// 播放卡牌使用音效
		PlayCardSound();
		
		// 从手牌中移除
		_hand.Remove(card);
		cardUI.QueueFree();
		
		// 执行卡牌效果
		card.Play(this, Enemy);
		
		// 根据卡牌类型添加适当的效果
		if (card.Type == CardType.Attack)
		{
			// 获取敌人角色动画组件
			CharacterAnimation enemyAnim = GetNode<CharacterAnimation>("EnemyCharacterArea/EnemyCharacter");
			if (enemyAnim != null)
			{
				// 播放敌人受击动画
				enemyAnim.PlayHitAnimation();
				
				// 播放敌人受击音效
				PlayEnemyHitSound();
			}
		}
		
		// 将卡牌添加到弃牌堆
		_discardPile.Add(card);
		
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
		// 不再处理手牌，保留所有卡牌
		AddBattleLog("回合结束，保留手牌", false);
		
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
		
		// 延迟执行敌人行动
		_timer.WaitTime = 1.0f;
		_timer.Timeout += () => EnemyAction();
		_timer.Start();
	}
	
	// 敌人行动
	private void EnemyAction()
	{
		// 解除计时器绑定，避免多次调用
		_timer.Timeout -= () => EnemyAction();
		
		// 简单AI：随机选择行动
		Random random = new Random();
		int action = random.Next(0, 10);
		
		// 70%概率攻击，30%概率防御
		if (action < 7)
		{
			// 攻击
			int damage = Enemy.Attack;
			
			// 获取敌人角色动画组件
			CharacterAnimation enemyAnim = GetNode<CharacterAnimation>("EnemyCharacterArea/EnemyCharacter");
			if (enemyAnim != null)
			{
				// 播放攻击动画
				enemyAnim.PlayAttackAnimation();
			}
			
			// 延迟一下再造成伤害，配合动画效果
			GetTree().CreateTimer(0.25f).Timeout += () =>
			{
				Player.TakeDamage(damage);
				
				// 获取玩家角色动画组件
				CharacterAnimation playerAnim = GetNode<CharacterAnimation>("PlayerCharacterArea/PlayerCharacter");
				if (playerAnim != null)
				{
					// 播放玩家受击动画
					playerAnim.PlayHitAnimation();
					
					// 播放玩家受击音效
					PlayPlayerHitSound();
				}
				
				AddBattleLog($"{Enemy.Name}对你造成了{damage}点伤害！", false);
				UpdatePlayerInfo(); // 更新玩家信息
				
				// 在伤害处理后延迟结束回合
				GetTree().CreateTimer(1.0f).Timeout += () => EndEnemyTurn();
			};
		}
		else
		{
			// 防御
			Enemy.AddBlock(Enemy.Defense);
			AddBattleLog($"{Enemy.Name}获得了{Enemy.Defense}点格挡。", false);
			UpdateEnemyInfo();
			
			// 防御完毕后延迟结束回合
			GetTree().CreateTimer(1.0f).Timeout += () => EndEnemyTurn();
		}
	}
	
	// 结束敌人回合
	private void EndEnemyTurn()
	{
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
		
		// 播放胜利音效
		PlayVictorySound();
		
		// 创建并显示胜利结算画面
		PackedScene victoryScreenScene = GD.Load<PackedScene>("res://Scenes/Prefabs/VictoryScreen.tscn");
		if (victoryScreenScene != null)
		{
			VictoryScreen victoryScreen = victoryScreenScene.Instantiate<VictoryScreen>();
			AddChild(victoryScreen);
			
			// 初始化胜利画面并传递玩家角色和敌人等级
			if (Player is PlayerCharacter playerCharacter)
			{
				victoryScreen.Initialize(playerCharacter, Enemy.Level);
				
				// 连接战斗完成信号
				victoryScreen.BattleCompleted += () =>
				{
					// 处理战斗胜利结果
					_gameManager.HandleBattleResult(true);
					
					// 返回到游戏主场景
					_gameManager.NavigateToScene("Game");
				};
			}
			else
			{
				// 如果Player不是PlayerCharacter类型，则直接完成战斗
				GD.PrintErr("玩家角色不是PlayerCharacter类型，跳过经验值结算");
				_gameManager.HandleBattleResult(true);
				_gameManager.NavigateToScene("Game");
			}
		}
		else
		{
			// 找不到胜利画面场景，直接完成战斗
			GD.PrintErr("找不到胜利画面场景");
			_gameManager.HandleBattleResult(true);
			_gameManager.NavigateToScene("Game");
		}
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

	// 更新玩家信息面板
	private void UpdatePlayerInfo()
	{
		if (Player == null || _playerNameLabel == null) return;
		
		_playerNameLabel.Text = $"{Player.Name} Lv.{Player.Level}";
		_playerHealthBar.Value = (float)Player.CurrentHealth / Player.MaxHealth * 100;
		_playerHealthLabel.Text = $"{Player.CurrentHealth}/{Player.MaxHealth}";
		_playerQiBar.Value = (float)Player.CurrentQi / Player.MaxQi * 100;
		_playerQiLabel.Text = $"{Player.CurrentQi}/{Player.MaxQi}";
	}
	
	// 更新敌人信息面板
	private void UpdateEnemyInfo()
	{
		if (Enemy == null || _enemyNameLabel == null) return;
		
		_enemyNameLabel.Text = Enemy.Name;
		_enemyLevelLabel.Text = $"Lv.{Enemy.Level}";
		_enemyHealthBar.Value = (float)Enemy.CurrentHealth / Enemy.MaxHealth * 100;
		_enemyHealthLabel.Text = $"{Enemy.CurrentHealth}/{Enemy.MaxHealth}";
		_enemyQiBar.Value = (float)Enemy.CurrentQi / Enemy.MaxQi * 100;
		_enemyQiLabel.Text = $"{Enemy.CurrentQi}/{Enemy.MaxQi}";
	}

	// 执行敌人回合
	private void ExecuteEnemyTurn()
	{
		_currentState = BattleState.EnemyTurn;
		AddBattleLog($"{Enemy.Name}的回合开始。", true);
		
		// 延迟执行敌人行动
		_timer.WaitTime = 1.0f;
		_timer.Timeout += () => EnemyAction();
		_timer.Start();
	}
} 
