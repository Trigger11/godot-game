using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FreecellGame : Node
{
    public enum GameState { WIN = 1, LOSE = 2, PLAYING = 3 }

    private static readonly string[] Suits = { "Heart", "Spade", "Diamond", "Club" };
    private const float AutoMoveTimerWatingTime = 0.2f;
    private const float GameGeneratingTimerWaitingTime = 0.05f;

    private List<Freecell> _freecells = new List<Freecell>();
    private List<Foundation> _foundations = new List<Foundation>();
    private List<Tableau> _tableaus = new List<Tableau>();
    private List<Card> _allCards = new List<Card>();
    private FreecellCardFactory _cardFactory;
    private int _gameSeed = 0;
    private bool _isCreatingNewGame = false;
    private Timer _autoMoveTimer;
    private Dictionary<string, object> _autoMoveTarget = new Dictionary<string, object>();
    private Dictionary<Card, Foundation> _autoMovingMap = new Dictionary<Card, Foundation>();
    private Timer _gameGeneratingTimer;
    private int _elapsedTime = 0;
    private Timer _gameTimer;
    private int _undoCount = 0;
    private int _score = 0;
    private GameState _gameState = GameState.PLAYING;
    private bool _isGameRunning = false;
    private RecordManager _recordManager;
    private PackedScene _menuScene;

    public int _moveCount = 0;

    private CardManager _cardManager;
    private GameGenerator _gameGenerator;
    private CardContainer _startPosition;
    private Label _timeDisplay;
    private Label _scoreDisplay;
    private AcceptDialog _restartGameDialog;
    private AcceptDialog _goToMenuDialog;
    private Label _information;

    public override void _Ready()
    {
        _menuScene = GD.Load<PackedScene>("res://freecell/scenes/menu/menu.tscn");
        
        _cardManager = GetNode<CardManager>("CardManager");
        _gameGenerator = GetNode<GameGenerator>("GameGenerator");
        _startPosition = GetNode<CardContainer>("CardManager/StartPosition");
        _timeDisplay = GetNode<Label>("Time");
        _scoreDisplay = GetNode<Label>("Score");
        _restartGameDialog = GetNode<AcceptDialog>("RestartGameDialog");
        _goToMenuDialog = GetNode<AcceptDialog>("GoToMenuDialog");
        _information = GetNode<Label>("Information");
        
        SetRecordManager();
        SetContainers();
        SetUiButtons();
        SetAutoMover();
        SetGameGeneratingTimer();
        SetGameTimer();
        UpdateInformation();
    }

    public int MaximumNumberOfSuperMove(Tableau tableau)
    {
        int emptyFreecells = CountRemainingFreecell();
        int emptyTableaus = CountRemainingTableaus();
        int result = (int)Math.Pow(2, emptyTableaus) * (emptyFreecells + 1);
        if (tableau != null && tableau.IsEmpty())
        {
            result = result / 2;
        }
        return result;
    }

    public void HoldMultipleCards(Card card, Tableau tableau)
    {
        Card currentCard = null;
        List<Card> holdingCardList = new List<Card>();
        int maxSuperMove = MaximumNumberOfSuperMove(null);
        
        for (int i = tableau._heldCards.Count - 1; i >= 0; i--)
        {
            PlayingCard targetCard = tableau._heldCards[i] as PlayingCard;
            if (currentCard == null)
            {
                currentCard = targetCard;
                holdingCardList.Add(currentCard);
            }
            else if (holdingCardList.Count >= maxSuperMove)
            {
                holdingCardList.Clear();
                return;
            }
            else if (((PlayingCard)currentCard).IsNextNumber(targetCard) && ((PlayingCard)currentCard).IsDifferentColor(targetCard))
            {
                currentCard = targetCard;
                holdingCardList.Add(currentCard);
                if (currentCard == card)
                {
                    break;
                }
            }
            else
            {
                holdingCardList.Clear();
                return;
            }
            if (currentCard == card)
            {
                break;
            }
        }

        foreach (Card targetCard in holdingCardList)
        {
            if (targetCard != card)
            {
                targetCard.StartHovering();
                targetCard.SetHolding();
            }
        }
    }

    public void UpdateAllTableausCardsCanBeInteractwith(bool useAutoMove = true)
    {
        foreach (Tableau tableau in _tableaus)
        {
            if (tableau.IsInitializing)
            {
                continue;
            }
            UpdateCardsCanBeInteractwith(tableau);
            if (useAutoMove)
            {
                CheckAutoMove(tableau);
            }
        }
        
        foreach (Freecell freecell in _freecells)
        {
            if (useAutoMove)
            {
                CheckAutoMove(freecell);
            }
        }

        UpdateScore();
        _gameState = GetGameState();

        switch (_gameState)
        {
            case GameState.WIN:
                ShowResultPopup(true);
                EndGame();
                break;
            case GameState.LOSE:
                ShowResultPopup(false);
                EndGame();
                break;
            case GameState.PLAYING:
                break;
        }
        
        UpdateInformation();
    }

    public void NewGame()
    {
        if (_isGameRunning)
        {
            _gameState = GameState.LOSE;
            EndGame();
        }
        
        if (_isCreatingNewGame)
        {
            return;
        }
        
        _isCreatingNewGame = true;
        
        if (_gameTimer != null)
        {
            _gameTimer.Stop();
        }
        
        SetElapsedTime(0);
        ResetCardsInGame();
        _ = GenerateCards(); // Call the async method but don't await it directly in the C# conversion
        StartGame();
        _isCreatingNewGame = false;
        _isGameRunning = true;
    }

    private GameState GetGameState()
    {
        bool winCondition = CheckWinCondition();
        if (winCondition)
        {
            return GameState.WIN;
        }

        bool loseCondition = CheckLoseCondition();
        if (loseCondition)
        {
            return GameState.LOSE;
        }

        return GameState.PLAYING;
    }

    private void UpdateCardsCanBeInteractwith(Tableau tableau)
    {
        Card currentCard = null;
        int count = 0;
        int maxSuperMove = MaximumNumberOfSuperMove(null);
        
        foreach (Card card in tableau._heldCards)
        {
            card.CanBeInteractedWith = false;
        }
        
        for (int i = tableau._heldCards.Count - 1; i >= 0; i--)
        {
            PlayingCard targetCard = tableau._heldCards[i] as PlayingCard;
            
            if (currentCard == null)
            {
                currentCard = targetCard;
                targetCard.CanBeInteractedWith = true;
                count++;
            }
            else if (count >= maxSuperMove)
            {
                return;
            }
            else if (((PlayingCard)currentCard).IsNextNumber(targetCard) && ((PlayingCard)currentCard).IsDifferentColor(targetCard))
            {
                currentCard = targetCard;
                targetCard.CanBeInteractedWith = true;
                count++;
            }
            else
            {
                return;
            }
            
            if (((PlayingCard)currentCard).CardNumber == PlayingCard.Number._K)
            {
                return;
            }
        }
    }

    private Foundation GetFoundation(PlayingCard.Suit suit)
    {
        switch (suit)
        {
            case PlayingCard.Suit.SPADE:
                return _foundations[1];
            case PlayingCard.Suit.HEART:
                return _foundations[0];
            case PlayingCard.Suit.DIAMOND:
                return _foundations[2];
            case PlayingCard.Suit.CLUB:
                return _foundations[3];
            default:
                return null;
        }
    }

    private int GetMinimumNumber(Foundation a, Foundation b)
    {
        Card aTopCard = a.GetTopCard();
        Card bTopCard = b.GetTopCard();
        int aTopNumber = 0;
        int bTopNumber = 0;
        
        if (aTopCard != null)
        {
            aTopNumber = (int)((PlayingCard)aTopCard).CardNumber;
        }
        
        if (bTopCard != null)
        {
            bTopNumber = (int)((PlayingCard)bTopCard).CardNumber;
        }
        
        return Math.Min(aTopNumber, bTopNumber);
    }

    private int GetMinimumNumberInFoundation(PlayingCard.CardColor cardColor)
    {
        if (cardColor == PlayingCard.CardColor.BLACK)
        {
            return GetMinimumNumber(GetFoundation(PlayingCard.Suit.SPADE), GetFoundation(PlayingCard.Suit.CLUB));
        }
        else if (cardColor == PlayingCard.CardColor.RED)
        {
            return GetMinimumNumber(GetFoundation(PlayingCard.Suit.HEART), GetFoundation(PlayingCard.Suit.DIAMOND));
        }
        else
        {
            return -1;
        }
    }

    private void CheckAutoMove(CardContainer container)
    {
        if (container._heldCards.Count == 0)
        {
            return;
        }
        
        PlayingCard topCard = container._heldCards[container._heldCards.Count - 1] as PlayingCard;
        PlayingCard.Suit suit = topCard.CardSuit;
        PlayingCard.CardColor cardColor = topCard.Color;
        PlayingCard.CardColor oppositeColor = PlayingCard.CardColor.NONE;
        
        if (cardColor == PlayingCard.CardColor.BLACK)
        {
            oppositeColor = PlayingCard.CardColor.RED;
        }
        else if (cardColor == PlayingCard.CardColor.RED)
        {
            oppositeColor = PlayingCard.CardColor.BLACK;
        }

        Foundation foundation = GetFoundation(suit);
        Card topCardOfFoundation = foundation.GetTopCard();

        bool result = false;
        
        if (topCardOfFoundation == null)
        {
            if ((int)topCard.CardNumber == 1)
            {
                result = true;
            }
        }
        else
        {
            int minOtherColorNumber = GetMinimumNumberInFoundation(oppositeColor);
            if (((PlayingCard)topCardOfFoundation).IsNextNumber(topCard) && (int)topCard.CardNumber <= minOtherColorNumber + 1)
            {
                result = true;
            }
        }

        if (result)
        {
            _autoMoveTarget["card"] = topCard;
            _autoMoveTarget["foundation"] = foundation;
            
            if (_autoMovingMap.ContainsKey(topCard))
            {
                return;
            }
            
            _autoMovingMap[topCard] = foundation;
            SetAllCardControl(true);
            _autoMoveTimer.Start(AutoMoveTimerWatingTime);
        }
    }

    private void SetContainers()
    {
        for (int i = 1; i <= 4; i++)
        {
            Freecell freecell = _cardManager.GetNode<Freecell>($"Freecell_{i}");
            _freecells.Add(freecell);
            freecell.FreecellGame = this;
        }

        foreach (string suit in Suits)
        {
            Foundation foundation = _cardManager.GetNode<Foundation>($"Foundation_{suit}");
            _foundations.Add(foundation);
            foundation.FreecellGame = this;
        }

        for (int i = 1; i <= 8; i++)
        {
            Tableau tableau = _cardManager.GetNode<Tableau>($"Tableau_{i}");
            _tableaus.Add(tableau);
            tableau.FreecellGame = this;
        }
    }

    private void SetAutoMover()
    {
        _autoMoveTimer = new Timer();
        _autoMoveTimer.WaitTime = AutoMoveTimerWatingTime;
        _autoMoveTimer.OneShot = true;
        _autoMoveTimer.Timeout += OnTimeout;
        AddChild(_autoMoveTimer);
    }

    private void SetGameGeneratingTimer()
    {
        _gameGeneratingTimer = new Timer();
        _gameGeneratingTimer.WaitTime = GameGeneratingTimerWaitingTime;
        _gameGeneratingTimer.OneShot = true;
        AddChild(_gameGeneratingTimer);
    }

    private void SetElapsedTime(int time)
    {
        _elapsedTime = time;
        _timeDisplay.Text = _elapsedTime.ToString();
    }

    private void SetGameTimer()
    {
        if (_gameTimer == null)
        {
            _gameTimer = new Timer();
            _gameTimer.WaitTime = 1.0f;
            _gameTimer.OneShot = false;
            _gameTimer.Timeout += OnGameTimerTimeout;
            AddChild(_gameTimer);
        }
        
        SetElapsedTime(0);
    }

    private void OnGameTimerTimeout()
    {
        SetElapsedTime(_elapsedTime + 1);
        UpdateInformation();
    }

    private void StartGame()
    {
        _moveCount = 0;
        _undoCount = 0;
        _score = 0;
        SetElapsedTime(0);
        _gameTimer.Start();
        UpdateInformation();
    }

    private void EndGame()
    {
        _gameTimer.Stop();
        _recordManager.MakeRecord(_gameSeed, _moveCount, _undoCount, _elapsedTime, _score, _gameState);
        GD.Print($"move: {_moveCount}, undo: {_undoCount}, score: {_score}, time: {_elapsedTime}");
        _isGameRunning = false;
    }

    private void UpdateScore()
    {
        _score = 0;
        
        foreach (Foundation foundation in _foundations)
        {
            _score += foundation._heldCards.Count * 10;
        }
        
        _score -= _moveCount;
        _score -= _undoCount * 3;
        _scoreDisplay.Text = _score.ToString();
        UpdateInformation();
    }

    private void SetRecordManager()
    {
        Node node = GetTree().Root.GetNode("RecordManager");
        _recordManager = node as RecordManager;
    }

    private void OnButtonRestartGamePressed()
    {
        if (_isGameRunning)
        {
            _restartGameDialog.PopupCentered();
        }
        else
        {
            NewGame();
        }
    }

    private void OnButtonUndoPressed()
    {
        if (_isGameRunning)
        {
            _cardManager.Undo();
        }
    }

    private void OnButtonMenuPressed()
    {
        if (_isGameRunning)
        {
            _goToMenuDialog.PopupCentered();
        }
        else
        {
            GoToMenu();
        }
    }

    private void SetUiButtons()
    {
        Button buttonRestartGame = GetNode<Button>("ButtonRestartGame");
        buttonRestartGame.Connect("pressed", new Callable(this, nameof(OnButtonRestartGamePressed)));
        
        Button buttonUndo = GetNode<Button>("ButtonUndo");
        buttonUndo.Connect("pressed", new Callable(this, nameof(OnButtonUndoPressed)));
        
        Button buttonMenu = GetNode<Button>("ButtonMenu");
        buttonMenu.Connect("pressed", new Callable(this, nameof(OnButtonMenuPressed)));
        
        _restartGameDialog.Connect("confirmed", new Callable(this, nameof(NewGame)));
        _goToMenuDialog.Connect("confirmed", new Callable(this, nameof(GoToMenu)));
    }

    private void OnTimeout()
    {
        Card targetCard = (Card)_autoMoveTarget["card"];
        Foundation targetFoundation = (Foundation)_autoMoveTarget["foundation"];
        targetFoundation.AutoMoveCards(new List<Card> { targetCard });
        _autoMovingMap.Remove(targetCard);
        
        if (_autoMovingMap.Count == 0)
        {
            SetAllCardControl(false);
        }
        
        UpdateInformation();
    }

    private void ResetCardsInGame()
    {
        foreach (Freecell freecell in _freecells)
        {
            freecell.ClearCards();
        }
        
        foreach (Foundation foundation in _foundations)
        {
            foundation.ClearCards();
        }
        
        foreach (Tableau tableau in _tableaus)
        {
            tableau.ClearCards();
        }
        
        _startPosition.ClearCards();
        _allCards.Clear();
        _autoMovingMap.Clear();
        _cardManager.ResetHistory();

        if (_cardFactory == null)
        {
            _cardFactory = _cardManager.GetNode<FreecellCardFactory>("FreecellCardFactory");
        }
    }

    private int CountRemainingFreecell()
    {
        int count = 0;
        
        foreach (Freecell freecell in _freecells)
        {
            if (freecell.IsEmpty())
            {
                count++;
            }
        }
        
        return count;
    }

    private int CountRemainingTableaus()
    {
        int count = 0;
        
        foreach (Tableau tableau in _tableaus)
        {
            if (tableau.IsEmpty())
            {
                count++;
            }
        }
        
        return count;
    }

    private async System.Threading.Tasks.Task GenerateCards()
    {
        int[] deck = _gameGenerator.Deal(_gameSeed);
        string[] cardsStr = _gameGenerator.GenerateCards(deck);

        foreach (Tableau tableau in _tableaus)
        {
            tableau.IsInitializing = true;
        }

        for (int i = cardsStr.Length - 1; i >= 0; i--)
        {
            string cardName = (string)cardsStr[i];
            Card card = _cardFactory.CreateCard(cardName, _startPosition);
            _allCards.Add(card);
        }

        int currentIndex = 0;
        int offset = _tableaus.Count;
        
        for (int i = _startPosition._heldCards.Count - 1; i >= 0; i--)
        {
            Card card = _startPosition._heldCards[i];
            Tableau tableau = _tableaus[currentIndex];
            tableau.InitMoveCards(new List<Card> { card }, false);
            currentIndex = (currentIndex + 1) % offset;
            _gameGeneratingTimer.Start(GameGeneratingTimerWaitingTime);
            await ToSignal(_gameGeneratingTimer, "timeout");
        }

        foreach (Tableau tableau in _tableaus)
        {
            tableau.IsInitializing = false;
            UpdateCardsCanBeInteractwith(tableau);
        }
    }

    private void GoToMenu()
    {
        if (_isGameRunning)
        {
            _gameState = GameState.LOSE;
            EndGame();
        }
        
        Node menuInstance = _menuScene.Instantiate();
        GetTree().Root.AddChild(menuInstance);
        GetNode("/root/FreecellGame").QueueFree();
    }

    private void SetAllCardControl(bool disable)
    {
        foreach (Card card in _allCards)
        {
            ((PlayingCard)card).IsStopControl = disable;
        }
    }

    private bool CheckWinCondition()
    {
        foreach (Foundation foundation in _foundations)
        {
            if (foundation._heldCards.Count != 13)
            {
                return false;
            }
        }
        
        return true;
    }

    private bool CheckCardCanBeAnywhere(Card card)
    {
        foreach (Tableau tableau in _tableaus)
        {
            if (tableau.CardCanBeAdded(new List<Card> { card }))
            {
                return true;
            }
        }
        
        foreach (Freecell freecell in _freecells)
        {
            if (freecell.CardCanBeAdded(new List<Card> { card }))
            {
                return true;
            }
        }
        
        foreach (Foundation foundation in _foundations)
        {
            if (foundation.CardCanBeAdded(new List<Card> { card }))
            {
                return true;
            }
        }
        
        return false;
    }

    private bool CheckLoseCondition()
    {
        foreach (Tableau tableau in _tableaus)
        {
            Card topCard = tableau.GetTopCard();
            
            if (topCard == null)
            {
                return false;
            }
            
            if (CheckCardCanBeAnywhere(topCard))
            {
                return false;
            }
        }

        foreach (Freecell freecell in _freecells)
        {
            if (freecell.IsEmpty())
            {
                return false;
            }
            
            if (CheckCardCanBeAnywhere(freecell.GetTopCard()))
            {
                return false;
            }
        }

        return true;
    }

    private void UpdateInformation()
    {
        string text = $"seed: {_gameSeed}" +
                     $",  move: {_moveCount}" +
                     $",  undo: {_undoCount}" +
                     $",  time: {_elapsedTime}" +
                     $",  score: {_score}";

        switch (_gameState)
        {
            case GameState.WIN:
                text += ",  state: win";
                break;
            case GameState.LOSE:
                text += ",  state: lose";
                break;
            case GameState.PLAYING:
                text += ",  state: playing";
                break;
        }

        _information.Text = text;
        _recordManager.SaveRunningGameInfo(_gameSeed, _moveCount, _undoCount, _elapsedTime, _score, _gameState);
    }

    private void ShowResultPopup(bool isWin)
    {
        AcceptDialog dialog = GetNode<AcceptDialog>("ResultDialog");
        RichTextLabel bodyText = dialog.GetNode<RichTextLabel>("BodyText");

        if (isWin)
        {
            dialog.Title = "Congratulations!";
        }
        else
        {
            dialog.Title = "Game Over";
        }

        bodyText.BbcodeEnabled = true;
        bodyText.Clear();

        string winText = isWin ? "[color=green]Win[/color]" : "[color=red]Lose[/color]";

        string textBody = "";
        textBody += $"Result:\t\t{winText}\n";
        textBody += $"Seed:\t\t\t{_gameSeed}\n";
        textBody += $"Time:\t\t\t{_elapsedTime}\n";
        textBody += $"Score:\t\t\t{_score}\n";
        textBody += $"Move:\t\t\t{_moveCount}\n";
        textBody += $"Undo:\t\t\t{_undoCount}\n";

        bodyText.Text = textBody;

        dialog.PopupCentered();
    }
}