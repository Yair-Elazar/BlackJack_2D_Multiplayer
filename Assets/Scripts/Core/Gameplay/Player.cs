using System.Collections.Generic;

public class Player
{
    public string Name { get; private set; }
    public string Id { get; private set; }

    public List<HandState> Hands { get; private set; } = new();

    public HandState ActiveHand
    {
        get
        {
            return Hands.Find(h => h.IsActive) ?? Hands[0];
        }
    }

    public Player(string id, string name)
    {
        Id = id;
        Name = name;

        ResetHand();
    }

    public void ResetHand()
    {
        Hands.Clear();

        var hand = new HandState(new Hand());
        hand.SetActive(true);

        Hands.Add(hand);
    }

    // =========================
    // SPLIT CHECK
    // =========================
    public bool CanSplit()
    {
        if (Hands.Count != 1)
            return false;

        var cards = Hands[0].Cards;

        if (cards.Count != 2)
            return false;

        return cards[0].Rank == cards[1].Rank;
    }

    public bool SplitHand()
    {
        if (!CanSplit())
            return false;

        var original = Hands[0];

        Card second = original.Hand.RemoveCardAt(1);
        Card first = original.Hand.RemoveCardAt(0);

        var hand1 = new HandState(new Hand());
        var hand2 = new HandState(new Hand());

        hand1.Hand.AddCard(first);
        hand2.Hand.AddCard(second);

        hand1.SetActive(true);

        Hands.Clear();
        Hands.Add(hand1);
        Hands.Add(hand2);

        return true;
    }

    public void MoveToNextHand()
    {
        int index = Hands.FindIndex(h => h.IsActive);

        if (index == -1)
            return;

        Hands[index].SetActive(false);

        if (index + 1 < Hands.Count)
            Hands[index + 1].SetActive(true);
    }
}