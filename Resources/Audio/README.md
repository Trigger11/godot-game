# 音频资源目录

此目录包含游戏中使用的所有音频资源。

## 目录结构

- **Music/**：包含背景音乐文件（主要为.mp3格式）
  - main_theme.ogg - 主菜单音乐
  - peaceful_theme.mp3 - 平静场景背景音乐
  - battle_theme.mp3 - 战斗场景背景音乐
  - cultivation_theme.mp3 - 修炼场景背景音乐
  - divination_theme.mp3 - 占卜场景背景音乐
  - exploration_theme.mp3 - 探索场景背景音乐

- **SFX/**：包含音效文件（主要为.wav格式）
  - ui_click.wav - 界面点击音效
  - ui_hover.wav - 界面悬停音效
  - ui_open.wav - 界面打开音效
  - ui_back.wav - 返回按钮音效
  - battle_hit.wav - 战斗击中音效
  - spell_cast.wav - 施法音效
  - level_up.wav - 升级音效

## 添加新音频

1. 将音频文件放入相应的文件夹中
2. 在 GameManager.cs 中的相关方法中引用音频文件

## 音频格式

- 背景音乐：建议使用 MP3 格式（44.1KHz, 128-192kbps）
- 音效：建议使用 WAV 格式（44.1KHz, 16bit）

## 音频命名规范

使用下划线分隔的小写字母命名，例如：battle_hit.wav, main_theme.ogg 