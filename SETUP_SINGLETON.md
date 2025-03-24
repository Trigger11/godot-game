# 设置 GameManager 单例

为了确保游戏数据在场景切换时得到保留，我们已经将 GameManager 设置为单例模式。以下是完整的设置指南：

## 已完成的代码修改

我们已经：

1. 修改了 `GameManager.cs`，使其正确实现单例模式
   - 添加了检查和防止多个实例创建的逻辑
   - 确保在场景切换时 GameManager 不会被销毁
   - 添加了处理游戏关闭时保存的逻辑框架

2. 创建了 `GameManager.tscn` 场景文件
   - 位于 `Scenes/Autoload/GameManager.tscn`
   - 包含了 GameManager 脚本

3. 更新了相关脚本（GameUI.cs, CultivationSystem.cs）
   - 修改为使用 `GameManager.Instance` 获取单例实例
   - 添加了备用方案，在单例不可用时尝试通过节点获取

## 需要您在 Godot 编辑器中完成的设置

请按照以下步骤设置 GameManager 为自动加载：

1. 打开 Godot 编辑器
2. 点击顶部菜单 **项目(Project)** > **项目设置(Project Settings)**
3. 在弹出的窗口中，切换到 **自动加载(AutoLoad)** 选项卡
4. 在 **路径(Path)** 字段中输入或浏览至：`res://Scenes/Autoload/GameManager.tscn`
5. 在 **名称(Name)** 字段中输入：`GameManager`
6. 勾选 **全局作用域(Enable)** 选项
7. 点击 **添加(Add)** 按钮

## 验证设置是否成功

设置完成后，请按照以下步骤验证单例是否正常工作：

1. 运行游戏
2. 切换到任何需要 GameManager 的场景（如修炼、战斗场景）
3. 检查控制台输出，确保没有关于 GameManager 的错误或警告
4. 测试场景之间的切换，确保数据（如玩家属性）能够正确保存

## 常见问题排查

如果遇到问题，请检查：

1. 是否正确设置了自动加载（项目设置 > 自动加载）
2. GameManager.tscn 文件路径是否正确
3. 是否有任何脚本在获取 GameManager 时出现错误

## 使用单例的优势

设置 GameManager 为单例后，您将获得以下好处：

1. 数据在场景切换时得到保留
2. 所有场景都可以方便地访问游戏管理器
3. 避免多个 GameManager 实例导致的数据不一致问题
4. 简化了数据管理和游戏状态控制 