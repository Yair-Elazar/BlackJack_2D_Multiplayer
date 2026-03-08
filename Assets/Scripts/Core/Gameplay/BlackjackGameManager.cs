/// <summary>
/// Manages Blackjack game flow for single-player gameplay.
/// Pure C# implementation with proper state tracking and Blackjack rule handling.
/// Prepared for future multiplayer expansion.
/// </summary>
public class BlackjackGameManager
{
    // Game state enums
    public enum GameState { Betting, PlayerTurn, DealerTurn, Finished }
    public enum RoundResult { PlayerWins, DealerWins, Push, PlayerBlackjack, DealerBlackjack }

    private Deck deck;
    private Player player;
    private Dealer dealer;

    private GameState gameState;
    private RoundResult? roundResult; // null if round not finished yet
    
    // Betting system
    private int playerBalance = 1000; // Starting balance
    private int currentBet = 0;

    /// <summary>
    /// The current player.
    /// </summary>
    public Player Player => player;

    /// <summary>
    /// The current dealer.
    /// </summary>
    public Dealer Dealer => dealer;

    /// <summary>
    /// Current game state.
    /// </summary>
    public GameState CurrentState => gameState;

    /// <summary>
    /// Round result, if the round has finished.
    /// </summary>
    public RoundResult? GetRoundResult() => roundResult;

    /// <summary>
    /// Gets the player's current balance.
    /// </summary>
    public int PlayerBalance => playerBalance;

    /// <summary>
    /// Gets the current bet amount.
    /// </summary>
    public int CurrentBet => currentBet;

    /// <summary>
    /// Starts a new round: resets deck, hands, and prepares for dealing.
    /// Sets state to Betting so player must place a bet first.
    /// </summary>
    public void StartNewRound(string playerName)
    {
        deck = new Deck();
        deck.Shuffle();

        if (player == null || player.Name != playerName)
            player = new Player(playerName);
        else
            player.ResetHand();

        if (dealer == null)
            dealer = new Dealer();
        else
            dealer.ResetHand();

        gameState = GameState.Betting;
        roundResult = null;
        currentBet = 0;
    }

    /// <summary>
    /// Adds a chip value to the current bet. Returns true if successful.
    /// </summary>
    public bool AddToBet(int chipValue)
    {
        if (gameState != GameState.Betting)
            return false;

        if (chipValue <= 0)
            return false;

        int newBet = currentBet + chipValue;
        if (newBet > playerBalance)
            return false; // Can't bet more than balance

        currentBet = newBet;
        return true;
    }

    /// <summary>
    /// Clears the current bet.
    /// </summary>
    public void ClearBet()
    {
        if (gameState == GameState.Betting)
            currentBet = 0;
    }

    /// <summary>
    /// Confirms the bet and starts the round. Returns true if bet was placed successfully.
    /// </summary>
    public bool ConfirmBet()
    {
        if (gameState != GameState.Betting)
            return false;

        if (currentBet <= 0)
            return false; // Must bet something

        if (currentBet > playerBalance)
            return false; // Can't bet more than balance

        // Deduct bet from balance
        playerBalance -= currentBet;
        
        // Change state to PlayerTurn to allow dealing
        gameState = GameState.PlayerTurn;
        return true;
    }

    /// <summary>
    /// Deals one card to the specified target (player or dealer).
    /// Returns the card that was dealt, or null if deck is empty.
    /// </summary>
    public Card DealCardToPlayer()
    {
        if (deck.Count == 0)
            return null;
        
        var card = deck.DrawCard();
        player.Hand.AddCard(card);
        return card;
    }

    /// <summary>
    /// Deals one card to the dealer.
    /// Returns the card that was dealt, or null if deck is empty.
    /// </summary>
    public Card DealCardToDealer()
    {
        if (deck.Count == 0)
            return null;
        
        var card = deck.DrawCard();
        dealer.Hand.AddCard(card);
        return card;
    }

    /// <summary>
    /// Checks for immediate Blackjack after initial dealing is complete.
    /// Should be called after all 4 initial cards are dealt.
    /// </summary>
    public void CheckInitialBlackjack()
    {
        bool playerBJ = IsBlackjack(player.Hand);
        bool dealerBJ = IsBlackjack(dealer.Hand);

        if (playerBJ || dealerBJ)
        {
            gameState = GameState.Finished;

            if (playerBJ && dealerBJ)
                roundResult = RoundResult.Push;
            else if (playerBJ)
                roundResult = RoundResult.PlayerBlackjack;
            else
                roundResult = RoundResult.DealerBlackjack;

            // Update balance for immediate blackjack
            UpdateBalance();
        }
    }

    /// <summary>
    /// Player takes a card. Returns true if player busts.
    /// </summary>
    public bool PlayerHit()
    {
        if (gameState != GameState.PlayerTurn)
            return player.IsBusted;

        player.Hit(deck);

        if (player.IsBusted)
        {
            gameState = GameState.Finished;
            roundResult = RoundResult.DealerWins;
            // Update balance (player loses, bet already deducted)
            UpdateBalance();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Player stands. Dealer plays and round outcome is determined.
    /// </summary>
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
        else if (dealerValue > playerValue)
            roundResult = RoundResult.DealerWins;
        else if (dealerValue < playerValue)
            roundResult = RoundResult.PlayerWins;
        else
            roundResult = RoundResult.Push;

        // Update balance based on result
        UpdateBalance();
    }

    /// <summary>
    /// Updates player balance based on round result.
    /// </summary>
    private void UpdateBalance()
    {
        if (currentBet <= 0)
            return;

        switch (roundResult)
        {
            case RoundResult.PlayerWins:
                // Player wins: get bet back + equal amount (1:1 payout)
                playerBalance += currentBet * 2;
                break;
            case RoundResult.PlayerBlackjack:
                // Blackjack: get bet back + 1.5x bet (3:2 payout)
                playerBalance += currentBet + (currentBet * 3 / 2);
                break;
            case RoundResult.DealerWins:
            case RoundResult.DealerBlackjack:
                // Dealer wins: bet is already deducted, nothing to add
                break;
            case RoundResult.Push:
                // Push: get bet back (no win, no loss)
                playerBalance += currentBet;
                break;
        }
    }

    /// <summary>
    /// Checks if a hand is a Blackjack (21 with exactly 2 cards).
    /// </summary>
    private static bool IsBlackjack(Hand hand)
    {
        return hand.Cards.Count == 2 && hand.GetTotalValue() == 21;
    }
}
