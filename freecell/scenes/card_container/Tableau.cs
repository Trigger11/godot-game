using Godot;
using System;
using System.Collections.Generic;

public partial class Tableau : CardContainer
{
    public FreecellGame FreecellGame { get; set; }
    public bool IsInitializing { get; set; } = false;

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
        // 如果tableau是空的，只接受King
        if (_heldCards.Count == 0)
        {
            if (cards[0] is PlayingCard playingCard && playingCard.CardNumber == PlayingCard.Number._K)
                return true;
            return false;
        }

        // 检查卡牌是否可以放在顶部卡牌上
        PlayingCard topCard = _heldCards[_heldCards.Count - 1] as PlayingCard;
        PlayingCard newCard = cards[0] as PlayingCard;

        return topCard.IsNextNumber(newCard) && topCard.IsDifferentColor(newCard);
    }

    public void InitMoveCards(List<Card> cards, bool withHistory = true)
    {
        base.MoveCards(cards, withHistory);
        FreecellGame.UpdateAllTableausCardsCanBeInteractwith(true);
    }
}