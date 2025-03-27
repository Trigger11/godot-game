using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameGenerator : Node
{
    public int[] RandomGenerator(int gameSeed = 1, int count = 1)
    {
        int maxInt32 = int.MaxValue;
        gameSeed = gameSeed & maxInt32;
        int[] rndNumbers = new int[count];
        
        for (int i = 0; i < count; i++)
        {
            gameSeed = (gameSeed * 214013 + 2531011) & maxInt32;
            rndNumbers[i] = gameSeed >> 16;
        }
        
        return rndNumbers;
    }

    public int[] Deal(int gameSeed)
    {
        int nc = 52;
        int[] cards = new int[nc];
        
        for (int i = 0; i < nc; i++)
        {
            cards[i] = nc - 1 - i;
        }
        
        int[] rndNumbers = RandomGenerator(gameSeed, nc);
        
        for (int i = 0; i < nc; i++)
        {
            int r = rndNumbers[i];
            int j = (nc - 1) - r % (nc - i);
            int temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
        
        return cards;
    }

    public string[] GenerateCards(int[] cards)
    {
        List<string> results = new List<string>();
        
        foreach (int c in cards)
        {
            PlayingCard.Suit suit = GetSuit(c);
            PlayingCard.Number number = GetNumber(c);
            string cardName = PlayingCard.GetCardName(suit, number);
            results.Add(cardName);
        }
        
        return results.ToArray();
    }

    public void PrintLog(int[] cards)
    {
        string numbers = "A23456789TJQK";
        string suits = "CDHS";
        List<string> l = new List<string>();
        
        foreach (int c in cards)
        {
            char number = numbers[c / 4];
            char suit = suits[c % 4];
            l.Add(number.ToString() + suit.ToString());
        }
        
        for (int i = 0; i < l.Count; i += 8)
        {
            int end = Math.Min(i + 8, l.Count);
            string line = string.Join(" ", l.GetRange(i, end - i));
            GD.Print(line);
        }
    }

    private PlayingCard.Number GetNumber(int card)
    {
        int num = card / 4;
        
        switch (num)
        {
            case 0: return PlayingCard.Number._A;
            case 1: return PlayingCard.Number._2;
            case 2: return PlayingCard.Number._3;
            case 3: return PlayingCard.Number._4;
            case 4: return PlayingCard.Number._5;
            case 5: return PlayingCard.Number._6;
            case 6: return PlayingCard.Number._7;
            case 7: return PlayingCard.Number._8;
            case 8: return PlayingCard.Number._9;
            case 9: return PlayingCard.Number._10;
            case 10: return PlayingCard.Number._J;
            case 11: return PlayingCard.Number._Q;
            case 12: return PlayingCard.Number._K;
            default:
                GD.PushError("Wrong card number within the game generating process");
                return PlayingCard.Number._OTHER;
        }
    }

    private PlayingCard.Suit GetSuit(int card)
    {
        int num = card % 4;
        
        switch (num)
        {
            case 0: return PlayingCard.Suit.CLUB;
            case 1: return PlayingCard.Suit.DIAMOND;
            case 2: return PlayingCard.Suit.HEART;
            case 3: return PlayingCard.Suit.SPADE;
            default:
                GD.PushError("Wrong card suit within the game generating process");
                return PlayingCard.Suit.NONE;
        }
    }
}