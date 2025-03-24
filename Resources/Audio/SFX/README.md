# 游戏音效

此目录包含游戏中使用的所有音效文件。

## 音效文件命名规范

音效文件使用descriptive_name.wav格式命名，例如：

- ui_click.wav - 用户界面点击音效
- ui_hover.wav - 用户界面悬停音效
- ui_open.wav - 界面打开音效
- ui_back.wav - 返回按钮音效
- battle_hit.wav - 战斗击中音效
- spell_cast.wav - 施法音效
- level_up.wav - 升级音效

## 音效分类

1. **界面音效** - 以ui_开头
   - 点击按钮
   - 弹出窗口
   - 菜单切换

2. **战斗音效** - 以battle_开头
   - 攻击
   - 受伤
   - 技能效果

3. **环境音效** - 以env_开头
   - 风声
   - 水流
   - 脚步声

## 如何添加和使用音效

1. 将WAV文件添加到此目录中
2. 在需要播放音效的地方使用GameManager.PlaySoundEffect方法

例如：
```csharp
// 在按钮点击时播放音效
_gameManager.PlaySoundEffect("res://Resources/Audio/SFX/ui_click.wav");

// 在战斗中播放击中音效
_gameManager.PlaySoundEffect("res://Resources/Audio/SFX/battle_hit.wav");
```

## 音效优化建议

1. 保持音效文件小而精简（通常低于1MB）
2. 使用16位/44.1kHz WAV格式获得最佳质量和兼容性
3. 确保所有音效音量相对一致，避免突然的音量变化

## 音效版权说明

请确保所有音效文件都有合法的使用权限或属于公共领域。推荐以下获取免费游戏音效的资源：

1. [Freesound](https://freesound.org/) - 大型免费音效库
2. [OpenGameArt](https://opengameart.org/) - 提供各种免费游戏资源
3. [Kenney](https://kenney.nl/) - 提供免费游戏资源包，包括音效

使用商业音效前请确保获得必要的授权。
