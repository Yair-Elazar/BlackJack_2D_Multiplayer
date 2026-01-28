/// <summary>
/// Represents a playing card with a rank and suit, used in Blackjack.
/// Clean, pure C# class with support for Blackjack value calculation.
/// Prepared for future multiplayer serialization.
/// </summary>
public class Card
{
    /// <summary>
    /// Enumerates the possible suits of a standard deck.
    /// </summary>
    public enum SuitType
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }

    /// <summary>
    /// Enumerates the possible ranks in a standard deck.
    /// </summary>
    public enum RankType
    {
        Ace = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    /// <summary>
    /// The suit of the card.
    /// </summary>
    public SuitType Suit { get; private set; }

    /// <summary>
    /// The rank of the card.
    /// </summary>
    public RankType Rank { get; private set; }

    /// <summary>
    /// Constructs a card with the given rank and suit.
    /// </summary>
    /// <param name="rank">Rank of the card</param>
    /// <param name="suit">Suit of the card</param>
    public Card(RankType rank, SuitType suit)
    {
        Rank = rank;
        Suit = suit;
    }

    /// <summary>
    /// Gets the standard Blackjack value of the card.
    /// For Aces, returns 1 (caller must check for 11).
    /// Face cards (J, Q, K) return 10.
    /// </summary>
    public int GetValue()
    {
        if (Rank == RankType.Ace)
            return 1; // Ace can be 1 or 11, caller will decide
        if (Rank >= RankType.Jack && Rank <= RankType.King)
            return 10;
        return (int)Rank;
    }
    
    public int MaxValue
    {
        get
        {
            return Rank == RankType.Ace ? 11 : GetValue();
        }
    }

    /// <summary>
    /// Returns a string representation for debugging (e.g., "Ace of Spades").
    /// </summary>
    public override string ToString()
    {
        return $"{Rank} of {Suit}";
    }
    
    // Prepared for future multiplayer serialization if needed
    // public string ToSerializableFormat() {...}
}