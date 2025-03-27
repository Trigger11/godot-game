using Godot;
using System;

public partial class FreecellCardFactory : Node
{
    [Export]
    public PackedScene CardScene { get; set; }

    public Card CreateCard(string cardName, CardContainer container)
    {
        Card card = CardScene.Instantiate<Card>();
        card.CardName = cardName;
        container.AddCard(card);
        return card;
    }
}