/// <summary>
/// Represents the dealer in a game of Blackjack.
/// Encapsulates dealer's hand and standard dealer behavior, including drawing until at least 17.
/// Prepared for future multiplayer extensions.
/// </summary>
public class Dealer
{
    private readonly Hand hand = new Hand();

    /// <summary>
    /// Gets the dealer's hand (read-only).
    /// </summary>
    public Hand Hand => hand;

    /// <summary>
    /// Simulates the dealer's turn: draw cards until total value is at least 17,
    /// following standard Blackjack dealer rules (soft 17 stands).
    /// </summary>
    /// <param name="deck">The deck from which to draw cards.</param>
    public void PlayTurn(Deck deck)
    {
        while (hand.GetTotalValue() < 17 && !hand.IsBust())
        {
            var card = deck.DrawCard();
            if (card == null)
                break; // No more cards to draw
            hand.AddCard(card);
        }
    }

    /// <summary>
    /// Returns the dealer's hand.
    /// </summary>
    /// <returns>The Hand object representing the dealer's cards.</returns>
    public Hand RevealHand()
    {
        return hand;
    }
    
    /// <summary>
    /// Resets the dealer's hand for a new round.
    /// </summary>
    public void ResetHand()
    {
        hand.ResetHand();
    }
    
    // Prepared for future multiplayer serialization/extensions
    // public string ToSerializableFormat() {...}
}
