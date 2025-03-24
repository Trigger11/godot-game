using Godot;
using System;

public partial class OptionsManager : Control
{
    // UI控件
    private HSlider _musicVolumeSlider;
    private HSlider _sfxVolumeSlider;
    private CheckBox _musicEnabledCheckbox;
    private CheckBox _sfxEnabledCheckbox;
    private Button _backButton;
    
    // GameManager引用
    private GameManager _gameManager;
    
    public override void _Ready()
    {
        // 获取GameManager引用
        _gameManager = GameManager.Instance;
        
        // 初始化UI控件
        _musicVolumeSlider = GetNode<HSlider>("Panel/VBoxContainer/MusicVolumeContainer/MusicVolumeSlider");
        _sfxVolumeSlider = GetNode<HSlider>("Panel/VBoxContainer/SFXVolumeContainer/SFXVolumeSlider");
        _musicEnabledCheckbox = GetNode<CheckBox>("Panel/VBoxContainer/MusicEnabledContainer/MusicEnabledCheckbox");
        _sfxEnabledCheckbox = GetNode<CheckBox>("Panel/VBoxContainer/SFXEnabledContainer/SFXEnabledCheckbox");
        _backButton = GetNode<Button>("Panel/VBoxContainer/BackButton");
        
        // 设置初始值
        if (_gameManager != null)
        {
            _musicVolumeSlider.Value = _gameManager.GetMusicVolume() * 100;
            _sfxVolumeSlider.Value = _gameManager.GetSfxVolume() * 100;
            _musicEnabledCheckbox.ButtonPressed = _gameManager.IsMusicEnabled();
            _sfxEnabledCheckbox.ButtonPressed = _gameManager.IsSfxEnabled();
        }
        
        // 连接信号
        _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
        _sfxVolumeSlider.ValueChanged += OnSfxVolumeChanged;
        _musicEnabledCheckbox.Toggled += OnMusicEnabledToggled;
        _sfxEnabledCheckbox.Toggled += OnSfxEnabledToggled;
        _backButton.Pressed += OnBackButtonPressed;
        
        // 播放测试音效
        if (_gameManager != null && _gameManager.IsSfxEnabled())
        {
            _gameManager.PlaySoundEffect("res://Resources/Audio/SFX/ui_open.wav");
        }
    }
    
    private void OnMusicVolumeChanged(double value)
    {
        if (_gameManager != null)
        {
            float volume = (float)value / 100f;
            _gameManager.SetMusicVolume(volume);
        }
    }
    
    private void OnSfxVolumeChanged(double value)
    {
        if (_gameManager != null)
        {
            float volume = (float)value / 100f;
            _gameManager.SetSfxVolume(volume);
            
            // 播放测试音效
            if (_gameManager.IsSfxEnabled())
            {
                _gameManager.PlaySoundEffect("res://Resources/Audio/SFX/ui_click.wav");
            }
        }
    }
    
    private void OnMusicEnabledToggled(bool toggled)
    {
        if (_gameManager != null)
        {
            _gameManager.EnableMusic(toggled);
        }
    }
    
    private void OnSfxEnabledToggled(bool toggled)
    {
        if (_gameManager != null)
        {
            _gameManager.EnableSfx(toggled);
            
            // 播放测试音效
            if (toggled)
            {
                _gameManager.PlaySoundEffect("res://Resources/Audio/SFX/ui_click.wav");
            }
        }
    }
    
    private void OnBackButtonPressed()
    {
        if (_gameManager != null)
        {
            // 播放按钮音效
            if (_gameManager.IsSfxEnabled())
            {
                _gameManager.PlaySoundEffect("res://Resources/Audio/SFX/ui_back.wav");
            }
            
            // 返回上一个场景
            _gameManager.NavigateToScene("Game");
        }
    }
} 