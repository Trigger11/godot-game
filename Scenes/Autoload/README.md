# GameManager 自动加载设置

为了确保GameManager在所有场景中都可用，并在场景切换时保持状态，请按照以下步骤设置自动加载：

## 设置步骤（非常重要！）

1. 在Godot编辑器中，点击顶部菜单的 **项目(Project)** > **项目设置(Project Settings)**
2. 在弹出的窗口中，切换到 **自动加载(AutoLoad)** 选项卡
3. 在 **路径(Path)** 字段中输入：`res://Scenes/Autoload/GameManager.tscn`
4. 在 **名称(Name)** 字段中输入：`GameManager` (名称必须与脚本中的单例名称完全一致)
5. 勾选 **全局作用域(Enable)** 选项
6. 点击 **添加(Add)** 按钮
7. 确认GameManager显示在自动加载列表中且标记为"启用全局"

## 特别注意事项

- **名称必须保持一致**: 自动加载的名称`GameManager`必须与代码中引用的名称完全一致
- **加载顺序**: 确保GameManager在其他需要它的自动加载节点之前加载
- **不要手动在场景中添加GameManager**: 既然它是自动加载的单例，就不应该在任何场景中再次添加它

## 解决常见问题

如果在场景切换时遇到`ObjectDisposedException`错误：
1. 确认GameManager已正确设置为自动加载
2. 检查GameManager.tscn文件是否正确引用了GameManager.cs脚本
3. 确保没有在场景中重复创建GameManager节点
4. 使用GameManager.Instance访问单例，而不是通过节点路径查找

## 在代码中访问GameManager

通过以下方式在任何脚本中访问GameManager：

```csharp
// 方式1：使用静态实例访问（推荐）
GameManager gameManager = GameManager.Instance;

// 方式2：通过自动加载节点访问（只作为备用）
GameManager gameManager = GetNode<GameManager>("/root/GameManager");
```

## 验证单例是否正常工作

在控制台中查看是否有以下消息：
- "GameManager初始化中..."
- "GameManager单例已设置"
- "GameManager初始化完成"

如果没有看到这些消息，或者看到"GameManager单例未找到"的错误，则表示自动加载可能没有正确设置。 