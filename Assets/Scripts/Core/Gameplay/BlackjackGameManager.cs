using System.Collections.Generic;
using UnityEngine;


public class BlackjackGameManager
{
    public enum GameState
    {
        Betting,
        PlayerTurn,
        SplitTurn,
        DealerTurn,
        Finished
    }

    public enum RoundResult
    {
        PlayerWins,
        DealerWins,
        Push,
        PlayerBlackjack,
        DealerBlackjack
    }

    public class HandResult
{
    public int Bet;
    public RoundResult Result;
}

    private Deck deck;
    private Player player;
    private Dealer dealer;

    private GameState gameState;
    private RoundResult? roundResult;

    private List<RoundResult> splitResults = new();

    private int playerBalance;
    private int currentBet;
    private int lastPayout = 0;

public int GetLastPayout() => lastPayout;

    public Player Player => player;
    public Dealer Dealer => dealer;
    public GameState CurrentState => gameState;

    public RoundResult? GetRoundResult() => roundResult;
    private List<HandResult> results = new();
    private bool hasResolved = false;
    private bool payoutApplied = false;

    public int PlayerBalance => playerBalance;
    public int CurrentBet => currentBet;

    // ================= INIT =================

    public void SetBalance(int balance)
    {
        playerBalance = balance;
    }

    public void StartNewRound(string playerId)
    {
        deck = new Deck();
        deck.Shuffle();

        deck.ForceNextCardsForPlayer(
    new Card(Card.RankType.Eight, Card.SuitType.Hearts),
    new Card(Card.RankType.Eight, Card.SuitType.Spades),
    new Card(Card.RankType.Eight, Card.SuitType.Diamonds)
);
        player = new Player(playerId, playerId);
        dealer = new Dealer();

        roundResult = null;
        splitResults.Clear();

        currentBet = 0;
        payoutApplied = false;

        gameState = GameState.Betting;
    }

    // ================= BET =================

    public void ClearBet()
    {
        if (gameState != GameState.Betting) return;
        currentBet = 0;
    }

    public bool AddToBet(int amount)
    {
        if (gameState != GameState.Betting) return false;
        if (amount <= 0) return false;
        if (currentBet + amount > playerBalance) return false;

        currentBet += amount;
        return true;
    }

    public bool ConfirmBet()
{
    if (gameState != GameState.Betting)
        return false;

    if (currentBet <= 0)
        return false;

    if (currentBet > playerBalance)
        return false;

    playerBalance -= currentBet;

    player.ResetHand(); // או StartNewRound כבר עושה, תלוי ב-flow

    // 🔥 CRITICAL FIX
    player.Hands[0].SetBet(currentBet);

    gameState = GameState.PlayerTurn;

    return true;
}


private void ApplyPayout()
{
    if (payoutApplied) return;
    payoutApplied = true;

    Debug.Log("RESULTS COUNT: " + results.Count);

    foreach (var r in results)
    {
        switch (r.Result)
        {
            case RoundResult.PlayerWins:
            Debug.Log("BET: " + r.Bet);
Debug.Log("RESULT: " + r.Result);
Debug.Log("BAL BEFORE: " + playerBalance);
                playerBalance += r.Bet * 2;
                break;

            case RoundResult.PlayerBlackjack:
            Debug.Log("BET: " + r.Bet);
Debug.Log("RESULT: " + r.Result);
Debug.Log("BAL BEFORE: " + playerBalance);
                playerBalance += r.Bet + (r.Bet * 3 / 2);
                break;

            case RoundResult.Push:
            Debug.Log("BET: " + r.Bet);
Debug.Log("RESULT: " + r.Result);
Debug.Log("BAL BEFORE: " + playerBalance);
                playerBalance += r.Bet;
                break;
        }
    }

    results.Clear();
}

    

    // ================= DEAL =================

    public Card DealCardToPlayer(int handIndex)
{
    EnsureDeck();

    Card c = deck.DrawCard();

    player.Hands[handIndex].Hand.AddCard(c);

    return c;
}

    public Card DealCardToDealer()
    {
        EnsureDeck();
        Card c = deck.DrawCard();
        dealer.Hand.AddCard(c);
        return c;
    }

    // ================= HIT =================

    public bool PlayerHit(int index)
    {
        var hand = player.Hands[index];

        Card c = DrawSafe();
        hand.Hand.AddCard(c);

        if (hand.Hand.IsBust())
        {
            hand.Finish();

            if (AllHandsDone())
                FinishRound(RoundResult.DealerWins);
        }

        return hand.Hand.IsBust();
    }

    // ================= STAND =================

    public void PlayerStand()
    {
        player.MoveToNextHand();

        if (AllHandsDone())
        {
            StartDealerTurn();
        }
        else
        {
            gameState = GameState.SplitTurn;
        }
    }

    // ================= DEALER FLOW =================

    public void StartDealerTurn()
    {
        gameState = GameState.DealerTurn;

        dealer.PlayTurn(deck);

        Resolve();

        gameState = GameState.Finished;
    }

    // ================= SPLIT =================

    public bool CanSplit()
    {
        var h = player.Hands[0];

        return player.Hands.Count == 1 &&
               h.Hand.Cards.Count == 2 &&
               h.Hand.Cards[0].Rank == h.Hand.Cards[1].Rank;
    }

    public bool SplitPlayerHand()
    {
        if (!CanSplit()) return false;

        // 🔥 צריך מספיק כסף לעוד יד
if (playerBalance < currentBet)
    return false;

// 🔥 תשלום על היד השנייה
playerBalance -= currentBet;

        var original = player.Hands[0];

        Card c1 = original.Hand.RemoveCardAt(0);
        Card c2 = original.Hand.RemoveCardAt(0);

        player.Hands.Clear();

        var h1 = new HandState(new Hand());
        var h2 = new HandState(new Hand());

        h1.SetBet(currentBet);
        h2.SetBet(currentBet);

        h1.Hand.AddCard(c1);
        h2.Hand.AddCard(c2);

        h1.SetActive(true);
        h2.SetActive(false); 

        player.Hands.Add(h1);
        player.Hands.Add(h2);

        // 🔥 נותנים קלף חדש לכל יד אחרי Split
        h1.Hand.AddCard(DrawSafe());
        h2.Hand.AddCard(DrawSafe());

        return true;
    }

    // ================= RESOLVE =================

    private void Resolve()
{
    if (hasResolved) return;

    hasResolved = true;
    results.Clear();

    int dealerVal = dealer.Hand.GetTotalValue();
    bool dealerBust = dealer.Hand.IsBust();

    for (int i = 0; i < player.Hands.Count; i++)
{
    var hand = player.Hands[i];

    int bet = hand.Bet; // ONLY source of truth

    int val = hand.Hand.GetTotalValue();

    RoundResult result;

    if (hand.Hand.IsBust())
        result = RoundResult.DealerWins;
    else if (dealerBust)
        result = RoundResult.PlayerWins;
    else if (val > dealerVal)
        result = RoundResult.PlayerWins;
    else if (val < dealerVal)
        result = RoundResult.DealerWins;
    else
        result = RoundResult.Push;

    results.Add(new HandResult
    {
        Bet = bet,
        Result = result
    });

    // חשוב: כאן אתה יכול גם לעדכן כסף, אבל לא חובה
}

    roundResult = results[0].Result;
}

    private void FinishRound(RoundResult result)
    {
        roundResult = result;
        gameState = GameState.Finished;
    }

    // ================= RESET SYSTEM (FIX חשוב) =================

    public void FinishAndResetRound()
    {
        if (gameState != GameState.Finished)
            return;

        ResetAll();

        gameState = GameState.Betting;
    }

    public void EndRound()
{
    Resolve(); // תמיד קודם

    ApplyPayout();

    ResetAll();
    gameState = GameState.Betting;
}

    private void ResetAll()
{
    player.ResetHand();
    dealer.ResetHand();

    results.Clear(); 
    roundResult = null;
    currentBet = 0;
    hasResolved = false;
    payoutApplied = false;
}

    // ================= HELPERS =================

    private bool AllHandsDone()
    {
        foreach (var h in player.Hands)
            if (h.IsActive) return false;

        return true;
    }

    private void EnsureDeck()
    {
        if (deck == null || deck.Count == 0)
        {
            deck = new Deck();
            deck.Shuffle();
        }
    }

    private Card DrawSafe()
    {
        EnsureDeck();
        return deck.DrawCard();
    }
}