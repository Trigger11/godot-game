using Godot;
using System;
using System.Collections.Generic;

public partial class Example1 : Node
{
    private CardManager _cardManager;
    private CardFactory _cardFactory;
    private Hand _hand;
    private Pile _pile1;
    private Pile _pile2;
    private Pile _pile3;
    private Pile _pile4;
    private Pile _deck;
    private Pile _discard;

    public override void _Ready()
    {
        _cardManager = GetNode<CardManager>("CardManager");
        _cardFactory = GetNode<CardFactory>("CardManager/CardFactory");
        _hand = GetNode<Hand>("CardManager/Hand");
        _pile1 = GetNode<Pile>("CardManager/Pile1");
        _pile2 = GetNode<Pile>("CardManager/Pile2");
        _pile3 = GetNode<Pile>("CardManager/Pile3");
        _pile4 = GetNode<Pile>("CardManager/Pile4");
        _deck = GetNode<Pile>("CardManager/Deck");
        _discard = GetNode<Pile>("CardManager/Discard");
        
        ResetDeck();
    }

    private void ResetDeck()
    {
        var list = GetRandomizedCardList();
        _deck.ClearCards();
        
        foreach (string card in list)
        {
            _cardFactory.CreateCard(card, _deck);
        }
    }

    private string[] GetRandomizedCardList()
    {
        string[] suits = { "club", "spade", "diamond", "heart" };
        string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
        
        List<string> cardList = new List<string>();
        
        foreach (string suit in suits)
        {
            foreach (string value in values)
            {
                cardList.Add($"{suit}_{value}");
            }
        }
        
        // Shuffle the list
        Random random = new Random();
        for (int i = cardList.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            string temp = cardList[i];
            cardList[i] = cardList[j];
            cardList[j] = temp;
        }
        
        return cardList.ToArray();
    }

    private void OnDraw1ButtonPressed()
    {
        _hand.MoveCards(_deck.GetTopCards(1));
    }

    private void OnDraw3ButtonPressed()
    {
        int currentDrawNumber = 3;
        while (currentDrawNumber > 0)
        {
            bool result = _hand.MoveCards(_deck.GetTopCards(currentDrawNumber));
            if (result)
            {
                break;
            }
            
            currentDrawNumber--;
        }
    }

    private void OnResetDeckButtonPressed()
    {
        ResetDeck();
    }

    private void OnUndoButtonPressed()
    {
        _cardManager.Undo();
    }

    private void OnShuffleHandButtonPressed()
    {
        _hand.Shuffle();
    }

    private void OnDiscard1ButtonPressed()
    {
        var cards = _hand.GetRandomCards(1);
        _discard.MoveCards(cards);
    }

    private void OnDiscard3ButtonPressed()
    {
        var cards = _hand.GetRandomCards(3);
        _discard.MoveCards(cards);
    }

    private void OnMoveToPile1ButtonPressed()
    {
        var cards = _hand.GetRandomCards(1);
        _pile1.MoveCards(cards);
    }

    private void OnMoveToPile2ButtonPressed()
    {
        var cards = _hand.GetRandomCards(1);
        _pile2.MoveCards(cards);
    }

    private void OnMoveToPile3ButtonPressed()
    {
        var cards = _hand.GetRandomCards(1);
        _pile3.MoveCards(cards);
    }

    private void OnMoveToPile4ButtonPressed()
    {
        var cards = _hand.GetRandomCards(1);
        _pile4.MoveCards(cards);
    }

    private void OnClearAllButtonPressed()
    {
        ResetDeck();
        _hand.ClearCards();
        _pile1.ClearCards();
        _pile2.ClearCards();
        _pile3.ClearCards();
        _pile4.ClearCards();
        _discard.ClearCards();
    }
}