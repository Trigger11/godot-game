using Godot;
using System;
using System.Collections.Generic;

public partial class DivinationSystem : Resource
{
    // 牌组
    public List<DivinationCard> Deck { get; private set; } = new List<DivinationCard>();
    
    // 当前抽出的牌
    public List<DivinationCard> DrawnCards { get; private set; } = new List<DivinationCard>();
    
    // 初始化牌组
    public void InitializeDeck()
    {
        Deck.Clear();
        
        // 添加命运牌
        AddCard(new DivinationCard
        {
            Name = "太极",
            Description = "万物的起源，阴阳的平衡。",
            Type = DivinationCardType.Fate,
            Effect = DivinationCardEffect.Balance,
            ImagePath = "res://Resources/Images/Cards/taiji.png"
        });
        
        AddCard(new DivinationCard
        {
            Name = "天道",
            Description = "顺应天道，得道多助。",
            Type = DivinationCardType.Fate,
            Effect = DivinationCardEffect.Prosperity,
            ImagePath = "res://Resources/Images/Cards/tiandao.png"
        });
        
        AddCard(new DivinationCard
        {
            Name = "逆行",
            Description = "逆天而行，必有灾殃。",
            Type = DivinationCardType.Fate,
            Effect = DivinationCardEffect.Adversity,
            ImagePath = "res://Resources/Images/Cards/nixing.png"
        });
        
        // 添加属性牌
        AddCard(new DivinationCard
        {
            Name = "金木水火土",
            Description = "五行相生相克，万物之理。",
            Type = DivinationCardType.Element,
            Effect = DivinationCardEffect.ElementalMastery,
            ImagePath = "res://Resources/Images/Cards/wuxing.png"
        });
        
        AddCard(new DivinationCard
        {
            Name = "阴阳",
            Description = "世间万物，皆有阴阳。",
            Type = DivinationCardType.Element,
            Effect = DivinationCardEffect.YinYangBalance,
            ImagePath = "res://Resources/Images/Cards/yinyang.png"
        });
        
        // 添加境界牌
        AddCard(new DivinationCard
        {
            Name = "修行",
            Description = "勤修苦练，方能有成。",
            Type = DivinationCardType.Realm,
            Effect = DivinationCardEffect.CultivationBoost,
            ImagePath = "res://Resources/Images/Cards/xiuxing.png"
        });
        
        AddCard(new DivinationCard
        {
            Name = "悟道",
            Description = "顿悟道法，心灵通透。",
            Type = DivinationCardType.Realm,
            Effect = DivinationCardEffect.Enlightenment,
            ImagePath = "res://Resources/Images/Cards/wudao.png"
        });
        
        AddCard(new DivinationCard
        {
            Name = "飞升",
            Description = "脱离凡胎，飞升成仙。",
            Type = DivinationCardType.Realm,
            Effect = DivinationCardEffect.Ascension,
            ImagePath = "res://Resources/Images/Cards/feisheng.png"
        });
        
        // 添加事件牌
        AddCard(new DivinationCard
        {
            Name = "奇遇",
            Description = "福缘深厚，遇奇缘。",
            Type = DivinationCardType.Event,
            Effect = DivinationCardEffect.LuckyEncounter,
            ImagePath = "res://Resources/Images/Cards/qiyu.png"
        });
        
        AddCard(new DivinationCard
        {
            Name = "劫难",
            Description = "命中注定，遭劫难。",
            Type = DivinationCardType.Event,
            Effect = DivinationCardEffect.Catastrophe,
            ImagePath = "res://Resources/Images/Cards/jienan.png"
        });
        
        AddCard(new DivinationCard
        {
            Name = "因果",
            Description = "种善因，得善果；种恶因，得恶果。",
            Type = DivinationCardType.Event,
            Effect = DivinationCardEffect.Karma,
            ImagePath = "res://Resources/Images/Cards/yinguo.png"
        });
    }
    
    // 添加牌到牌组
    public void AddCard(DivinationCard card)
    {
        Deck.Add(card);
    }
    
    // 洗牌
    public void ShuffleDeck()
    {
        Random random = new Random();
        int n = Deck.Count;
        
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            DivinationCard value = Deck[k];
            Deck[k] = Deck[n];
            Deck[n] = value;
        }
    }
    
    // 抽牌
    public DivinationCard DrawCard()
    {
        if (Deck.Count == 0)
            return null;
            
        DivinationCard card = Deck[0];
        Deck.RemoveAt(0);
        DrawnCards.Add(card);
        
        return card;
    }
    
    // 抽指定数量的牌
    public List<DivinationCard> DrawCards(int count)
    {
        List<DivinationCard> result = new List<DivinationCard>();
        
        for (int i = 0; i < count && Deck.Count > 0; i++)
        {
            DivinationCard card = DrawCard();
            if (card != null)
                result.Add(card);
        }
        
        return result;
    }
    
    // 应用占卜效果到玩家
    public void ApplyCardEffect(DivinationCard card, PlayerData player)
    {
        switch (card.Effect)
        {
            case DivinationCardEffect.Balance:
                // 平衡所有属性
                int qiPower = player.GetAttribute("气力");
                int spirit = player.GetAttribute("神识");
                int body = player.GetAttribute("体魄");
                int fate = player.GetAttribute("命运");
                
                int avg = (qiPower + spirit + body + fate) / 4;
                player.AddAttributePoints("气力", avg - qiPower);
                player.AddAttributePoints("神识", avg - spirit);
                player.AddAttributePoints("体魄", avg - body);
                player.AddAttributePoints("命运", avg - fate);
                break;
                
            case DivinationCardEffect.Prosperity:
                // 增加幸运和经验
                player.AddAttributePoints("命运", 5);
                player.AddExperience(100);
                break;
                
            case DivinationCardEffect.Adversity:
                // 减少幸运但增加其他属性
                player.AddAttributePoints("命运", -3);
                player.AddAttributePoints("气力", 3);
                player.AddAttributePoints("体魄", 3);
                break;
                
            case DivinationCardEffect.ElementalMastery:
                // 增加所有属性
                player.AddAttributePoints("气力", 2);
                player.AddAttributePoints("神识", 2);
                player.AddAttributePoints("体魄", 2);
                player.AddAttributePoints("命运", 2);
                break;
                
            case DivinationCardEffect.YinYangBalance:
                // 特殊效果：平衡阴阳，允许突破
                // TODO: 实现特殊突破逻辑
                break;
                
            case DivinationCardEffect.CultivationBoost:
                // 增加修炼速度
                player.AddExperience(200);
                break;
                
            case DivinationCardEffect.Enlightenment:
                // 顿悟，大幅增加神识
                player.AddAttributePoints("神识", 10);
                break;
                
            case DivinationCardEffect.Ascension:
                // 如果条件满足，提升境界
                if (player.Level >= 5)
                {
                    // 直接提升一级
                    player.AddExperience(player.GetRequiredExperienceForNextLevel());
                }
                break;
                
            case DivinationCardEffect.LuckyEncounter:
                // 随机获得一项奇遇
                // TODO: 实现奇遇系统
                player.AddAttributePoints("命运", 5);
                break;
                
            case DivinationCardEffect.Catastrophe:
                // 遭遇劫难
                // TODO: 实现劫难系统
                break;
                
            case DivinationCardEffect.Karma:
                // 根据玩家之前的选择产生不同效果
                // TODO: 实现业力系统
                break;
        }
    }
}

// 占卜牌类
public partial class DivinationCard : Resource
{
    public string Name { get; set; } = "未命名卡牌";
    public string Description { get; set; } = "这是一张占卜牌。";
    public DivinationCardType Type { get; set; } = DivinationCardType.Fate;
    public DivinationCardEffect Effect { get; set; } = DivinationCardEffect.Balance;
    public string ImagePath { get; set; } = "res://Resources/Images/Cards/default_card.png";
}

// 占卜牌类型
public enum DivinationCardType
{
    Fate,      // 命运牌
    Element,   // 元素牌
    Realm,     // 境界牌
    Event      // 事件牌
}

// 占卜牌效果
public enum DivinationCardEffect
{
    Balance,           // 平衡
    Prosperity,        // 繁荣
    Adversity,         // 逆境
    ElementalMastery,  // 元素掌握
    YinYangBalance,    // 阴阳平衡
    CultivationBoost,  // 修炼提升
    Enlightenment,     // 顿悟
    Ascension,         // 飞升
    LuckyEncounter,    // 奇遇
    Catastrophe,       // 劫难
    Karma              // 因果
} 