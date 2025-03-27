using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 卡牌管理器类，负责管理所有卡牌容器和卡牌操作
/// </summary>
[Tool]
public partial class CardManager : Control
{
    /// <summary>
    /// 卡牌尺寸
    /// </summary>
    [Export]
    public Vector2 CardSize { get; set; } = new Vector2(150, 210);

    /// <summary>
    /// 卡牌图像资源目录
    /// </summary>
    [Export]
    public string CardAssetDir { get; set; }

    /// <summary>
    /// 卡牌信息JSON目录
    /// </summary>
    [Export]
    public string CardInfoDir { get; set; }

    /// <summary>
    /// 卡牌通用背面图像
    /// </summary>
    [Export]
    public Texture2D BackImage { get; set; }

    /// <summary>
    /// 卡牌工厂场景
    /// </summary>
    [Export]
    public PackedScene CardFactoryScene { get; set; }

    /// <summary>
    /// 卡牌工厂实例
    /// </summary>
    public CardFactory CardFactory { get; private set; }
    
    // 卡牌容器字典，通过ID快速访问
    private Dictionary<int, CardContainer> _cardContainerDict = new Dictionary<int, CardContainer>();
    
    // 操作历史记录列表，用于支持撤销功能
    private List<HistoryElement> _history = new List<HistoryElement>();

    /// <summary>
    /// 构造函数
    /// </summary>
    public CardManager()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }
    }

    /// <summary>
    /// 初始化卡牌管理器
    /// </summary>
    public override void _Ready()
    {
        if (!PreProcessExportedVariables())
        {
            return;
        }

        if (Engine.IsEditorHint())
        {
            return;
        }

        // 配置卡牌工厂
        CardFactory.CardSize = CardSize;
        CardFactory.CardAssetDir = CardAssetDir;
        CardFactory.CardInfoDir = CardInfoDir;
        CardFactory.BackImage = BackImage;
        CardFactory.PreloadCardData();
    }

    /// <summary>
    /// 撤销最后一次卡牌操作
    /// </summary>
    public void Undo()
    {
        if (_history.Count == 0)
        {
            return;
        }

        // 获取最后一条历史记录并从列表中移除
        var last = _history[_history.Count - 1];
        _history.RemoveAt(_history.Count - 1);
        
        // 将卡牌返回到原容器
        if (last.From != null)
        {
            last.From.Undo(last.Cards);
        }
    }

    /// <summary>
    /// 重置历史记录
    /// </summary>
    public void ResetHistory()
    {
        _history.Clear();
    }

    /// <summary>
    /// 添加卡牌容器到管理器
    /// </summary>
    /// <param name="id">容器ID</param>
    /// <param name="cardContainer">卡牌容器实例</param>
    public void _AddCardContainer(int id, CardContainer cardContainer)
    {
        _cardContainerDict[id] = cardContainer;
    }

    /// <summary>
    /// 从管理器删除卡牌容器
    /// </summary>
    /// <param name="id">容器ID</param>
    public void _DeleteCardContainer(int id)
    {
        _cardContainerDict.Remove(id);
    }

    /// <summary>
    /// 处理卡牌拖放事件
    /// </summary>
    /// <param name="cards">被拖放的卡牌列表</param>
    public void _OnDragDropped(List<Card> cards)
    {
        if (cards.Count == 0)
        {
            return;
        }

        // 禁用卡牌的鼠标交互
        foreach (Card card in cards)
        {
            card.MouseFilter = MouseFilterEnum.Ignore;
        }

        // 遍历所有容器，查找可以放置卡牌的容器
        foreach (var key in _cardContainerDict.Keys)
        {
            var cardContainer = _cardContainerDict[key];
            bool result = cardContainer.CheckCardCanBeDropped(cards);
            
            if (result)
            {
                cardContainer.MoveCards(cards);
                return;
            }
        }

        // 如果没有找到可放置的容器，让卡牌返回原位
        foreach (Card card in cards)
        {
            card.ReturnCard();
        }
    }

    /// <summary>
    /// 添加操作历史记录
    /// </summary>
    /// <param name="to">目标容器</param>
    /// <param name="cards">移动的卡牌列表</param>
    public void _AddHistory(CardContainer to, List<Card> cards)
    {
        CardContainer from = null;

        // 确保所有卡牌来自同一个容器
        for (int i = 0; i < cards.Count; i++)
        {
            Card c = cards[i] as Card;
            var current = c.CardContainer;
            
            if (i == 0)
            {
                from = current;
            }
            else
            {
                if (from != current)
                {
                    GD.PushError("所有卡牌必须来自同一个容器！");
                    return;
                }
            }
        }

        // 创建并添加历史记录
        var historyElement = new HistoryElement();
        historyElement.From = from;
        historyElement.To = to;
        historyElement.Cards = cards;
        _history.Add(historyElement);
    }

    /// <summary>
    /// 检查目录是否有效
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>如果目录有效则返回true，否则返回false</returns>
    private bool IsValidDirectory(string path)
    {
        var dir = DirAccess.Open(path);
        return dir != null;
    }

    /// <summary>
    /// 预处理导出变量，确保所有必要的资源都已设置
    /// </summary>
    /// <returns>如果所有变量有效则返回true，否则返回false</returns>
    private bool PreProcessExportedVariables()
    {
        if (!IsValidDirectory(CardAssetDir))
        {
            GD.PushError("CardManager的card_asset_dir无效");
            return false;
        }

        if (!IsValidDirectory(CardInfoDir))
        {
            GD.PushError("CardManager的card_info_dir无效");
            return false;
        }

        if (BackImage == null)
        {
            GD.PushError("CardManager未分配背面图像");
        }

        if (CardFactoryScene == null)
        {
            GD.PushError("未分配CardFactory！请在CardManager检视器中设置。");
            return false;
        }

        var factoryInstance = CardFactoryScene.Instantiate() as CardFactory;
        
        if (factoryInstance == null)
        {
            GD.PushError("创建CardFactory实例失败！CardManager导入了错误的卡牌工厂场景。");
            return false;
        }

        AddChild(factoryInstance);
        CardFactory = factoryInstance;
        return true;
    }
}