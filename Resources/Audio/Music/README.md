# 游戏背景音乐

此目录包含游戏中使用的所有背景音乐文件。

## 音乐文件命名规范

音乐文件使用descriptive_name.mp3格式命名，例如：

- main_theme.ogg - 主菜单音乐
- peaceful_theme.mp3 - 平静场景背景音乐
- battle_theme.mp3 - 战斗场景背景音乐
- cultivation_theme.mp3 - 修炼场景背景音乐
- divination_theme.mp3 - 占卜场景背景音乐
- exploration_theme.mp3 - 探索场景背景音乐

## 如何添加音乐

1. 将MP3文件添加到此目录中
2. 在GameManager.cs的NavigateToScene或其他相关方法中使用PlayBackgroundMusic方法播放音乐

例如：
```csharp
PlayBackgroundMusic("res://Resources/Audio/Music/your_new_theme.mp3");
```

## 如何设置场景音乐

在GameManager.cs中的NavigateToScene方法中，为每个场景设置相应的背景音乐。

```csharp
switch (sceneName)
{
    case "YourScene":
        targetScene = YourScenePath;
        PlayBackgroundMusic("res://Resources/Audio/Music/your_scene_theme.mp3");
        break;
    // ...
}
```

## 音乐版权说明

请确保所有音乐文件都有合法的使用权限或属于公共领域。推荐以下获取免费游戏音乐的资源：

1. [OpenGameArt](https://opengameart.org/) - 提供各种免费游戏资源
2. [FreePD](https://freepd.com/) - 免费公共领域音乐
3. [Free Music Archive](https://freemusicarchive.org/) - 创作共用许可的音乐

使用商业音乐前请确保获得必要的授权。
