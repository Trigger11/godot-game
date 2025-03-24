using Godot;
using System;
using System.Collections.Generic;

public partial class CultivationSystem : Node
{
	// 引用节点
	private GameManager _gameManager;
	private Timer _cultivationTimer;
	
	// UI元素
	private Label _qiPowerValue;
	private Label _spiritValue;
	private Label _bodyValue;
	private Label _fateValue;
	private Label _selectedTechniqueLabel;
	private Label _cultivationSpeedLabel;
	private Label _experienceGainLabel;
	private ProgressBar _expProgress;
	private Label _progressLabel;
	private Button _startCultivationButton;
	private VBoxContainer _techniquesList;
	
	// 修炼相关属性
	private bool _isCultivating = false;
	private float _cultivationTimeMinutes = 0;
	private CultivationType _selectedCultivationType = CultivationType.Qi;
	private Technique _selectedTechnique;
	
	// 修炼属性加成
	private const int QI_CULTIVATION_QI_BONUS = 3;
	private const int QI_CULTIVATION_SPIRIT_BONUS = 1;
	
	private const int SPIRIT_CULTIVATION_SPIRIT_BONUS = 3;
	private const int SPIRIT_CULTIVATION_QI_BONUS = 1;
	
	private const int BODY_CULTIVATION_BODY_BONUS = 3;
	private const int BODY_CULTIVATION_QI_BONUS = 1;
	
	// 经验获取配置
	private const int BASE_EXP_PER_MINUTE = 10;
	
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
		
		// 获取Timer引用
		_cultivationTimer = GetNode<Timer>("Timer");
		_cultivationTimer.WaitTime = 1.0f; // 每秒更新一次
		_cultivationTimer.Timeout += OnCultivationTimerTimeout;
		
		// 获取UI元素引用
		_qiPowerValue = GetNode<Label>("MainContent/HSplitContainer/LeftPanel/VBoxContainer/HBoxContainer/AttributeValues/QiPowerValue");
		_spiritValue = GetNode<Label>("MainContent/HSplitContainer/LeftPanel/VBoxContainer/HBoxContainer/AttributeValues/SpiritValue");
		_bodyValue = GetNode<Label>("MainContent/HSplitContainer/LeftPanel/VBoxContainer/HBoxContainer/AttributeValues/BodyValue");
		_fateValue = GetNode<Label>("MainContent/HSplitContainer/LeftPanel/VBoxContainer/HBoxContainer/AttributeValues/FateValue");
		
		_selectedTechniqueLabel = GetNode<Label>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/EffectsPanel/MarginContainer/VBoxContainer/SelectedTechniqueLabel");
		_cultivationSpeedLabel = GetNode<Label>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/EffectsPanel/MarginContainer/VBoxContainer/CultivationSpeedLabel");
		_experienceGainLabel = GetNode<Label>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/EffectsPanel/MarginContainer/VBoxContainer/ExperienceGainLabel");
		_expProgress = GetNode<ProgressBar>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/EffectsPanel/MarginContainer/VBoxContainer/ExpProgress");
		_progressLabel = GetNode<Label>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/EffectsPanel/MarginContainer/VBoxContainer/ExpProgress/ProgressLabel");
		
		_startCultivationButton = GetNode<Button>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/StartCultivationButton");
		_techniquesList = GetNode<VBoxContainer>("MainContent/HSplitContainer/LeftPanel/VBoxContainer/TechniquesList");
		
		// 绑定按钮事件
		_startCultivationButton.Pressed += OnStartCultivationButtonPressed;
		GetNode<Button>("TopPanel/BackButton").Pressed += OnBackButtonPressed;
		
		// 绑定修炼方式选择事件
		GetNode<Button>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/CultivationOptionsContainer/QiCultivation").Pressed += () => SetCultivationType(CultivationType.Qi);
		GetNode<Button>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/CultivationOptionsContainer/SpiritCultivation").Pressed += () => SetCultivationType(CultivationType.Spirit);
		GetNode<Button>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/CultivationOptionsContainer/BodyCultivation").Pressed += () => SetCultivationType(CultivationType.Body);
		
		// 初始化
		InitializeUI();
	}
	
	private void InitializeUI()
	{
		// 清空并重新填充功法列表
		foreach (Node child in _techniquesList.GetChildren())
		{
			child.QueueFree();
		}
		
		foreach (var techniqueName in _gameManager.PlayerData.GetLearnedTechniques())
		{
			var technique = _gameManager.PlayerData.GetTechnique(techniqueName);
			if (technique != null && technique.Type == TechniqueType.QiCultivation || 
				technique.Type == TechniqueType.BodyRefining || 
				technique.Type == TechniqueType.SpiritCultivation)
			{
				Button techniqueButton = new Button();
				techniqueButton.Text = technique.Name;
				techniqueButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				techniqueButton.Alignment = HorizontalAlignment.Left;
				
				// 样式设置
				techniqueButton.AddThemeColorOverride("font_color", new Color(0.84f, 0.76f, 0.61f));
				techniqueButton.AddThemeColorOverride("font_hover_color", new Color(0.99f, 0.92f, 0.80f));
				techniqueButton.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://Resources/Fonts/XiaoKeFont.ttf"));
				techniqueButton.AddThemeFontSizeOverride("font_size", 20);
				
				// 设置样式boxflat
				var styleNormal = new StyleBoxFlat();
				styleNormal.BgColor = new Color(0.16f, 0.24f, 0.29f, 0.78f);
				styleNormal.BorderWidthLeft = 2;
				styleNormal.BorderWidthTop = 2;
				styleNormal.BorderWidthRight = 2;
				styleNormal.BorderWidthBottom = 2;
				styleNormal.BorderColor = new Color(0.32f, 0.44f, 0.54f, 1.0f);
				styleNormal.CornerRadiusTopLeft = 5;
				styleNormal.CornerRadiusTopRight = 5;
				styleNormal.CornerRadiusBottomRight = 5;
				styleNormal.CornerRadiusBottomLeft = 5;
				techniqueButton.AddThemeStyleboxOverride("normal", styleNormal);
				
				var styleHover = new StyleBoxFlat();
				styleHover.BgColor = new Color(0.24f, 0.19f, 0.13f, 0.78f);
				styleHover.BorderWidthLeft = 2;
				styleHover.BorderWidthTop = 2;
				styleHover.BorderWidthRight = 2;
				styleHover.BorderWidthBottom = 2;
				styleHover.BorderColor = new Color(0.69f, 0.56f, 0.37f, 1.0f);
				styleHover.CornerRadiusTopLeft = 5;
				styleHover.CornerRadiusTopRight = 5;
				styleHover.CornerRadiusBottomRight = 5;
				styleHover.CornerRadiusBottomLeft = 5;
				techniqueButton.AddThemeStyleboxOverride("hover", styleHover);
				
				// 添加事件
				techniqueButton.Pressed += () => SelectTechnique(technique);
				
				_techniquesList.AddChild(techniqueButton);
			}
		}
		
		// 如果有功法，选择第一个作为默认功法
		if (_techniquesList.GetChildCount() > 0)
		{
			_selectedTechnique = _gameManager.PlayerData.GetTechnique(_gameManager.PlayerData.GetLearnedTechniques()[0]);
		}
		UpdateTechniqueDisplay();
		
		// 更新属性显示
		UpdateAttributes();
		
		// 更新经验进度条
		UpdateExpProgress();
	}
	
	private void UpdateAttributes()
	{
		var player = _gameManager.PlayerData;
		
		// 更新属性显示
		_qiPowerValue.Text = player.GetAttribute("气力").ToString();
		_spiritValue.Text = player.GetAttribute("神识").ToString();
		_bodyValue.Text = player.GetAttribute("体魄").ToString();
		_fateValue.Text = player.GetAttribute("命运").ToString();
	}
	
	private void UpdateTechniqueDisplay()
	{
		if (_selectedTechnique != null)
		{
			_selectedTechniqueLabel.Text = $"已选择：{_selectedTechnique.Name}";
			// 假设修炼速度来自于功法的属性加成总和
			int speedBonus = 0;
			foreach (var bonus in _selectedTechnique.AttributeBonus)
			{
				speedBonus += bonus.Value;
			}
			_cultivationSpeedLabel.Text = $"修炼速度：{BASE_EXP_PER_MINUTE + speedBonus} 经验/分钟";
		}
		else
		{
			_selectedTechniqueLabel.Text = "未选择功法";
			_cultivationSpeedLabel.Text = "修炼速度：0 经验/分钟";
		}
	}
	
	private void UpdateExpProgress()
	{
		var player = _gameManager.PlayerData;
		int requiredExp = player.GetRequiredExperienceForNextLevel();
		
		_expProgress.MaxValue = requiredExp;
		_expProgress.Value = player.Experience;
		
		_progressLabel.Text = $"{player.Experience}/{requiredExp}";
		_experienceGainLabel.Text = $"当前等级：{player.Level} 经验获取：{CalculateExperienceGain(1.0f)} 点/分钟";
	}
	
	private void SelectTechnique(Technique technique)
	{
		_selectedTechnique = technique;
		UpdateTechniqueDisplay();
		
		// 为选中的技能添加视觉反馈
		foreach (Node child in _techniquesList.GetChildren())
		{
			if (child is Button button)
			{
				// 重置所有按钮样式
				var normalStyle = new StyleBoxFlat();
				normalStyle.BgColor = new Color(0.16f, 0.24f, 0.29f, 0.78f);
				normalStyle.BorderWidthLeft = 2;
				normalStyle.BorderWidthTop = 2;
				normalStyle.BorderWidthRight = 2;
				normalStyle.BorderWidthBottom = 2;
				normalStyle.BorderColor = new Color(0.32f, 0.44f, 0.54f, 1.0f);
				normalStyle.CornerRadiusTopLeft = normalStyle.CornerRadiusTopRight = 
				normalStyle.CornerRadiusBottomLeft = normalStyle.CornerRadiusBottomRight = 5;
				button.AddThemeStyleboxOverride("normal", normalStyle);
				
				// 如果是当前选中的技能，应用高亮样式
				if (button.Text == technique.Name)
				{
					var selectedStyle = new StyleBoxFlat();
					selectedStyle.BgColor = new Color(0.24f, 0.35f, 0.17f, 0.78f);
					selectedStyle.BorderWidthLeft = selectedStyle.BorderWidthTop = 
					selectedStyle.BorderWidthRight = selectedStyle.BorderWidthBottom = 2;
					selectedStyle.BorderColor = new Color(0.69f, 0.86f, 0.37f, 1.0f);
					selectedStyle.CornerRadiusTopLeft = selectedStyle.CornerRadiusTopRight = 
					selectedStyle.CornerRadiusBottomLeft = selectedStyle.CornerRadiusBottomRight = 5;
					button.AddThemeStyleboxOverride("normal", selectedStyle);
				}
			}
		}
		
		// 显示技能详情
		CreateStatusLabelIfNeeded();
		RichTextLabel statusLabel = GetNode<RichTextLabel>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/StatusLabel");
		statusLabel.Text = $"[color=#E5B96A]选择了修炼功法：{technique.Name}[/color]\n{technique.Description}";
	}
	
	private void SetCultivationType(CultivationType type)
	{
		_selectedCultivationType = type;
		
		// 可以在这里添加UI反馈，比如高亮选中的按钮
	}
	
	private void OnStartCultivationButtonPressed()
	{
		if (_isCultivating)
		{
			GD.Print("停止修炼");
			StopCultivation();
		}
		else
		{
			GD.Print("开始修炼");
			StartCultivation();
		}
	}
	
	private void StartCultivation()
	{
		if (_selectedTechnique == null)
		{
			GD.PushWarning("请先选择一种修炼功法");
			return;
		}
		
		_isCultivating = true;
		_cultivationTimeMinutes = 0;
		_cultivationTimer.Start();
		_startCultivationButton.Text = "停止修炼";
	}
	
	private void StopCultivation()
	{
		_isCultivating = false;
		_cultivationTimer.Stop();
		_startCultivationButton.Text = "开始修炼";
		
		// 应用修炼结果
		ApplyCultivationResults();
	}
	
	private void OnCultivationTimerTimeout()
	{
		if (_isCultivating)
		{
			// 每秒累积1/60分钟
			_cultivationTimeMinutes += 1.0f / 60.0f;
			
			// 每分钟应用一次效果
			if (_cultivationTimeMinutes >= 1.0f)
			{
				// 修炼1分钟
				ApplyMinuteCultivation();
				_cultivationTimeMinutes -= 1.0f;
			}
			
			// 更新UI显示修炼进度
			UpdateCultivationProgress();
		}
	}
	
	private void UpdateCultivationProgress()
	{
		// 创建并更新状态标签，显示当前修炼状态
		CreateStatusLabelIfNeeded();
		RichTextLabel statusLabel = GetNode<RichTextLabel>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/StatusLabel");
		
		int minutes = (int)_cultivationTimeMinutes;
		int seconds = (int)((_cultivationTimeMinutes - minutes) * 60);
		
		statusLabel.Text = $"[color=#E5B96A]正在修炼中...[/color]\n修炼时间: {minutes}分{seconds}秒";
		
		// 更新经验进度条，实时显示经验增长
		var player = _gameManager.PlayerData;
		int expGain = CalculateExperienceGain(_cultivationTimeMinutes);
		
		// 模拟经验增长，但不实际添加到玩家数据，直到修炼结束
		_expProgress.Value = player.Experience + expGain;
		_progressLabel.Text = $"{(int)_expProgress.Value}/{(int)_expProgress.MaxValue}";
	}
	
	private void ApplyMinuteCultivation()
	{
		var player = _gameManager.PlayerData;
		
		// 根据修炼类型，实时小幅度增加属性，让玩家看到即时反馈
		// 这是为了UI反馈，真正的大量属性增长在结束修炼时计算
		switch (_selectedCultivationType)
		{
			case CultivationType.Qi:
				player.AddAttributePoints("气力", 1);
				break;
			case CultivationType.Spirit:
				player.AddAttributePoints("神识", 1);
				break;
			case CultivationType.Body:
				player.AddAttributePoints("体魄", 1);
				break;
		}
		
		// 更新属性显示
		UpdateAttributes();
	}
	
	private void ApplyCultivationResults()
	{
		// 修炼结果只在结束修炼时应用
		var player = _gameManager.PlayerData;
		
		// 计算属性增长
		int qiGain = 0, spiritGain = 0, bodyGain = 0;
		
		// 按修炼类型和时间增加相应属性
		switch (_selectedCultivationType)
		{
			case CultivationType.Qi:
				// 主修气力，增加神识
				qiGain = (int)(QI_CULTIVATION_QI_BONUS * _cultivationTimeMinutes);
				spiritGain = (int)(QI_CULTIVATION_SPIRIT_BONUS * _cultivationTimeMinutes);
				player.AddAttributePoints("气力", qiGain);
				player.AddAttributePoints("神识", spiritGain);
				break;
				
			case CultivationType.Spirit:
				// 主修神识，增加气力
				spiritGain = (int)(SPIRIT_CULTIVATION_SPIRIT_BONUS * _cultivationTimeMinutes);
				qiGain = (int)(SPIRIT_CULTIVATION_QI_BONUS * _cultivationTimeMinutes);
				player.AddAttributePoints("神识", spiritGain);
				player.AddAttributePoints("气力", qiGain);
				break;
				
			case CultivationType.Body:
				// 主修体魄，增加气力
				bodyGain = (int)(BODY_CULTIVATION_BODY_BONUS * _cultivationTimeMinutes);
				qiGain = (int)(BODY_CULTIVATION_QI_BONUS * _cultivationTimeMinutes);
				player.AddAttributePoints("体魄", bodyGain);
				player.AddAttributePoints("气力", qiGain);
				break;
		}
		
		// 获得修炼经验
		int expGain = CalculateExperienceGain(_cultivationTimeMinutes);
		int oldExperience = player.Experience;
		player.AddExperience(expGain);
		
		// 显示修炼结果消息
		CreateStatusLabelIfNeeded();
		RichTextLabel statusLabel = GetNode<RichTextLabel>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer/StatusLabel");
		
		string resultText = $"[color=#E5B96A]修炼结束！[/color]\n获得了 {expGain} 点经验值\n";
		
		// 显示属性增长详情
		if (qiGain > 0) resultText += $"气力增加了 {qiGain} 点\n";
		if (spiritGain > 0) resultText += $"神识增加了 {spiritGain} 点\n";
		if (bodyGain > 0) resultText += $"体魄增加了 {bodyGain} 点\n";
		
		// 检查是否升级
		int oldLevel = player.Level;
		if (player.Experience >= player.GetRequiredExperienceForNextLevel() && oldExperience < player.GetRequiredExperienceForNextLevel())
		{
			// 由于CheckLevelUp是私有方法，我们需要让系统自动检查升级
			player.AddExperience(0); // 触发内部的升级检查
			
			// 检查是否升级了
			if (player.Level > oldLevel)
			{
				resultText += $"[color=#FFD700]突破成功！修为提升到了 {player.Level} 级！[/color]\n";
				
				// 播放突破动画或效果
				PlayBreakthroughEffect();
			}
		}
		
		// 随机悟道事件 - 有小几率在修炼中领悟新功法或技能
		Random random = new Random();
		if (random.NextDouble() < 0.05 && _cultivationTimeMinutes >= 10) // 5%几率，且修炼时间至少10分钟
		{
			resultText += "[color=#7FFFD4]修炼中你有所领悟，获得了新的启发！[/color]\n";
			// 这里可以添加学习新技能的代码
		}
		
		statusLabel.Text = resultText;
		
		// 清零修炼时间
		_cultivationTimeMinutes = 0;
		
		// 更新显示
		UpdateAttributes();
		UpdateExpProgress();
	}
	
	// 播放突破效果
	private void PlayBreakthroughEffect()
	{
		// 使用简化的动画效果而不是粒子系统
		// 创建一个临时的控制节点来显示突破效果
		var effectsContainer = GetNode<Control>("MainContent/HSplitContainer/RightPanel");
		
		// 创建一个发光的标签
		var label = new Label();
		label.Text = "✧突破✧";
		label.HorizontalAlignment = HorizontalAlignment.Center;
		label.VerticalAlignment = VerticalAlignment.Center;
		label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		label.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		label.AddThemeColorOverride("font_color", new Color(1.0f, 0.8f, 0.0f)); // 金色
		label.AddThemeFontSizeOverride("font_size", 48);
		label.Position = new Vector2(effectsContainer.Size.X / 2 - 100, effectsContainer.Size.Y / 2 - 50);
		
		// 添加到场景
		effectsContainer.AddChild(label);
		
		// 创建一个计时器来改变标签的透明度并最终移除它
		float elapsed = 0;
		float duration = 2.0f; // 2秒动画
		
		Timer timer = new Timer();
		timer.WaitTime = 0.05f; // 每50毫秒更新一次
		timer.Timeout += () => 
		{
			elapsed += 0.05f;
			if (elapsed < duration)
			{
				// 让标签缓慢淡出
				float alpha = 1.0f - (elapsed / duration);
				label.Modulate = new Color(1.0f, 0.8f, 0.0f, alpha);
				
				// 让标签变大
				float scale = 1.0f + elapsed;
				label.Scale = new Vector2(scale, scale);
			}
			else
			{
				// 动画结束，移除标签和计时器
				label.QueueFree();
				timer.QueueFree();
			}
		};
		
		effectsContainer.AddChild(timer);
		timer.Start();
		
		// 添加音效效果（如果需要）
		// AudioStreamPlayer audioPlayer = new AudioStreamPlayer();
		// audioPlayer.Stream = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/breakthrough.wav");
		// audioPlayer.Play();
	}
	
	private int CalculateExperienceGain(float minutes)
	{
		int baseExp = (int)(BASE_EXP_PER_MINUTE * minutes);
		
		// 根据选择的功法获取经验加成
		float expMultiplier = 1.0f;
		if (_selectedTechnique != null)
		{
			// 获取功法的经验加成总和
			foreach (var bonus in _selectedTechnique.AttributeBonus)
			{
				expMultiplier += bonus.Value * 0.1f; // 每点属性加成增加10%的经验
			}
		}
		
		// 应用临时修炼效率加成
		if (_gameManager.PlayerData.HasTemporaryBonus("修炼效率"))
		{
			expMultiplier += _gameManager.PlayerData.GetTemporaryBonus("修炼效率");
		}
		
		return (int)(baseExp * expMultiplier);
	}
	
	private void OnBackButtonPressed()
	{
		// 如果正在修炼，先停止
		if (_isCultivating)
		{
			StopCultivation();
		}
		
		// 返回主游戏界面
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}
	
	// 创建状态标签
	private void CreateStatusLabelIfNeeded()
	{
		var container = GetNode<VBoxContainer>("MainContent/HSplitContainer/RightPanel/MarginContainer/VBoxContainer");
		
		// 检查状态标签是否存在
		if (container.GetNodeOrNull("StatusLabel") == null)
		{
			// 创建状态标签
			RichTextLabel statusLabel = new RichTextLabel();
			statusLabel.Name = "StatusLabel";
			statusLabel.BbcodeEnabled = true;
			statusLabel.FitContent = true;
			statusLabel.CustomMinimumSize = new Vector2(0, 40);
			statusLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			statusLabel.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
			
			// 设置样式
			statusLabel.AddThemeFontOverride("normal_font", ResourceLoader.Load<Font>("res://Resources/Fonts/XiaoKeFont.ttf"));
			statusLabel.AddThemeFontSizeOverride("normal_font_size", 20);
			
			// 在EffectsPanel之前插入
			int effectsPanelIndex = -1;
			for (int i = 0; i < container.GetChildCount(); i++)
			{
				if (container.GetChild(i).Name == "EffectsPanel")
				{
					effectsPanelIndex = i;
					break;
				}
			}
			
			container.AddChild(statusLabel);
			
			if (effectsPanelIndex >= 0)
			{
				container.MoveChild(statusLabel, effectsPanelIndex);
			}
		}
	}
}

// 修炼类型枚举
public enum CultivationType
{
	Qi,     // 气元修炼
	Spirit, // 心神修炼
	Body    // 炼体修炼
} 
