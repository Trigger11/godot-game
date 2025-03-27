using Godot;
using System;

public partial class PlayingCard : Card
{
    public enum Suit { NONE = 0, SPADE = 1, HEART = 2, DIAMOND = 3, CLUB = 4 }
    public enum Number { _OTHER = 0, _A = 1, _2 = 2, _3 = 3, _4 = 4, _5 = 5, _6 = 6, _7 = 7, _8 = 8, _9 = 9, _10 = 10, _J = 11, _Q = 12, _K = 13 }
    public enum CardColor { NONE = 0, BLACK = 1, RED = 2 }

    private Suit _suit;
    public Suit CardSuit
    {
        get
        {
            return GetSuitFromString(CardInfo["suit"].ToString());
        }
    }

    private Number _number;
    public Number CardNumber
    {
        get
        {
            return GetNumberFromString(CardInfo["value"].ToString());
        }
    }

    private CardColor _cardColor;
    public CardColor Color
    {
        get
        {
            switch (CardSuit)
            {
                case Suit.SPADE:
                    return CardColor.BLACK;
                case Suit.HEART:
                    return CardColor.RED;
                case Suit.DIAMOND:
                    return CardColor.RED;
                case Suit.CLUB:
                    return CardColor.BLACK;
                default:
                    return CardColor.NONE;
            }
        }
    }

    public bool IsStopControl { get; set; } = false;

    public static string GetSuitAsString(Suit suit)
    {
        string suitStr;
        switch (suit)
        {
            case Suit.SPADE:
                suitStr = "spade";
                break;
            case Suit.HEART:
                suitStr = "heart";
                break;
            case Suit.DIAMOND:
                suitStr = "diamond";
                break;
            case Suit.CLUB:
                suitStr = "club";
                break;
            default:
                suitStr = "none";
                break;
        }
        return suitStr;
    }

    public static string GetNumberAsString(Number number)
    {
        string numberStr;
        switch (number)
        {
            case Number._A:
                numberStr = "A";
                break;
            case Number._2:
                numberStr = "2";
                break;
            case Number._3:
                numberStr = "3";
                break;
            case Number._4:
                numberStr = "4";
                break;
            case Number._5:
                numberStr = "5";
                break;
            case Number._6:
                numberStr = "6";
                break;
            case Number._7:
                numberStr = "7";
                break;
            case Number._8:
                numberStr = "8";
                break;
            case Number._9:
                numberStr = "9";
                break;
            case Number._10:
                numberStr = "10";
                break;
            case Number._J:
                numberStr = "J";
                break;
            case Number._Q:
                numberStr = "Q";
                break;
            case Number._K:
                numberStr = "K";
                break;
            case Number._OTHER:
                numberStr = "other";
                break;
            default:
                numberStr = "unknown";
                break;
        }
        return numberStr;
    }

    public static string GetCardName(Suit suit, Number number)
    {
        string suitStr = GetSuitAsString(suit);
        string numberStr = GetNumberAsString(number);
        return suitStr + "_" + numberStr;
    }

    public bool IsNextNumber(PlayingCard targetCard)
    {
        int currentNumber = (int)CardNumber;
        int targetNumber = (int)targetCard.CardNumber;
        int nextNumber = (currentNumber % 13) + 1;
        return nextNumber == targetNumber;
    }

    public bool IsDifferentColor(PlayingCard other)
    {
        return Color != other.Color;
    }

    private Suit GetSuitFromString(string str)
    {
        if (str == "spade")
            return Suit.SPADE;
        else if (str == "heart")
            return Suit.HEART;
        else if (str == "diamond")
            return Suit.DIAMOND;
        else if (str == "club")
            return Suit.CLUB;
        else
            return Suit.NONE;
    }

    private Number GetNumberFromString(string str)
    {
        if (str == "2")
            return Number._2;
        else if (str == "3")
            return Number._3;
        else if (str == "4")
            return Number._4;
        else if (str == "5")
            return Number._5;
        else if (str == "6")
            return Number._6;
        else if (str == "7")
            return Number._7;
        else if (str == "8")
            return Number._8;
        else if (str == "9")
            return Number._9;
        else if (str == "10")
            return Number._10;
        else if (str == "J")
            return Number._J;
        else if (str == "Q")
            return Number._Q;
        else if (str == "K")
            return Number._K;
        else if (str == "A")
            return Number._A;
        else
            return Number._OTHER;
    }

    public new void OnMouseEnter()
    {
        if (IsStopControl)
            return;
        base.OnMouseEnter();
    }

    public new void OnMouseExit()
    {
        if (IsStopControl)
            return;
        base.OnMouseExit();
    }

    public new void OnGuiInput(InputEvent @event)
    {
        if (IsStopControl)
            return;
        base.OnGuiInput(@event);
    }
}