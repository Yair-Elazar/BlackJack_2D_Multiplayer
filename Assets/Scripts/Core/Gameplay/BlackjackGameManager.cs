/// <summary>
/// Manages Blackjack game flow for single-player gameplay.
/// Pure C# implementation with proper state tracking and Blackjack rule handling.
/// Prepared for future multiplayer expansion.
/// </summary>
public class BlackjackGameManager
{
    // Game state enums
    public enum GameState { PlayerTurn, DealerTurn, Finished }
    public enum RoundResult { PlayerWins, DealerWins, Push, PlayerBlackjack, DealerBlackjack }

    private Deck deck;
    private Player player;
    private Dealer dealer;

    private GameState gameState;
    private RoundResult? roundResult; // null if round not finished yet

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
    /// Starts a new round: resets deck, hands, and prepares for dealing.
    /// Cards should be dealt using DealInitialCards() for animated dealing.
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

        gameState = GameState.PlayerTurn;
        roundResult = null;
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
    }

    /// <summary>
    /// Checks if a hand is a Blackjack (21 with exactly 2 cards).
    /// </summary>
    private static bool IsBlackjack(Hand hand)
    {
        return hand.Cards.Count == 2 && hand.GetTotalValue() == 21;
    }
}
