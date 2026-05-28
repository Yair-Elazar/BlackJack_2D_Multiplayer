using System;
using System.Collections.Generic;

public class Deck
{
        private Queue<Card> debugQueue;

    private List<Card> cards;
    private static readonly Random rng = new Random();

    public int Count => cards.Count;

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

    public void Shuffle()
    {
        int n = cards.Count;

        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);

            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }

    public Card DrawCard()
{
    if (debugQueue != null && debugQueue.Count > 0)
        return debugQueue.Dequeue();

    if (cards.Count == 0)
        return null;

    Card card = cards[cards.Count - 1];
    cards.RemoveAt(cards.Count - 1);
    return card;
}

    public List<Card> DrawMultiple(int n)
    {
        List<Card> drawn = new List<Card>();

        for (int i = 0; i < n && cards.Count > 0; i++)
        {
            drawn.Add(DrawCard());
        }

        return drawn;
    }

    public void InsertCardOnTop(Card card)
    {
        if (card == null) return;
        cards.Add(card);
    }

    public override string ToString()
    {
        return $"Deck with {Count} cards remaining.";
    }


public void ForceNextCardsForPlayer(Card c1, Card c2, Card c3)
{
    debugQueue = new Queue<Card>();
    debugQueue.Enqueue(c1);
    debugQueue.Enqueue(c2);
    debugQueue.Enqueue(c3);
}
}