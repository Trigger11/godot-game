using Godot;
using System;
using System.IO;
using System.Collections.Generic;

public partial class LoadGameUI : Control
{
    // 节点引用
    private VBoxContainer _saveGameList;
    private Button _loadButton;
    private Button _deleteButton;
    private Button _backButton;
    private Label _noSavesLabel;
    
    // 存档数据
    private List<string> _saveFiles = new List<string>();
    private int _selectedSaveIndex = -1;
    
    private const string SAVE_DIRECTORY = "user://saves/";
    
    // 加载时调用
    public override void _Ready()
    {
        // 获取UI元素引用
        _saveGameList = GetNode<VBoxContainer>("%SaveGameList");
        _loadButton = GetNode<Button>("%LoadButton");
        _deleteButton = GetNode<Button>("%DeleteButton");
        _backButton = GetNode<Button>("%BackButton");
        _noSavesLabel = GetNode<Label>("%NoSavesLabel");
        
        // 连接信号
        _loadButton.Pressed += OnLoadButtonPressed;
        _deleteButton.Pressed += OnDeleteButtonPressed;
        _backButton.Pressed += OnBackButtonPressed;
        
        // 初始状态禁用按钮
        _loadButton.Disabled = true;
        _deleteButton.Disabled = true;
        
        // 加载存档列表
        LoadSaveFiles();
    }
    
    // 加载存档文件列表
    private void LoadSaveFiles()
    {
        // 清除之前的列表
        foreach (Node child in _saveGameList.GetChildren())
        {
            if (child != _noSavesLabel)
            {
                child.QueueFree();
            }
        }
        
        _saveFiles.Clear();
        _selectedSaveIndex = -1;
        
        // 确保保存目录存在
        DirAccess dir = DirAccess.Open("user://");
        if (!dir.DirExists("saves"))
        {
            dir.MakeDir("saves");
        }
        
        // 获取所有存档文件
        dir = DirAccess.Open(SAVE_DIRECTORY);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            
            while (!string.IsNullOrEmpty(fileName))
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".save"))
                {
                    _saveFiles.Add(fileName);
                }
                fileName = dir.GetNext();
            }
        }
        
        // 显示存档列表或提示
        if (_saveFiles.Count > 0)
        {
            _noSavesLabel.Visible = false;
            
            // 添加每个存档到列表
            for (int i = 0; i < _saveFiles.Count; i++)
            {
                string saveFile = _saveFiles[i];
                AddSaveItemToList(saveFile, i);
            }
        }
        else
        {
            _noSavesLabel.Visible = true;
        }
    }
    
    // 向列表添加存档项
    private void AddSaveItemToList(string saveFileName, int saveIndex)
    {
        // 提取存档名称（去掉.save后缀）
        string displayName = saveFileName.Substring(0, saveFileName.Length - 5);
        
        // 创建存档项按钮
        Button saveButton = new Button();
        saveButton.Text = displayName;
        saveButton.ToggleMode = true;
        saveButton.SizeFlagsHorizontal = SizeFlags.Fill;
        saveButton.ThemeTypeVariation = "SaveListItem";
        
        // 设置字体和颜色
        saveButton.AddThemeColorOverride("font_color", new Color(0.839f, 0.788f, 0.647f));
        saveButton.AddThemeFontSizeOverride("font_size", 22);
        
        // 连接选择信号
        saveButton.Toggled += (toggled) => OnSaveItemToggled(saveIndex, toggled);
        
        // 添加到列表
        _saveGameList.AddChild(saveButton);
    }
    
    // 处理存档项选择
    private void OnSaveItemToggled(int index, bool toggled)
    {
        // 如果这个项被选中
        if (toggled)
        {
            // 取消其他项的选择
            for (int i = 0; i < _saveGameList.GetChildCount(); i++)
            {
                if (i != index + 1) // +1 因为第一个子节点是NoSavesLabel
                {
                    Node child = _saveGameList.GetChild(i);
                    if (child is Button button && button.ButtonPressed)
                    {
                        button.ButtonPressed = false;
                    }
                }
            }
            
            // 记录选中项
            _selectedSaveIndex = index;
            
            // 启用按钮
            _loadButton.Disabled = false;
            _deleteButton.Disabled = false;
        }
        else if (_selectedSaveIndex == index)
        {
            // 如果取消选择当前项
            _selectedSaveIndex = -1;
            
            // 禁用按钮
            _loadButton.Disabled = true;
            _deleteButton.Disabled = true;
        }
    }
    
    // 处理加载按钮点击
    private void OnLoadButtonPressed()
    {
        if (_selectedSaveIndex >= 0 && _selectedSaveIndex < _saveFiles.Count)
        {
            string saveFile = _saveFiles[_selectedSaveIndex];
            GD.Print($"加载存档: {saveFile}");
            
            // 加载存档数据
            // 这里需要调用GameManager或类似的单例来处理加载
            // GameManager.Instance.LoadGame(SAVE_DIRECTORY + saveFile);
            
            // 转到游戏主场景
            GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
        }
    }
    
    // 处理删除按钮点击
    private void OnDeleteButtonPressed()
    {
        if (_selectedSaveIndex >= 0 && _selectedSaveIndex < _saveFiles.Count)
        {
            string saveFile = _saveFiles[_selectedSaveIndex];
            
            // 删除存档文件
            DirAccess dir = DirAccess.Open(SAVE_DIRECTORY);
            if (dir != null && dir.FileExists(saveFile))
            {
                dir.Remove(saveFile);
                GD.Print($"删除存档: {saveFile}");
            }
            
            // 重新加载存档列表
            LoadSaveFiles();
        }
    }
    
    // 处理返回按钮点击
    private void OnBackButtonPressed()
    {
        // 返回主菜单
        GetTree().ChangeSceneToFile("res://UI/MainMenuUI.tscn");
    }
} 