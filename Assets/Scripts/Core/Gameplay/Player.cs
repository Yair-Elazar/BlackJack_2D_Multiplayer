/// <summary>
/// Represents a player in a Blackjack game.
/// Encapsulates player identity and hand management. Prepared for multiplayer usage and extension.
/// </summary>
public class Player
{
    /// <summary>
    /// The player's display name.
    /// </summary>
    public string Name { get; private set; }

    private readonly Hand hand = new Hand();

    /// <summary>
    /// Gets the player's current hand (read-only).
    /// </summary>
    public Hand Hand => hand;

    /// <summary>
    /// Returns true if the player's hand value exceeds 21.
    /// </summary>
    public bool IsBusted => hand.IsBust();

    /// <summary>
    /// Returns true if the player's hand value is exactly 21 (Blackjack).
    /// </summary>
    public bool HasBlackjack => hand.GetTotalValue() == 21;

    /// <summary>
    /// Initializes a new player with the specified name.
    /// </summary>
    /// <param name="name">Player's display name.</param>
    public Player(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Draws a card from the deck and adds it to the player's hand.
    /// </summary>
    /// <param name="deck">The deck to draw from.</param>
    public void Hit(Deck deck)
    {
        var card = deck.DrawCard();
        if (card != null)
            hand.AddCard(card);
    }

    /// <summary>
    /// Placeholder for stand behavior, e.g., ending turn.
    /// </summary>
    public void Stand()
    {
        // Logic for standing (end turn) can be implemented here for multiplayer turn management
    }

    /// <summary>
    /// Clears the player's hand for a new round.
    /// </summary>
    public void ResetHand()
    {
        hand.ResetHand();
    }

    // Prepared for future multiplayer serialization/extensions
    // public string ToSerializableFormat() {...}
}
