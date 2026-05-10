public class BlackjackGameManager
{
    public enum GameState
    {
        Betting,
        PlayerTurn,
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

    private Deck deck;
    private Player player;
    private Dealer dealer;

    private GameState gameState;
    private RoundResult? roundResult;

    private int playerBalance;
    private int currentBet;

    public Player Player => player;
    public Dealer Dealer => dealer;

    public GameState CurrentState => gameState;
    public RoundResult? GetRoundResult() => roundResult;

    public int PlayerBalance => playerBalance;
    public int CurrentBet => currentBet;

    public void SetBalance(int balance)
    {
        playerBalance = balance;
    }

    public void StartNewRound(string playerId)
    {
        deck = new Deck();
        deck.Shuffle();

        if (player == null)
            player = new Player(playerId, playerId);
        else
            player.ResetHand();

        if (dealer == null)
            dealer = new Dealer();
        else
            dealer.ResetHand();

        roundResult = null;
        currentBet = 0;

        gameState = GameState.Betting;
    }

    public void PrepareNextRound()
    {
        if (gameState != GameState.Finished)
            return;

        player.ResetHand();
        dealer.ResetHand();

        roundResult = null;
        currentBet = 0;

        gameState = GameState.Betting;
    }

    // =========================
    // BETTING
    // =========================

    public bool AddToBet(int amount)
    {
        if (gameState != GameState.Betting)
            return false;

        if (amount <= 0)
            return false;

        if (currentBet + amount > playerBalance)
            return false;

        currentBet += amount;

        return true;
    }

    public void ClearBet()
    {
        if (gameState != GameState.Betting)
            return;

        currentBet = 0;
    }

    public bool ConfirmBet()
    {
        if (gameState != GameState.Betting)
            return false;

        if (currentBet <= 0)
            return false;

        if (currentBet > playerBalance)
            return false;

        // מוריד כסף מהמאזן
        playerBalance -= currentBet;

        gameState = GameState.PlayerTurn;

        return true;
    }

    // =========================
    // DEALING
    // =========================

    public Card DealCardToPlayer()
    {
        if (deck.Count == 0)
            return null;

        Card card = deck.DrawCard();

        player.Hand.AddCard(card);

        return card;
    }

    public Card DealCardToDealer()
    {
        if (deck.Count == 0)
            return null;

        Card card = deck.DrawCard();

        dealer.Hand.AddCard(card);

        return card;
    }

    // =========================
    // GAME FLOW
    // =========================

    public void CheckInitialBlackjack()
    {
        bool playerBJ = IsBlackjack(player.Hand);
        bool dealerBJ = IsBlackjack(dealer.Hand);

        if (!playerBJ && !dealerBJ)
            return;

        gameState = GameState.Finished;

        if (playerBJ && dealerBJ)
            roundResult = RoundResult.Push;
        else if (playerBJ)
            roundResult = RoundResult.PlayerBlackjack;
        else
            roundResult = RoundResult.DealerBlackjack;

        UpdateBalance();
    }

    public bool PlayerHit()
{
    if (gameState != GameState.PlayerTurn)
        return false;

    // 🚨 NEW: block if 21 or above
    if (player.Hand.GetTotalValue() >= 21)
        return false;

    player.Hit(deck);

    if (player.IsBusted)
    {
        gameState = GameState.Finished;
        roundResult = RoundResult.DealerWins;
        UpdateBalance();
        return true;
    }

    // 🚨 NEW: auto stand on 21
    if (player.Hand.GetTotalValue() == 21)
    {
        PlayerStand();
        return true;
    }

    return false;
}

    public void PlayerStand()
    {
        if (gameState != GameState.PlayerTurn)
            return;

        gameState = GameState.DealerTurn;

        dealer.PlayTurn(deck);

        int playerValue = player.Hand.GetTotalValue();
        int dealerValue = dealer.Hand.GetTotalValue();

        bool dealerBusted = dealer.Hand.IsBust();

        gameState = GameState.Finished;

        if (dealerBusted)
            roundResult = RoundResult.PlayerWins;
        else if (playerValue > dealerValue)
            roundResult = RoundResult.PlayerWins;
        else if (dealerValue > playerValue)
            roundResult = RoundResult.DealerWins;
        else
            roundResult = RoundResult.Push;

        UpdateBalance();
    }

    // =========================
    // BALANCE
    // =========================

    private void UpdateBalance()
    {
        if (currentBet <= 0)
            return;

        switch (roundResult)
        {
            case RoundResult.PlayerWins:
                playerBalance += currentBet * 2;
                break;

            case RoundResult.PlayerBlackjack:
                playerBalance += currentBet + (currentBet * 3 / 2);
                break;

            case RoundResult.Push:
                playerBalance += currentBet;
                break;
        }
    }

    private bool IsBlackjack(Hand hand)
    {
        return hand.Cards.Count == 2 &&
               hand.GetTotalValue() == 21;
    }
}