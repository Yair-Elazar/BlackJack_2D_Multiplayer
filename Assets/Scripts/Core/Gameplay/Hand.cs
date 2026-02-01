using System.Collections.Generic;
using System.Text;

/// <summary>
/// Represents a Blackjack hand, containing multiple cards. Handles card addition,
/// value calculation with Ace adjustment, and hand bust logic. Prepared for multiplayer extension.
/// </summary>
public class Hand
{
    private readonly List<Card> cards = new List<Card>();

    /// <summary>

// Adds a card to the hand.
    /// </summary>
    /// <param name="card">The Card to add.</param>
    public void AddCard(Card card)
    {
        if (card != null)
            cards.Add(card);
    }

    /// <summary>
    /// Calculates the total Blackjack value of the hand, adjusting Aces as needed.
    /// </summary>
    /// <returns>Total value of the hand with optimal Ace handling.</returns>
    public int GetTotalValue()
    {
        int total = 0;
        int aceCount = 0;
        foreach (var card in cards)
        {
            total += card.MaxValue;
            if (card.Rank == Card.RankType.Ace)
                aceCount++;
        }
        // Adjust Aces from 11 down to 1 if busting
        while (total > 21 && aceCount > 0)
        {
            total -= 10; // Change an Ace from 11 to 1
            aceCount--;
        }
        return total;
    }

    /// <summary>
    /// Returns true if the hand is bust (value exceeds 21), otherwise false.
    /// </summary>
    public bool IsBust()
    {
        return GetTotalValue() > 21;
    }

    /// <summary>
    /// Returns a string representation of the hand and its total value.
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < cards.Count; i++)
        {
            sb.Append(cards[i].ToString());
            if (i != cards.Count - 1)
                sb.Append(", ");
        }
        sb.Append($" (Total: {GetTotalValue()})");
        return sb.ToString();
    }

    public void ResetHand()
    {
    cards.Clear();
    }

    /// <summary>
    /// Gets the list of cards in the hand. (Read-only)
    /// </summary>
    public IReadOnlyList<Card> Cards => cards;
    
    // Prepared for future multiplayer serialization/extensions
    // public string ToSerializableFormat() {...}
}
