using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterAnimation : Control
{
    // 动画引用
    private AnimationPlayer _animationPlayer;
    private TextureRect _characterSprite;
    private TextureRect _blockIcon;
    private VBoxContainer _statusEffectsContainer;
    private Panel _characterPanel;
    
    // 角色引用
    private Character _character;
    
    // 状态效果图标
    private Dictionary<string, TextureRect> _statusIcons = new Dictionary<string, TextureRect>();
    
    // 是否正在展示攻击动画
    private bool _isAttacking = false;
    
    // 初始位置
    private Vector2 _initialPosition;
    
    // 准备就绪
    public override void _Ready()
    {
        // 获取组件引用
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _characterSprite = GetNode<TextureRect>("CharacterPanel/CharacterSprite");
        _blockIcon = GetNode<TextureRect>("BlockIcon");
        _statusEffectsContainer = GetNode<VBoxContainer>("StatusEffects");
        _characterPanel = GetNode<Panel>("CharacterPanel");
        
        // 记录初始位置
        _initialPosition = _characterPanel.Position;
    }
    
    // 设置角色数据
    public void SetCharacter(Character character)
    {
        _character = character;
        
        // 加载角色图像
        if (!string.IsNullOrEmpty(character.ImagePath) && ResourceLoader.Exists(character.ImagePath))
        {
            Texture2D texture = ResourceLoader.Load<Texture2D>(character.ImagePath);
            if (texture != null)
            {
                _characterSprite.Texture = texture;
            }
        }
        
        // 更新显示
        UpdateDisplay();
    }
    
    // 更新显示
    public void UpdateDisplay()
    {
        if (_character == null) return;
        
        // 更新格挡图标
        if (_character.Block > 0)
        {
            _blockIcon.Visible = true;
            // 可以设置格挡数值显示
        }
        else
        {
            _blockIcon.Visible = false;
        }
        
        // 更新状态效果图标
        UpdateStatusEffects();
    }
    
    // 更新状态效果图标
    private void UpdateStatusEffects()
    {
        if (_character == null) return;
        
        // 清空当前状态图标
        foreach (Node child in _statusEffectsContainer.GetChildren())
        {
            child.QueueFree();
        }
        _statusIcons.Clear();
        
        // 添加新的状态图标
        foreach (var effect in _character.StatusEffects)
        {
            string effectName = effect.Key;
            int effectStacks = effect.Value;
            
            if (effectStacks <= 0) continue;
            
            // 创建状态效果图标
            HBoxContainer container = new HBoxContainer();
            
            TextureRect icon = new TextureRect();
            icon.CustomMinimumSize = new Vector2(24, 24);
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            
            // 根据状态名称加载图标
            string iconPath = $"res://Resources/Images/Icons/{effectName.ToLower()}.png";
            if (ResourceLoader.Exists(iconPath))
            {
                icon.Texture = ResourceLoader.Load<Texture2D>(iconPath);
            }
            
            Label stacksLabel = new Label();
            stacksLabel.Text = effectStacks.ToString();
            
            container.AddChild(icon);
            container.AddChild(stacksLabel);
            
            _statusEffectsContainer.AddChild(container);
            _statusIcons[effectName] = icon;
        }
    }
    
    // 播放攻击动画
    public void PlayAttackAnimation()
    {
        if (_isAttacking) return;
        _isAttacking = true;
        
        // 如果找不到AnimationPlayer，创建一个简单的移动动画
        if (_animationPlayer == null || !_animationPlayer.HasAnimation("attack"))
        {
            Vector2 attackPosition = _initialPosition;
            
            // 向前移动（玩家向右，敌人向左）
            if (_character != null && _character.IsPlayer)
            {
                attackPosition.X += 30;
            }
            else
            {
                attackPosition.X -= 30;
            }
            
            // 创建补间动画
            Tween tween = CreateTween();
            tween.TweenProperty(_characterPanel, "position", attackPosition, 0.1f);
            tween.TweenProperty(_characterPanel, "position", _initialPosition, 0.2f);
            tween.Finished += () => _isAttacking = false;
        }
        else
        {
            _animationPlayer.Play("attack");
            _animationPlayer.AnimationFinished += (stringName) => _isAttacking = false;
        }
    }
    
    // 播放受击动画
    public void PlayHitAnimation()
    {
        // 如果找不到AnimationPlayer，创建一个简单的晃动动画
        if (_animationPlayer == null || !_animationPlayer.HasAnimation("hit"))
        {
            // 创建晃动效果
            Tween tween = CreateTween();
            
            // 角色左右晃动
            for (int i = 0; i < 4; i++)
            {
                Vector2 shakeOffset = new Vector2((i % 2 == 0) ? 5 : -5, 0);
                tween.TweenProperty(_characterPanel, "position", _initialPosition + shakeOffset, 0.05f);
            }
            
            // 回到初始位置
            tween.TweenProperty(_characterPanel, "position", _initialPosition, 0.05f);
            
            // 添加闪烁效果
            Color normalColor = _characterPanel.Modulate;
            Color hitColor = new Color(1, 0.5f, 0.5f);
            
            tween.Parallel().TweenProperty(_characterPanel, "modulate", hitColor, 0.1f);
            tween.TweenProperty(_characterPanel, "modulate", normalColor, 0.2f);
        }
        else
        {
            _animationPlayer.Play("hit");
        }
    }
    
    // 播放死亡动画
    public void PlayDeathAnimation()
    {
        // 如果找不到AnimationPlayer，创建一个简单的淡出动画
        if (_animationPlayer == null || !_animationPlayer.HasAnimation("death"))
        {
            Tween tween = CreateTween();
            tween.TweenProperty(_characterPanel, "modulate:a", 0.0f, 0.5f);
        }
        else
        {
            _animationPlayer.Play("death");
        }
    }
} 