using Godot;
using System;
using System.Collections.Generic;

public partial class Foundation : CardContainer
{
    public FreecellGame FreecellGame { get; set; }

    public bool IsEmpty()
    {
        return _heldCards.Count == 0;
    }

    public Card GetTopCard()
    {
        if (_heldCards.Count == 0)
            return null;
        return _heldCards[_heldCards.Count - 1];
    }

    public override bool CardCanBeAdded(List<Card> cards)
    {
        if (cards.Count != 1)
            return false;

        PlayingCard newCard = cards[0] as PlayingCard;
        if (newCard == null)
            return false;

        // 如果Foundation是空的，只接受Ace
        if (_heldCards.Count == 0)
        {
            return newCard.CardNumber == PlayingCard.Number._A;
        }

        // 检查卡牌是否可以放在顶部卡牌上（相同花色，递增顺序）
        PlayingCard topCard = _heldCards[_heldCards.Count - 1] as PlayingCard;
        return topCard.CardSuit == newCard.CardSuit && topCard.IsNextNumber(newCard);
    }

    public void AutoMoveCards(List<Card> cards, bool withHistory = true)
    {
        base.MoveCards(cards, withHistory);
        FreecellGame.UpdateAllTableausCardsCanBeInteractwith(true);
    }
}