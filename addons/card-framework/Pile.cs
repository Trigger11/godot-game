using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 牌堆类，表示一组堆叠的卡牌
/// </summary>
public partial class Pile : CardContainer
{
    /// <summary>
    /// 牌堆排列方向枚举
    /// </summary>
    public enum PileDirection
    {
        Up,     // 向上堆叠
        Down,   // 向下堆叠
        Left,   // 向左堆叠
        Right   // 向右堆叠
    }

    // 牌堆的Z轴索引基准值
    private const int PILE_Z_INDEX = 3000;

    /// <summary>
    /// 牌堆中每张卡牌之间的间距
    /// </summary>
    [Export]
    public int StackDisplayGap { get; set; } = 8;

    /// <summary>
    /// 牌堆中最多显示的卡牌数量
    /// </summary>
    [Export]
    public int MaxStackDisplay { get; set; } = 6;

    /// <summary>
    /// 确定牌堆中的卡牌是否正面朝上
    /// </summary>
    [Export]
    public bool CardFaceUp { get; set; } = true;

    /// <summary>
    /// 卡牌堆叠的方向
    /// </summary>
    [Export]
    public PileDirection Layout { get; set; } = PileDirection.Up;

    /// <summary>
    /// 确定牌堆中的卡牌是否可以移动
    /// </summary>
    [Export]
    public bool AllowCardMovement { get; set; } = true;

    /// <summary>
    /// 限制只能移动牌堆顶部的卡牌（需要AllowCardMovement为true）
    /// </summary>
    [Export]
    public bool RestrictToTopCard { get; set; } = true;

    /// <summary>
    /// 确定放置区域是否跟随顶部卡牌（需要AllowCardMovement为true）
    /// </summary>
    [Export]
    public bool AlignDropZoneWithTopCard { get; set; } = true;

    /// <summary>
    /// 获取牌堆顶部的n张卡牌
    /// </summary>
    /// <param name="n">要获取的卡牌数量</param>
    /// <returns>顶部的n张卡牌列表</returns>
    public List<Card> GetTopCards(int n)
    {
        int arrSize = _heldCards.Count;
        // 限制n不超过牌堆大小
        if (n > arrSize)
        {
            n = arrSize;
        }

        List<Card> result = new List<Card>();

        // 从牌堆顶部开始获取卡牌
        for (int i = 0; i < n; i++)
        {
            result.Add(_heldCards[arrSize - 1 - i]);
        }

        return result;
    }

    /// <summary>
    /// 更新卡牌的Z轴索引
    /// </summary>
    protected override void UpdateTargetZIndex()
    {
        for (int i = 0; i < _heldCards.Count; i++)
        {
            var card = _heldCards[i];
            // 如果卡牌被按下，则提高其Z轴索引
            if (card.IsPressed)
            {
                card.StoredZIndex = PILE_Z_INDEX + i;
            }
            else
            {
                card.StoredZIndex = i;
            }
        }
    }

    /// <summary>
    /// 更新卡牌的位置
    /// </summary>
    protected override void UpdateTargetPositions()
    {
        int lastIndex = _heldCards.Count - 1;
        Vector2 lastOffset = CalculateOffset(lastIndex);
        
        // 如果启用了放置区域跟随顶部卡牌，则更新放置区域位置
        if (EnableDropZone && AlignDropZoneWithTopCard)
        {
            DropZone.ChangeSensorPositionWithOffset(lastOffset);
        }

        // 更新每张卡牌的位置和交互状态
        for (int i = 0; i < _heldCards.Count; i++)
        {
            var card = _heldCards[i];
            Vector2 offset = CalculateOffset(i);
            Vector2 targetPos = Position + offset;
            card.ShowFront = CardFaceUp;
            card.Move(targetPos, 0);

            // 设置卡牌是否可交互
            if (!AllowCardMovement)
            {
                card.CanBeInteractedWith = false;
            }
            else if (RestrictToTopCard)
            {
                // 如果限制只能移动顶部卡牌，则只有顶部卡牌可交互
                if (i == _heldCards.Count - 1)
                {
                    card.CanBeInteractedWith = true;
                }
                else
                {
                    card.CanBeInteractedWith = false;
                }
            }
        }
    }

    /// <summary>
    /// 计算指定索引位置卡牌的偏移量
    /// </summary>
    /// <param name="index">卡牌索引</param>
    /// <returns>偏移量向量</returns>
    private Vector2 CalculateOffset(int index)
    {
        // 限制索引不超过最大显示数量
        int actualIndex = Math.Min(index, MaxStackDisplay - 1);
        int offsetValue = actualIndex * StackDisplayGap;
        Vector2 offset = Vector2.Zero;

        // 根据布局方向计算偏移量
        switch (Layout)
        {
            case PileDirection.Up:
                offset.Y -= offsetValue;
                break;
            case PileDirection.Down:
                offset.Y += offsetValue;
                break;
            case PileDirection.Right:
                offset.X += offsetValue;
                break;
            case PileDirection.Left:
                offset.X -= offsetValue;
                break;
        }

        return offset;
    }
}