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
        // 获取GameManager引用
        _gameManager = GetNode<GameManager>("/root/GameManager");
        
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
        }
    }
    
    private void ApplyMinuteCultivation()
    {
        _cultivationTimeMinutes += 1.0f;
        
        // 每分钟更新一次
        if (_cultivationTimeMinutes % 1.0f < 0.01f)
        {
            var player = _gameManager.PlayerData;
            
            // 累积修炼时间，每分钟的奖励在结束修炼时计算
            
            // 更新UI显示
            UpdateAttributes();
            UpdateExpProgress();
        }
    }
    
    private void ApplyCultivationResults()
    {
        // 修炼结果只在结束修炼时应用
        var player = _gameManager.PlayerData;
        
        // 按修炼类型和时间增加相应属性
        switch (_selectedCultivationType)
        {
            case CultivationType.Qi:
                // 主修气力，增加神识
                player.AddAttributePoints("气力", (int)(QI_CULTIVATION_QI_BONUS * _cultivationTimeMinutes));
                player.AddAttributePoints("神识", (int)(QI_CULTIVATION_SPIRIT_BONUS * _cultivationTimeMinutes));
                break;
                
            case CultivationType.Spirit:
                // 主修神识，增加气力
                player.AddAttributePoints("神识", (int)(SPIRIT_CULTIVATION_SPIRIT_BONUS * _cultivationTimeMinutes));
                player.AddAttributePoints("气力", (int)(SPIRIT_CULTIVATION_QI_BONUS * _cultivationTimeMinutes));
                break;
                
            case CultivationType.Body:
                // 主修体魄，增加气力
                player.AddAttributePoints("体魄", (int)(BODY_CULTIVATION_BODY_BONUS * _cultivationTimeMinutes));
                player.AddAttributePoints("气力", (int)(BODY_CULTIVATION_QI_BONUS * _cultivationTimeMinutes));
                break;
        }
        
        // 获得修炼经验
        int expGain = CalculateExperienceGain(_cultivationTimeMinutes);
        player.AddExperience(expGain);
        
        // 清零修炼时间
        _cultivationTimeMinutes = 0;
        
        // 更新显示
        UpdateAttributes();
        UpdateExpProgress();
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