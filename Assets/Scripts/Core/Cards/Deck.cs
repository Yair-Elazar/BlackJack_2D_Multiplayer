using System;
using System.Collections.Generic;

/// <summary>
/// Represents a standard 52-card deck for Blackjack.
/// Pure C# implementation prepared for multiplayer extensions.
/// </summary>
public class Deck
{
    private List<Card> cards;
    private static readonly Random rng = new Random();

    /// <summary>
    /// Gets the number of cards remaining in the deck.
    /// </summary>
    public int Count => cards.Count;

    /// <summary>
    /// Initializes a new deck with all 52 cards.
    /// </summary>
    public Deck()
    {
        cards = new List<Card>(52);
        foreach (Card.SuitType suit in Enum.GetValues(typeof(Card.SuitType)))
        {
            foreach (Card.RankType rank in Enum.GetValues(typeof(Card.RankType)))
            {
                cards.Add(new Card(rank, suit));
            }
        }
    }

    /// <summary>
    /// Shuffles the deck using the Fisher-Yates algorithm for randomness.
    /// </summary>
    public void Shuffle()
    {
        int n = cards.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            // Swap cards[i] and cards[j]
            Card temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
    }

    /// <summary>
    /// Draws the top card from the deck and removes it. Returns null if no cards remain.
    /// </summary>
    /// <returns>The top Card, or null if the deck is empty.</returns>
    public Card DrawCard()
    {
        if (cards.Count == 0)
            return null;
        Card topCard = cards[0];
        cards.RemoveAt(0);
        return topCard;
    }

    /// <summary>
    /// Returns a string representation for debugging (e.g., number of cards left).
    /// </summary>
    public override string ToString()
    {
        return $"Deck with {Count} cards remaining.";
    }
    
    // Prepared for future multiplayer serialization/extensions if needed
    // public string ToSerializableFormat() {...}
}
