# 修仙奇缘

## 项目概述

"修仙奇缘"是一款结合中国传统修仙文化的角色扮演游戏，玩家将扮演一位踏上修仙之路的凡人，通过修炼、战斗和探索，逐步提升自己的境界，最终成就仙道。游戏采用Godot引擎开发，使用C#作为主要编程语言。

## 游戏特性

- **修仙系统**：玩家可以通过冥想、炼丹、参悟功法等方式提升自己的修为，突破不同的境界
- **战斗系统**：回合制战斗机制，施展各种功法和法术与妖魔鬼怪战斗
- **占卜系统**：通过抽取命运卡牌预测未来，获得不同的属性加成和特殊事件
- **物品系统**：收集各种丹药、法宝、灵草等物品辅助修炼和战斗
- **剧情系统**：体验丰富的修仙故事情节，做出影响游戏进程的关键决策

## 开发环境

- **引擎**：Godot 4.2+
- **编程语言**：C#
- **资源管理**：Git LFS
- **构建工具**：.NET SDK 8.0+

## 项目结构

- **Scenes/**：游戏场景文件
  - Game.tscn：主游戏场景
  - Battle.tscn：战斗场景
  - Divination.tscn：占卜场景
  - Inventory.tscn：背包场景
  
- **Scripts/**：C#脚本文件
  - GameManager.cs：游戏管理器
  - PlayerData.cs：玩家数据
  - Character.cs：角色基类
  - DivinationSystem.cs：占卜系统
  - InventoryItem.cs：物品系统
  - Technique.cs：功法系统
  
- **UI/**：用户界面相关文件
  - MainMenuUI.tscn：主菜单界面
  - ResourceErrorUI.tscn：资源错误提示界面
  
- **Resources/**：游戏资源文件
  - Images/：图像资源
  - Audio/：音频资源
  - Fonts/：字体资源

## 开发计划

### 第一阶段：基础框架（3天）
- [x] 设计游戏架构
- [x] 实现基本的数据结构和类
- [x] 构建资源管理系统
- [x] 创建主菜单界面

### 第二阶段：核心系统（7天）
- [x] 实现角色属性和升级系统
- [x] 开发占卜系统
- [x] 实现物品和背包系统
- [x] 编写功法和技能系统
- [ ] 完善资源错误处理

### 第三阶段：游戏场景（5天）
- [x] 设计战斗场景
- [ ] 开发修炼界面
- [ ] 创建探索地图系统
- [ ] 实现NPC交互系统

### 第四阶段：内容制作（10天）
- [ ] 创建多种敌人类型
- [ ] 设计多样化的功法和技能
- [ ] 添加丰富的物品和装备
- [ ] 编写游戏剧情和对话

### 第五阶段：优化和测试（5天）
- [ ] 游戏平衡性调整
- [ ] 界面美化和用户体验优化
- [ ] 性能优化
- [ ] 错误修复和测试

## 总体时间预估

完整开发周期预计需要30天，具体分配如下：
- 基础框架：3天
- 核心系统：7天
- 游戏场景：5天
- 内容制作：10天
- 优化测试：5天

## 资源需求

游戏需要以下资源文件：
- 字体文件：XiaoKeFont.ttf（须放置于Resources/Fonts/目录）
- 卡牌图像：各类占卜牌图片（须放置于Resources/Images/Cards/目录）
- 物品图标：功法、丹药等图标（须放置于Resources/Images/目录）
- 界面素材：背景、按钮等UI元素（须放置于Resources/Images/UI/目录）

## 许可证

本项目使用[MIT许可证](LICENSE)

## 字体文件

- `Resources/Fonts/XiaoKeFont.ttf` - 中文小楷字体
  - 推荐下载链接：[玄式字体](https://github.com/SmallXY/XuanShi_TTF)或[站酷小楷](https://www.foundertype.com/index.php/FontInfo/index/id/4792)

## 图片资源

### 卡牌图片

请在 `Resources/Images/Cards` 目录下放置以下图片文件：
- `taiji.png` - 太极卡牌图片
- `tiandao.png` - 天道卡牌图片
- `nixing.png` - 逆行卡牌图片
- `wuxing.png` - 五行卡牌图片
- `yinyang.png` - 阴阳卡牌图片
- `xiuxing.png` - 修行卡牌图片
- `wudao.png` - 悟道卡牌图片
- `feisheng.png` - 飞升卡牌图片
- `qiyu.png` - 奇遇卡牌图片
- `jienan.png` - 劫难卡牌图片
- `yinguo.png` - 因果卡牌图片

### 物品图片

请在 `Resources/Images/Items` 目录下放置以下图片文件：
- `default_item.png` - 默认物品图片
- `default_technique.png` - 默认功法图片
- `qiPill.png` - 聚气丹图片
- `spiritHerb.png` - 灵参草图片

### UI图片

请在 `Resources/Images/UI` 目录下放置以下图片文件：
- `background.png` - 游戏背景图片
- `button_normal.png` - 按钮普通状态图片
- `button_hover.png` - 按钮悬停状态图片
- `button_pressed.png` - 按钮按下状态图片
- `panel_background.png` - 面板背景图片

## 素材资源获取建议

以下是一些可以获取免费素材的网站：
1. [爱给网](https://www.aigei.com/) - 提供大量免费游戏素材
2. [站酷](https://www.zcool.com.cn/) - 提供各种设计素材
3. [包图网](https://ibaotu.com/) - 有部分免费可商用素材
4. [创绘网](https://www.96at.com/) - 游戏美术资源网站
5. [锈游网](https://www.99ruyu.com/) - 游戏素材下载

## 注意事项

- 请确保下载的资源文件符合商用条款（如果计划商用）
- 文件名应与上述列表保持一致，否则可能导致游戏加载失败
- 图片资源推荐使用PNG格式，支持透明背景
- 尺寸建议：卡牌图片建议为256x256或512x512像素，UI元素根据实际需要调整 