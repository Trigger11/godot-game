using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 历史记录元素类，用于跟踪卡牌移动的历史记录，支持撤销操作
/// </summary>
public partial class HistoryElement : GodotObject
{
    /// <summary>
    /// 卡牌的来源容器
    /// </summary>
    public CardContainer From { get; set; }
    
    /// <summary>
    /// 卡牌的目标容器
    /// </summary>
    public CardContainer To { get; set; }
    
    /// <summary>
    /// 移动的卡牌列表
    /// </summary>
    public List<Card> Cards { get; set; }

    /// <summary>
    /// 获取历史记录的字符串表示
    /// </summary>
    /// <returns>包含来源、目标和卡牌信息的字符串</returns>
    public string GetString()
    {
        string fromStr = From.GetString();
        string toStr = To.GetString();
        List<string> cardStrings = new List<string>();
        
        // 收集所有卡牌的字符串表示
        foreach (Card c in Cards)
        {
            cardStrings.Add(c.GetString());
        }

        // 将卡牌字符串合并为逗号分隔的列表
        string cardsStr = string.Join(", ", cardStrings);

        return $"from: [{fromStr}], to: [{toStr}], cards: [{cardsStr}]";
    }
}