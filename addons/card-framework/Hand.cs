using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 手牌类，表示玩家的手牌区域
/// </summary>
public partial class Hand : CardContainer
{
    /// <summary>
    /// 手牌区域的大小
    /// </summary>
    [Export]
    public Vector2 HandArea { get; set; }

    [ExportGroup("hand_meta_info")]
    /// <summary>
    /// 可持有的最大卡牌数量
    /// </summary>
    [Export]
    public int MaxHandSize { get; set; } = 10;

    /// <summary>
    /// 手牌的最大展开宽度
    /// </summary>
    [Export]
    public int MaxHandSpread { get; set; } = 700;

    /// <summary>
    /// 卡牌是否正面朝上
    /// </summary>
    [Export]
    public bool CardFaceUp { get; set; } = true;

    /// <summary>
    /// 卡牌悬停时的高度偏移
    /// </summary>
    [Export]
    public int CardHoverDistance { get; set; } = 30;

    [ExportGroup("hand_shape")]
    /// <summary>
    /// 手牌的旋转曲线
    /// 最好使用从-X到+X的两点线性曲线
    /// </summary>
    [Export]
    public Curve HandRotationCurve { get; set; }

    /// <summary>
    /// 手牌的垂直曲线
    /// 最好使用从0到X再到0的三点缓入缓出曲线
    /// </summary>
    [Export]
    public Curve HandVerticalCurve { get; set; }

    /// <summary>
    /// 初始化手牌区域
    /// </summary>
    public override void _Ready()
    {
        base._Ready();
        Size = HandArea;
    }

    /// <summary>
    /// 获取随机的卡牌
    /// </summary>
    /// <param name="n">要获取的卡牌数量</param>
    /// <returns>随机选择的卡牌列表</returns>
    public List<Card> GetRandomCards(int n)
    {
        // 创建_heldCards的副本并转换为List<Card>
        List<Card> deck = new List<Card>(_heldCards);
        
        // 使用Fisher-Yates算法洗牌
        Random random = new Random();
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            Card temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
        
        // 限制n不超过牌组大小
        if (n > deck.Count)
        {
            n = deck.Count;
        }
        
        // 创建包含前n张卡牌的新列表
        List<Card> result = new List<Card>();
        for (int i = 0; i < n; i++)
        {
            result.Add(deck[i]);
        }
        
        return result;
    }

    /// <summary>
    /// 检查是否可以添加卡牌到手牌
    /// </summary>
    /// <param name="cards">要添加的卡牌列表</param>
    /// <returns>如果可以添加则返回true，否则返回false</returns>
    public override bool CardCanBeAdded(List<Card> cards)
    {
        int cardSize = cards.Count;
        return _heldCards.Count + cardSize <= MaxHandSize;
    }

    /// <summary>
    /// 更新卡牌的Z索引
    /// </summary>
    protected override void UpdateTargetZIndex()
    {
        for (int i = 0; i < _heldCards.Count; i++)
        {
            Card card = _heldCards[i];
            card.StoredZIndex = i;
        }
    }

    /// <summary>
    /// 更新卡牌的位置和旋转
    /// </summary>
    protected override void UpdateTargetPositions()
    {
        for (int i = 0; i < _heldCards.Count; i++)
        {
            Card card = _heldCards[i];
            
            // 计算卡牌在手牌中的相对位置比例
            float handRatio = 0.5f;
            if (_heldCards.Count > 1)
            {
                handRatio = (float)i / (_heldCards.Count - 1);
            }
            
            // 计算卡牌的目标位置
            Vector2 targetPos = GlobalPosition;
            float cardSpacing = MaxHandSpread / (_heldCards.Count + 1);
            targetPos.X += (i + 1) * cardSpacing - MaxHandSpread / 2.0f;
            
            // 应用垂直曲线
            if (HandVerticalCurve != null)
            {
                targetPos.Y -= HandVerticalCurve.Sample(handRatio);
            }
            
            // 计算卡牌的旋转角度
            float targetRotation = 0;
            if (HandRotationCurve != null)
            {
                targetRotation = Mathf.DegToRad(HandRotationCurve.Sample(handRatio));
            }
            
            // 移动卡牌到目标位置并设置其属性
            card.Move(targetPos, targetRotation);
            card.ShowFront = CardFaceUp;
            card.CanBeInteractedWith = true;
        }
    }
}