/// <summary>
/// Manages Blackjack game flow for single-player gameplay.
/// Pure C# implementation using clean architecture principles.
/// Prepared for future multiplayer expansion.
/// </summary>
public class BlackjackGameManager
{
    private Deck deck;
    private Player player;
    private Dealer dealer;
    private bool isPlayerTurn;

    /// <summary>
    /// Gets the current player.
    /// </summary>
    public Player Player => player;

    /// <summary>
    /// Gets the current dealer.
    /// </summary>
    public Dealer Dealer => dealer;

    /// <summary>
    /// Starts a new round: resets deck, hands, and deals initial cards.
    /// </summary>
    /// <param name="playerName">Name for the player (created if not exists).</param>
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

        isPlayerTurn = true;

        // Deal two cards to player and dealer each
        for (int i = 0; i < 2; i++)
        {
            player.Hit(deck);
            dealer.Hand.AddCard(deck.DrawCard());
        }
    }

    /// <summary>
    /// Player chooses to take a card (Hit).
    /// Returns true if the player busts as a result.
    /// </summary>
    public bool PlayerHit()
    {
        if (!isPlayerTurn)
            return player.IsBusted;

        player.Hit(deck);
        if (player.IsBusted)
            isPlayerTurn = false;
        return player.IsBusted;
    }

    /// <summary>
    /// Player stands; dealer plays turn.
    /// </summary>
    public void PlayerStand()
    {
        if (!isPlayerTurn)
            return;

        isPlayerTurn = false;
        dealer.PlayTurn(deck);
    }

    /// <summary>
    /// Checks the outcome of the current round.
    /// Returns "Player Wins", "Dealer Wins", or "Push" (tie) based on Blackjack rules.
    /// </summary>
    public string CheckOutcome()
    {
        int playerValue = player.Hand.GetTotalValue();
        int dealerValue = dealer.Hand.GetTotalValue();
        bool playerBusted = player.IsBusted;
        bool dealerBusted = dealer.Hand.IsBust();

        if (playerBusted)
            return "Dealer Wins";
        if (dealerBusted)
            return "Player Wins";
        if (playerValue > dealerValue)
            return "Player Wins";
        if (dealerValue > playerValue)
            return "Dealer Wins";
        return "Push";
    }

    // Prepared for future multiplayer serialization/extensions
    // public string ToSerializableFormat() {...}
}
