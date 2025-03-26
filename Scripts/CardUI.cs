using Godot;
using System;

public partial class CardUI : Control
{
	// 卡牌数据
	private Card _cardData;
	
	// 拖拽相关变量
	private bool _isDragging = false;
	private Vector2 _dragStartPosition;
	private Vector2 _mouseOffset;
	
	// UI组件引用
	private Panel _cardPanel;
	private Label _nameLabel;
	private Label _descriptionLabel;
	private Label _costLabel;
	private TextureRect _cardImage;
	private Panel _valuePanel;
	private Label _valueLabel;
	
	// 信号
	[Signal]
	public delegate void CardPlayedEventHandler(CardUI card);
	
	public override void _Ready()
	{
		// 获取UI组件引用
		_cardPanel = GetNode<Panel>("CardPanel");
		_nameLabel = GetNode<Label>("CardPanel/NamePanel/CardName");
		_descriptionLabel = GetNode<Label>("CardPanel/DescPanel/CardDescription");
		_costLabel = GetNode<Label>("CardPanel/CostPanel/CostLabel");
		_cardImage = GetNode<TextureRect>("CardPanel/CardImage");
		
		// 尝试获取值面板和标签（可能不存在于所有卡牌类型）
		_valuePanel = GetNodeOrNull<Panel>("CardPanel/ValuePanel");
		if (_valuePanel != null)
		{
			_valueLabel = GetNodeOrNull<Label>("CardPanel/ValuePanel/ValueLabel");
		}
		
		// 节点准备好后，如果有卡牌数据则更新UI
		if (_cardData != null)
		{
			UpdateCardUI();
		}
	}
	
	// 设置卡牌数据
	public void SetCardData(Card cardData)
	{
		_cardData = cardData;
		
		// 只有在节点已添加到场景树中才更新UI
		if (IsInsideTree())
		{
			UpdateCardUI();
		}
	}
	
	// 获取卡牌数据
	public Card GetCardData()
	{
		return _cardData;
	}
	
	// 更新卡牌UI显示
	private void UpdateCardUI()
	{
		if (_cardData == null) return;
		
		// 检查UI组件是否已初始化
		if (_nameLabel == null || _descriptionLabel == null || _costLabel == null || _cardPanel == null || _cardImage == null)
		{
			GD.PrintErr("卡牌UI组件未初始化");
			return;
		}
		
		// 更新基本信息
		_nameLabel.Text = _cardData.Name;
		_descriptionLabel.Text = _cardData.Description;
		_costLabel.Text = _cardData.Cost.ToString();
		
		// 设置卡牌类型颜色
		StyleBoxFlat styleBox = new StyleBoxFlat();
		styleBox.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);
		styleBox.BorderWidthBottom = styleBox.BorderWidthLeft = styleBox.BorderWidthRight = styleBox.BorderWidthTop = 2;
		styleBox.BorderColor = GetCardTypeColor(_cardData.Type);
		styleBox.CornerRadiusBottomLeft = styleBox.CornerRadiusBottomRight = styleBox.CornerRadiusTopLeft = styleBox.CornerRadiusTopRight = 10;
		_cardPanel.AddThemeStyleboxOverride("panel", styleBox);
		
		// 加载卡牌图像
		if (!string.IsNullOrEmpty(_cardData.ImagePath) && ResourceLoader.Exists(_cardData.ImagePath))
		{
			Texture2D texture = ResourceLoader.Load<Texture2D>(_cardData.ImagePath);
			if (texture != null)
			{
				_cardImage.Texture = texture;
			}
		}
		else
		{
			// 设置默认图像
			string defaultPath = GetDefaultImagePathForCardType(_cardData.Type);
			if (ResourceLoader.Exists(defaultPath))
			{
				Texture2D defaultTexture = ResourceLoader.Load<Texture2D>(defaultPath);
				if (defaultTexture != null)
				{
					_cardImage.Texture = defaultTexture;
					_cardImage.Modulate = new Color(1, 1, 1, 0.8f);
				}
			}
		}
		
		// 根据卡牌类型更新特殊数值显示
		if (_valuePanel != null && _valueLabel != null)
		{
			StyleBoxFlat valueStyle = new StyleBoxFlat();
			
			if (_cardData.Type == CardType.Attack)
			{
				foreach (var effect in _cardData.Effects)
				{
					if (effect is DamageEffect damageEffect)
					{
						_valueLabel.Text = damageEffect.DamageAmount.ToString();
						valueStyle.BgColor = new Color(0.7f, 0.2f, 0.2f, 0.8f); // 红色底色用于攻击
						break;
					}
				}
			}
			else if (_cardData.Type == CardType.Defense)
			{
				foreach (var effect in _cardData.Effects)
				{
					if (effect is BlockEffect blockEffect)
					{
						_valueLabel.Text = blockEffect.BlockAmount.ToString();
						valueStyle.BgColor = new Color(0.2f, 0.5f, 0.8f, 0.8f); // 蓝色底色用于防御
						break;
					}
				}
			}
			else
			{
				_valuePanel.Visible = false;
				_valueLabel.Visible = false;
			}
			
			if (_valuePanel.Visible)
			{
				valueStyle.BorderWidthBottom = valueStyle.BorderWidthLeft = valueStyle.BorderWidthRight = valueStyle.BorderWidthTop = 1;
				valueStyle.BorderColor = new Color(0.85f, 0.85f, 0.85f, 0.5f);
				valueStyle.CornerRadiusBottomRight = valueStyle.CornerRadiusTopLeft = valueStyle.CornerRadiusTopRight = 8;
				_valuePanel.AddThemeStyleboxOverride("panel", valueStyle);
			}
		}
	}
	
	// 获取卡牌类型对应的颜色
	private Color GetCardTypeColor(CardType type)
	{
		switch (type)
		{
			case CardType.Attack:
				return new Color(0.8f, 0.2f, 0.2f); // 红色
			case CardType.Defense:
				return new Color(0.2f, 0.6f, 0.8f); // 蓝色
			case CardType.Skill:
				return new Color(0.2f, 0.8f, 0.4f); // 绿色
			case CardType.Power:
				return new Color(0.8f, 0.4f, 0.8f); // 紫色
			default:
				return new Color(0.8f, 0.8f, 0.8f); // 灰色
		}
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
	
	// 处理输入事件
	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseEvent.Pressed)
				{
					// 开始拖拽
					_isDragging = true;
					_dragStartPosition = GlobalPosition;
					_mouseOffset = GetGlobalMousePosition() - _dragStartPosition;
					
					// 放大卡牌
					Scale = new Vector2(1.2f, 1.2f);
					PivotOffset = Size / 2;
					ZIndex = 100;
					
					GD.Print($"开始拖拽卡牌: {_cardData?.Name}");
					GetViewport().SetInputAsHandled();
				}
				else if (_isDragging)
				{
					// 结束拖拽
					_isDragging = false;
					
					// 还原卡牌大小
					Scale = new Vector2(1.0f, 1.0f);
					ZIndex = 0;
					
					// 检查是否应该使用卡牌
					Vector2 globalPos = GlobalPosition;
					GD.Print($"卡牌最终位置: {globalPos.Y}");
					
					if (globalPos.Y < 450) // 调整判定区域，增大Y值使判定区域下移
					{
						// 发射卡牌使用信号，在BattleSystem中判断是否有足够气力
						BattleSystem battleSystem = GetTree().Root.GetNode<BattleSystem>("Battle");
						if (battleSystem != null && _cardData != null)
						{
							if (battleSystem.CurrentEnergy < _cardData.Cost)
							{
								// 如果气力不足，直接返回原位
								GlobalPosition = _dragStartPosition;
								GD.Print("气力不足，卡牌返回原位");
								
								// 显示气力不足提示
								ShowEnergyWarning(battleSystem);
							}
							else
							{
								// 气力足够，发出使用信号
								EmitSignal(SignalName.CardPlayed, this);
							}
						}
						else
						{
							// 没有找到BattleSystem，返回原位
							GlobalPosition = _dragStartPosition;
						}
					}
					else
					{
						// 返回原位
						GlobalPosition = _dragStartPosition;
					}
					
					GetViewport().SetInputAsHandled();
				}
			}
		}
		else if (@event is InputEventMouseMotion motionEvent && _isDragging)
		{
			// 更新拖拽位置
			GlobalPosition = GetGlobalMousePosition() - _mouseOffset;
			GetViewport().SetInputAsHandled();
		}
	}
	
	// 显示气力不足警告
	private void ShowEnergyWarning(BattleSystem battleSystem)
	{
		// 播放气力不足音效
		battleSystem.PlayNoEnergySound();
		
		// 创建提示文本
		Label warningLabel = new Label();
		warningLabel.Text = "气力不足!";
		warningLabel.HorizontalAlignment = HorizontalAlignment.Center;
		warningLabel.VerticalAlignment = VerticalAlignment.Center;
		warningLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
		warningLabel.AddThemeFontSizeOverride("font_size", 24);
		
		// 设置提示位置
		warningLabel.GlobalPosition = battleSystem.GetNode<Control>("CardArea/EnergyPanel").GlobalPosition - new Vector2(0, 40);
		
		// 添加到场景中
		battleSystem.AddChild(warningLabel);
		
		// 创建动画
		Tween tween = battleSystem.CreateTween();
		tween.TweenProperty(warningLabel, "modulate", new Color(1, 1, 1, 0), 1.0f);
		tween.TweenCallback(Callable.From(() => warningLabel.QueueFree()));
	}
} 
