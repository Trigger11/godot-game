using Godot;
using System;

/// <summary>
/// 卡牌类，表示游戏中的一张卡牌
/// </summary>
public partial class Card : Control
{
    // 卡牌被持有时的Z轴索引偏移量
    private const int Z_INDEX_OFFSET_WHEN_HOLDING = 1000;

    // 当前正在悬停的卡牌数量
    private static int hovering_card_count = 0;

    /// <summary>
    /// 卡牌的名称
    /// </summary>
    [Export]
    public string CardName { get; set; }

    /// <summary>
    /// 卡牌的尺寸
    /// </summary>
    [Export]
    public Vector2 CardSize { get; set; } = new Vector2(150, 210);

    /// <summary>
    /// 卡牌正面的纹理
    /// </summary>
    [Export]
    public Texture2D FrontImage { get; set; }

    /// <summary>
    /// 卡牌背面的纹理
    /// </summary>
    [Export]
    public Texture2D BackImage { get; set; }

    private bool _showFront = true;
    /// <summary>
    /// 是否显示卡牌正面
    /// 如果为true，则显示正面；否则显示背面
    /// </summary>
    [Export]
    public bool ShowFront
    {
        get => _showFront;
        set
        {
            _showFront = value;
            if (value)
            {
                FrontFaceTexture.Visible = true;
                BackFaceTexture.Visible = false;
            }
            else
            {
                FrontFaceTexture.Visible = false;
                BackFaceTexture.Visible = true;
            }
        }
    }

    /// <summary>
    /// 卡牌移动速度
    /// </summary>
    [Export]
    public int MovingSpeed { get; set; } = 2000;

    /// <summary>
    /// 卡牌是否可交互
    /// </summary>
    [Export]
    public bool CanBeInteractedWith { get; set; } = true;

    /// <summary>
    /// 卡牌悬停时的高度偏移
    /// </summary>
    [Export]
    public int HoverDistance { get; set; } = 10;

    // 卡牌的额外信息
    public Godot.Collections.Dictionary CardInfo { get; set; }
    
    // 持有该卡牌的容器
    public CardContainer CardContainer { get; set; }
    
    // 卡牌是否正在悬停
    public bool IsHovering { get; set; } = false;
    
    // 卡牌是否被按下
    public bool IsPressed { get; set; } = false;
    
    // 卡牌是否正在被持有（拖动）
    public bool IsHolding { get; set; } = false;

    // 存储的Z轴索引值
    private int _storedZIndex;
    public int StoredZIndex
    {
        get => _storedZIndex;
        set
        {
            ZIndex = value;
            _storedZIndex = value;
        }
    }

    // 卡牌是否正在移动到目标位置
    public bool IsMovingToDestination { get; set; } = false;
    
    // 当前持有卡牌的鼠标位置偏移
    public Vector2 CurrentHoldingMousePosition { get; set; }
    
    // 目标位置（全局坐标）
    public Vector2 Destination { get; set; }
    
    // 目标位置（本地坐标）
    public Vector2 DestinationAsLocal { get; set; }
    
    // 目标旋转角度
    public float DestinationDegree { get; set; }
    
    // 目标容器
    public CardContainer TargetContainer { get; set; }

    // 卡牌正面纹理组件
    private TextureRect FrontFaceTexture;
    
    // 卡牌背面纹理组件
    private TextureRect BackFaceTexture;

    /// <summary>
    /// 初始化卡牌
    /// </summary>
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;
        GuiInput += OnGuiInput;

        // 获取纹理组件引用
        FrontFaceTexture = GetNode<TextureRect>("FrontFace/TextureRect");
        BackFaceTexture = GetNode<TextureRect>("BackFace/TextureRect");

        // 设置纹理大小
        FrontFaceTexture.Size = CardSize;
        BackFaceTexture.Size = CardSize;
        
        // 设置纹理
        if (FrontImage != null)
        {
            FrontFaceTexture.Texture = FrontImage;
        }
        
        if (BackImage != null)
        {
            BackFaceTexture.Texture = BackImage;
        }
        
        // 设置旋转锚点
        PivotOffset = CardSize / 2;
        Destination = GlobalPosition;
        StoredZIndex = ZIndex;
    }

    /// <summary>
    /// 更新卡牌状态
    /// </summary>
    public override void _Process(double delta)
    {
        // 处理卡牌拖动
        if (IsHolding)
        {
            StartHovering();
            GlobalPosition = GetGlobalMousePosition() - CurrentHoldingMousePosition;
        }

        // 处理卡牌移动到目标位置
        if (IsMovingToDestination)
        {
            SetDestination();

            Vector2 newPosition = Position.MoveToward(DestinationAsLocal, MovingSpeed * (float)delta);

            // 检查卡牌是否已经到达目标位置
            if (Position.DistanceTo(newPosition) < 0.01 || Position.DistanceTo(DestinationAsLocal) < 0.01)
            {
                Position = DestinationAsLocal;
                IsMovingToDestination = false;
                EndHovering(false);
                ZIndex = StoredZIndex;
                Rotation = DestinationDegree;
                MouseFilter = MouseFilterEnum.Stop;
                CardContainer.OnCardMoveDone(this);
                TargetContainer = null;
            }
            else
            {
                Position = newPosition;
            }
        }
    }

    /// <summary>
    /// 鼠标进入卡牌区域时的处理
    /// </summary>
    public void OnMouseEnter()
    {
        if (hovering_card_count == 0 && !IsMovingToDestination && CanBeInteractedWith)
        {
            StartHovering();
        }
    }

    /// <summary>
    /// 鼠标离开卡牌区域时的处理
    /// </summary>
    public void OnMouseExit()
    {
        if (IsPressed)
        {
            return;
        }
        
        EndHovering(true);
    }

    /// <summary>
    /// 处理输入事件
    /// </summary>
    public void OnGuiInput(InputEvent @event)
    {
        if (!CanBeInteractedWith)
        {
            return;
        }

        if (@event is InputEventMouseButton mouseEvent)
        {
            HandleMouseButton(mouseEvent);
        }
    }

    /// <summary>
    /// 设置卡牌正反面纹理
    /// </summary>
    public void SetFaces(Texture2D frontFace, Texture2D backFace)
    {
        FrontFaceTexture.Texture = frontFace;
        BackFaceTexture.Texture = backFace;
    }

    /// <summary>
    /// 让卡牌返回原位置
    /// </summary>
    public void ReturnCard()
    {
        Rotation = 0;
        IsMovingToDestination = true;
    }

    /// <summary>
    /// 移动卡牌到指定位置
    /// </summary>
    /// <param name="targetDestination">目标位置</param>
    /// <param name="degree">目标旋转角度</param>
    public void Move(Vector2 targetDestination, float degree)
    {
        Rotation = 0;
        DestinationDegree = degree;
        IsMovingToDestination = true;
        Destination = targetDestination;
    }

    /// <summary>
    /// 开始悬停效果
    /// </summary>
    public void StartHovering()
    {
        if (!IsHovering)
        {
            IsHovering = true;
            hovering_card_count++;
            ZIndex += Z_INDEX_OFFSET_WHEN_HOLDING;
            Position = new Vector2(Position.X, Position.Y - HoverDistance);
        }
    }

    /// <summary>
    /// 结束悬停效果
    /// </summary>
    /// <param name="restoreCardPosition">是否恢复卡牌位置</param>
    public void EndHovering(bool restoreCardPosition)
    {
        if (IsHovering)
        {
            ZIndex = StoredZIndex;
            IsHovering = false;
            hovering_card_count--;
            
            if (restoreCardPosition)
            {
                Position = new Vector2(Position.X, Position.Y + HoverDistance);
            }
        }
    }

    /// <summary>
    /// 设置卡牌为持有状态（拖动）
    /// </summary>
    public void SetHolding()
    {
        IsHolding = true;
        CurrentHoldingMousePosition = GetLocalMousePosition();
        ZIndex = StoredZIndex + Z_INDEX_OFFSET_WHEN_HOLDING;
        Rotation = 0;
        
        if (CardContainer != null)
        {
            CardContainer.HoldCard(this);
        }
    }

    /// <summary>
    /// 设置卡牌为释放状态（结束拖动）
    /// </summary>
    public void SetReleasing()
    {
        IsHolding = false;
    }

    /// <summary>
    /// 获取卡牌的字符串表示
    /// </summary>
    public string GetString()
    {
        return CardName;
    }

    /// <summary>
    /// 处理鼠标按钮事件
    /// </summary>
    private void HandleMouseButton(InputEventMouseButton mouseEvent)
    {
        // 只处理左键点击
        if (mouseEvent.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        // 如果卡牌正在移动，则不处理点击事件
        if (IsMovingToDestination)
        {
            return;
        }

        // 处理按下和释放事件
        if (mouseEvent.IsPressed())
        {
            HandleMousePressed();
        }

        if (mouseEvent.IsReleased())
        {
            HandleMouseReleased();
        }
    }

    /// <summary>
    /// 处理鼠标按下事件
    /// </summary>
    private void HandleMousePressed()
    {
        IsPressed = true;
        CardContainer.OnCardPressed(this);
        SetHolding();
    }

    /// <summary>
    /// 处理鼠标释放事件
    /// </summary>
    private void HandleMouseReleased()
    {
        IsPressed = false;
        
        if (CardContainer != null)
        {
            CardContainer.ReleaseHoldingCards();
        }
    }

    /// <summary>
    /// 计算目标位置的本地坐标
    /// </summary>
    private void SetDestination()
    {
        Transform2D t = GetGlobalTransform().AffineInverse();
        Vector2 localPosition = (t.X * Destination.X) + (t.Y * Destination.Y) + t.Origin;
        DestinationAsLocal = localPosition + Position;
        ZIndex = StoredZIndex + Z_INDEX_OFFSET_WHEN_HOLDING;
    }
}