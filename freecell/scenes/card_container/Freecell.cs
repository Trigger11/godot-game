using Godot;
using System;
using System.Collections.Generic;

public partial class Freecell : CardContainer
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
        // Freecell只能放一张牌，并且只有在空的情况下才能放
        return _heldCards.Count == 0 && cards.Count == 1;
    }

    public void AutoMoveCards(List<Card> cards, bool withHistory = true)
    {
        base.MoveCards(cards, withHistory);
        FreecellGame.UpdateAllTableausCardsCanBeInteractwith(true);
    }
}