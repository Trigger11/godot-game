using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 卡牌容器基类，用于管理卡牌的容器，如牌堆、手牌区等
/// </summary>
public partial class CardContainer : Control
{
    // 用于生成唯一ID的静态计数器
    private static int nextId = 0;

    [ExportGroup("drop_zone")]
    /// <summary>
    /// 启用或禁用放置区域功能
    /// </summary>
    [Export]
    public bool EnableDropZone { get; set; } = true;

    [ExportSubgroup("Sensor")]
    /// <summary>
    /// 感应区域的大小。如果未设置，将使用卡牌的大小
    /// </summary>
    [Export]
    public Vector2 SensorSize { get; set; }

    /// <summary>
    /// 感应区域的位置
    /// </summary>
    [Export]
    public Vector2 SensorPosition { get; set; }

    /// <summary>
    /// 感应区域使用的纹理
    /// </summary>
    [Export]
    public Texture2D SensorTexture { get; set; }

    /// <summary>
    /// 确定感应区域是否可见
    /// </summary>
    [Export]
    public bool SensorVisibility { get; set; } = true;

    // 容器的唯一标识符
    public int UniqueId { get; private set; }
    
    // 放置区域场景引用
    private PackedScene _dropZoneScene;
    
    // 放置区域实例
    public DropZone DropZone { get; private set; }
    
    // 容器中持有的所有卡牌
    public List<Card> _heldCards = new List<Card>();
    
    // 当前正在持有（拖动）的卡牌
    private List<Card> _holdingCards = new List<Card>();
    
    // 卡牌的父节点
    private Card _cardsNode;
    
    // 卡牌管理器引用
    public CardManager CardManager { get; private set; }

    /// <summary>
    /// 构造函数，分配唯一ID
    /// </summary>
    public CardContainer()
    {
        UniqueId = nextId;
        nextId++;
    }

    /// <summary>
    /// 节点准备完成时的初始化
    /// </summary>
    public override void _Ready()
    {
        // 检查'Cards'节点是否已存在
        if (HasNode("Cards"))
        {
            _cardsNode = GetNode<Card>("Cards");
        }
        else
        {
            // 创建新的Cards节点作为卡牌的父节点
            _cardsNode = new Card();
            _cardsNode.Name = "Cards";
            _cardsNode.MouseFilter = MouseFilterEnum.Stop;
            AddChild(_cardsNode);
        }

        // 获取并验证CardManager
        var parent = GetParent();
        if (parent is CardManager cardManager)
        {
            CardManager = cardManager;
        }
        else
        {
            GD.PushError("CardContainer必须是CardManager的子节点");
            return;
        }

        // 向CardManager注册此容器
        CardManager._AddCardContainer(UniqueId, this);

        // 如果启用了放置区域，则创建并配置放置区域
        if (EnableDropZone)
        {
            _dropZoneScene = GD.Load<PackedScene>("drop_zone.tscn");
            DropZone = _dropZoneScene.Instantiate<DropZone>();
            AddChild(DropZone);
            DropZone.ParentCardContainer = this;
            
            // 如果未设置感应区域大小，则使用卡牌大小
            if (SensorSize == Vector2.Zero)
            {
                SensorSize = CardManager.CardSize;
            }
            
            DropZone.SetSensor(SensorSize, SensorPosition, SensorTexture, SensorVisibility);
        }
    }

    /// <summary>
    /// 节点销毁时从CardManager中注销
    /// </summary>
    public override void _ExitTree()
    {
        if (CardManager != null)
        {
            CardManager._DeleteCardContainer(UniqueId);
        }
    }

    /// <summary>
    /// 向容器添加卡牌
    /// </summary>
    public void AddCard(Card card)
    {
        AssignCardToContainer(card);
        MoveObject(card, _cardsNode);
    }

    /// <summary>
    /// 从容器中移除卡牌
    /// </summary>
    /// <returns>移除是否成功</returns>
    public bool RemoveCard(Card card)
    {
        int index = _heldCards.IndexOf(card);
        if (index != -1)
        {
            _heldCards.RemoveAt(index);
        }
        else
        {
            return false;
        }
        
        UpdateCardUI();
        return true;
    }

    /// <summary>
    /// 检查容器是否包含指定卡牌
    /// </summary>
    public bool HasCard(Card card)
    {
        return _heldCards.Contains(card);
    }

    /// <summary>
    /// 清空容器中的所有卡牌
    /// </summary>
    public void ClearCards()
    {
        foreach (Card card in _heldCards)
        {
            RemoveObject(card);
        }
        
        _heldCards.Clear();
    }

    /// <summary>
    /// 检查卡牌是否可以放置到该容器
    /// </summary>
    public bool CheckCardCanBeDropped(List<Card> cards)
    {
        if (DropZone == null)
        {
            return false;
        }

        if (!DropZone.CheckMouseIsInDropZone())
        {
            return false;
        }

        return CardCanBeAdded(cards);
    }

    /// <summary>
    /// 洗牌，使用Fisher-Yates算法随机排序卡牌
    /// </summary>
    public void Shuffle()
    {
        FisherYatesShuffle(_heldCards);
        
        // 重新排列节点顺序以匹配洗牌后的顺序
        for (int i = 0; i < _heldCards.Count; i++)
        {
            var card = _heldCards[i];
            _cardsNode.MoveChild(card, i);
        }
        
        UpdateCardUI();
    }

    /// <summary>
    /// 移动卡牌到该容器
    /// </summary>
    /// <param name="withHistory">是否记录历史</param>
    /// <returns>移动是否成功</returns>
    public bool MoveCards(List<Card> cards, bool withHistory = true)
    {
        if (!CardCanBeAdded(cards))
        {
            return false;
        }
        
        if (withHistory)
        {
            CardManager._AddHistory(this, cards);
        }
        
        MoveCardsInternal(cards);
        return true;
    }

    /// <summary>
    /// 撤销操作，恢复卡牌到该容器
    /// </summary>
    public void Undo(List<Card> cards)
    {
        MoveCardsInternal(cards);
    }

    /// <summary>
    /// 持有卡牌（开始拖动）
    /// </summary>
    public void HoldCard(Card card)
    {
        if (_heldCards.Contains(card))
        {
            _holdingCards.Add(card);
        }
    }

    /// <summary>
    /// 释放所有正在持有的卡牌（结束拖动）
    /// </summary>
    public void ReleaseHoldingCards()
    {
        if (_holdingCards.Count == 0)
        {
            return;
        }
        
        foreach (Card card in _holdingCards)
        {
            card.SetReleasing();
        }
        
        var copiedHoldingCards = new List<Card>(_holdingCards);
        
        if (CardManager != null)
        {
            CardManager._OnDragDropped(copiedHoldingCards);
        }
        
        _holdingCards.Clear();
    }

    /// <summary>
    /// 获取容器的字符串表示
    /// </summary>
    public string GetString()
    {
        return $"card_container: {UniqueId}";
    }

    /// <summary>
    /// 卡牌移动完成时的回调函数
    /// </summary>
    public virtual void OnCardMoveDone(Card card)
    {
        // 由子类重写
    }

    /// <summary>
    /// 卡牌被按下时的回调函数
    /// </summary>
    public virtual void OnCardPressed(Card card)
    {
        // 由子类重写
    }

    /// <summary>
    /// 将卡牌分配给容器并添加到_heldCards列表
    /// </summary>
    protected void AssignCardToContainer(Card card)
    {
        if (card.CardContainer != this)
        {
            card.CardContainer = this;
        }
        
        if (!_heldCards.Contains(card))
        {
            _heldCards.Add(card);
        }
        
        UpdateCardUI();
    }

    /// <summary>
    /// 将卡牌移动到该容器
    /// </summary>
    protected void MoveToCardContainer(Card card)
    {
        if (card.CardContainer != null)
        {
            card.CardContainer.RemoveCard(card);
        }
        
        AddCard(card);
        card.TargetContainer = this;
    }

    /// <summary>
    /// 使用Fisher-Yates算法进行洗牌
    /// </summary>
    protected void FisherYatesShuffle(List<Card> array)
    {
        Random random = new Random();
        
        for (int i = array.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            Card temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    /// <summary>
    /// 移动多张卡牌到该容器的内部实现
    /// </summary>
    protected void MoveCardsInternal(List<Card> cards)
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            Card card = cards[i] as Card;
            MoveToCardContainer(card);
        }
    }

    /// <summary>
    /// 检查卡牌是否可以添加到该容器
    /// </summary>
    public virtual bool CardCanBeAdded(List<Card> cards)
    {
        return true;
    }

    /// <summary>
    /// 更新卡牌UI，包括Z索引和位置
    /// </summary>
    public void UpdateCardUI()
    {
        UpdateTargetZIndex();
        UpdateTargetPositions();
    }

    /// <summary>
    /// 更新卡牌的Z索引
    /// </summary>
    protected virtual void UpdateTargetZIndex()
    {
        // 由子类重写
    }

    /// <summary>
    /// 更新卡牌的位置
    /// </summary>
    protected virtual void UpdateTargetPositions()
    {
        // 由子类重写
    }

    /// <summary>
    /// 将卡牌节点移动到指定父节点
    /// </summary>
    protected void MoveObject(Card target, Card to)
    {
        if (target.GetParent() != null)
        {
            Vector2 globalPos = target.GlobalPosition;
            target.GetParent().RemoveChild(target);
            to.AddChild(target);
            target.GlobalPosition = globalPos;
        }
        else
        {
            to.AddChild(target);
        }
    }

    /// <summary>
    /// 删除卡牌节点
    /// </summary>
    protected void RemoveObject(Card target)
    {
        Node parent = target.GetParent();
        
        if (parent != null)
        {
            parent.RemoveChild(target);
        }
        
        target.QueueFree();
    }
}