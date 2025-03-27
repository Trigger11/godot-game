using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 卡牌工厂类，用于创建和管理卡牌实例
/// </summary>
[Tool]
public partial class CardFactory : Node
{
    /// <summary>
    /// 用于实例化的基础卡牌场景
    /// </summary>
    [Export]
    public PackedScene DefaultCardScene { get; set; }

    // 预加载的卡牌数据缓存
    private Dictionary<string, Dictionary<string, object>> _preloadedCards = new Dictionary<string, Dictionary<string, object>>();
    
    /// <summary>
    /// 卡牌资源目录路径
    /// </summary>
    public string CardAssetDir { get; set; }
    
    /// <summary>
    /// 卡牌信息目录路径
    /// </summary>
    public string CardInfoDir { get; set; }
    
    /// <summary>
    /// 卡牌尺寸
    /// </summary>
    public Vector2 CardSize { get; set; }
    
    /// <summary>
    /// 卡牌背面图像
    /// </summary>
    public Texture2D BackImage { get; set; }

    /// <summary>
    /// 初始化卡牌工厂
    /// </summary>
    public override void _Ready()
    {
        if (DefaultCardScene == null)
        {
            GD.PushError("default_card_scene未分配！");
            return;
        }

        // 验证默认卡牌场景是否为Card类型
        var tempInstance = DefaultCardScene.Instantiate();
        if (!(tempInstance is Card))
        {
            GD.PushError("无效的节点类型！default_card_scene必须引用Card类型。");
            DefaultCardScene = null;
        }
        tempInstance.QueueFree();
    }

    /// <summary>
    /// 创建指定名称的卡牌实例并添加到目标容器
    /// </summary>
    /// <param name="cardName">卡牌名称</param>
    /// <param name="target">目标容器</param>
    /// <returns>创建的卡牌实例</returns>
    public Card CreateCard(string cardName, CardContainer target)
    {
        // 检查卡牌信息是否已缓存
        if (_preloadedCards.ContainsKey(cardName))
        {
            var cardInfo = (Godot.Collections.Dictionary)_preloadedCards[cardName]["info"];
            var frontImage = (Texture2D)_preloadedCards[cardName]["texture"];
            return CreateCardNode(cardInfo["name"].ToString(), frontImage, target, cardInfo);
        }
        else
        {
            // 加载卡牌信息
            var cardInfo = LoadCardInfo(cardName);
            if (cardInfo == null || cardInfo.Count == 0)
            {
                GD.PushError($"找不到卡牌信息：{cardName}");
                return null;
            }

            // 验证前面图像键是否存在
            if (!cardInfo.ContainsKey("front_image"))
            {
                GD.PushError($"卡牌信息中不包含'front_image'键：{cardName}");
                return null;
            }

            // 加载前面图像
            string frontImagePath = CardAssetDir + "/" + cardInfo["front_image"].ToString();
            var frontImage = LoadImage(frontImagePath);
            if (frontImage == null)
            {
                GD.PushError($"找不到卡牌图像：{frontImagePath}");
                return null;
            }

            return CreateCardNode(cardInfo["name"].ToString(), frontImage, target, cardInfo);
        }
    }

    /// <summary>
    /// 预加载所有卡牌数据
    /// </summary>
    public void PreloadCardData()
    {
        var dir = DirAccess.Open(CardInfoDir);
        if (dir == null)
        {
            GD.PushError($"无法打开目录：{CardInfoDir}");
            return;
        }

        dir.ListDirBegin();
        var fileName = dir.GetNext();
        while (fileName != "")
        {
            // 只处理JSON文件
            if (!fileName.EndsWith(".json"))
            {
                fileName = dir.GetNext();
                continue;
            }

            // 加载卡牌信息
            string cardName = fileName.GetBaseName();
            var cardInfo = LoadCardInfo(cardName);
            if (cardInfo == null)
            {
                GD.PushError($"无法加载卡牌信息：{cardName}");
                fileName = dir.GetNext();
                continue;
            }

            // 加载卡牌前面图像
            string frontImageKey = "front_image";
            string defaultPath = "";
            string frontImagePath = CardAssetDir + "/" + (cardInfo.ContainsKey(frontImageKey) ? cardInfo[frontImageKey].ToString() : defaultPath);
            
            var frontImageTexture = LoadImage(frontImagePath);
            if (frontImageTexture == null)
            {
                GD.PushError($"无法加载卡牌图像：{frontImagePath}");
                fileName = dir.GetNext();
                continue;
            }

            // 缓存卡牌数据
            _preloadedCards[cardName] = new Dictionary<string, object>
            {
                { "info", cardInfo },
                { "texture", frontImageTexture }
            };
            
            GD.Print("预加载卡牌数据:", _preloadedCards[cardName]);
            
            fileName = dir.GetNext();
        }
    }

    /// <summary>
    /// 加载指定卡牌的信息
    /// </summary>
    /// <param name="cardName">卡牌名称</param>
    /// <returns>卡牌信息字典</returns>
    private Godot.Collections.Dictionary LoadCardInfo(string cardName)
    {
        string jsonPath = CardInfoDir + "/" + cardName + ".json";
        if (!FileAccess.FileExists(jsonPath))
        {
            return new Godot.Collections.Dictionary();
        }

        // 读取JSON文件
        var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
        string jsonString = file.GetAsText();
        file.Close();

        // 解析JSON数据
        var json = new Json();
        Error error = json.Parse(jsonString);
        if (error != Error.Ok)
        {
            GD.PushError($"解析JSON失败：{jsonPath}");
            return new Godot.Collections.Dictionary();
        }

        return (Godot.Collections.Dictionary)json.Data;
    }

    /// <summary>
    /// 加载指定路径的图像
    /// </summary>
    /// <param name="imagePath">图像路径</param>
    /// <returns>图像纹理</returns>
    private Texture2D LoadImage(string imagePath)
    {
        var texture = GD.Load<Texture2D>(imagePath);
        if (texture == null)
        {
            GD.PushError($"无法加载图像资源：{imagePath}");
            return null;
        }
        
        return texture;
    }

    /// <summary>
    /// 创建卡牌节点并设置属性
    /// </summary>
    /// <param name="cardName">卡牌名称</param>
    /// <param name="frontImage">正面图像</param>
    /// <param name="target">目标容器</param>
    /// <param name="cardInfo">卡牌信息</param>
    /// <returns>创建的卡牌实例</returns>
    private Card CreateCardNode(string cardName, Texture2D frontImage, CardContainer target, Godot.Collections.Dictionary cardInfo)
    {
        Card card = GenerateCard(cardInfo);
        
        // 检查卡牌是否可以添加到目标容器
        if (!target.CardCanBeAdded(new List<Card> { card }))
        {
            GD.Print($"卡牌无法添加：{cardName}");
            card.QueueFree();
            return null;
        }

        // 设置卡牌属性
        card.CardInfo = cardInfo;
        card.CardSize = CardSize;
        var cardsNode = target.GetNode("Cards");
        cardsNode.AddChild(card);
        target.AddCard(card);
        card.CardName = cardName;
        card.SetFaces(frontImage, BackImage);

        return card;
    }

    /// <summary>
    /// 根据卡牌信息生成卡牌实例
    /// </summary>
    /// <param name="cardInfo">卡牌信息</param>
    /// <returns>生成的卡牌实例</returns>
    private Card GenerateCard(Godot.Collections.Dictionary cardInfo)
    {
        if (DefaultCardScene == null)
        {
            GD.PushError("default_card_scene未分配！");
            return null;
        }
        
        return DefaultCardScene.Instantiate<Card>();
    }
}