using Godot;
using System;

public partial class Options : Control
{
    // 节点引用
    private Button _backButton;
    private HSlider _musicVolumeSlider;
    private HSlider _sfxVolumeSlider;
    private CheckButton _fullscreenToggle;
    
    // 加载时调用
    public override void _Ready()
    {
        // 获取节点引用
        _backButton = GetNode<Button>("%BackButton");
        _musicVolumeSlider = GetNode<HSlider>("%MusicVolumeSlider");
        _sfxVolumeSlider = GetNode<HSlider>("%SFXVolumeSlider");
        _fullscreenToggle = GetNode<CheckButton>("%FullscreenToggle");
        
        // 连接信号
        _backButton.Pressed += OnBackButtonPressed;
        _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
        _sfxVolumeSlider.ValueChanged += OnSFXVolumeChanged;
        _fullscreenToggle.Toggled += OnFullscreenToggled;
        
        // 加载当前设置
        LoadSettings();
    }
    
    // 加载设置
    private void LoadSettings()
    {
        // 这里应该从配置文件或者全局设置中加载当前设置
        // 示例: 从配置文件加载音量设置
        float musicVolume = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(1)); // 假设音乐在总线1
        float sfxVolume = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(2));   // 假设音效在总线2
        
        _musicVolumeSlider.Value = musicVolume * 100; // 假设滑块范围是0-100
        _sfxVolumeSlider.Value = sfxVolume * 100;
        
        // 加载全屏设置
        _fullscreenToggle.ButtonPressed = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
    }
    
    // 保存设置
    private void SaveSettings()
    {
        // 这里应该将设置保存到配置文件或者全局设置中
        // 示例:
        float musicVolume = (float)_musicVolumeSlider.Value / 100;
        float sfxVolume = (float)_sfxVolumeSlider.Value / 100;
        
        AudioServer.SetBusVolumeDb(1, Mathf.LinearToDb(musicVolume)); // 音乐总线
        AudioServer.SetBusVolumeDb(2, Mathf.LinearToDb(sfxVolume));   // 音效总线
        
        // 可以添加保存到配置文件的代码
    }
    
    // 返回按钮处理
    private void OnBackButtonPressed()
    {
        // 保存设置
        SaveSettings();
        
        // 返回主菜单
        GetTree().ChangeSceneToFile("res://UI/MainMenuUI.tscn");
    }
    
    // 音乐音量改变
    private void OnMusicVolumeChanged(double value)
    {
        // 实时更新音乐音量
        float musicVolume = (float)value / 100;
        AudioServer.SetBusVolumeDb(1, Mathf.LinearToDb(musicVolume));
    }
    
    // 音效音量改变
    private void OnSFXVolumeChanged(double value)
    {
        // 实时更新音效音量
        float sfxVolume = (float)value / 100;
        AudioServer.SetBusVolumeDb(2, Mathf.LinearToDb(sfxVolume));
    }
    
    // 全屏切换
    private void OnFullscreenToggled(bool toggled)
    {
        // 切换全屏模式
        if (toggled)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
    }
} 