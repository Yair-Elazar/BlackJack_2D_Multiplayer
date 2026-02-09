using NUnit.Framework;

[TestFixture]
public class HandTests
{
    [Test]
    public void EmptyHand_ValueIsZero()
    {
        var hand = new Hand();
        Assert.AreEqual(0, hand.GetTotalValue());
    }

    [Test]
    public void Hand_NoAce_SumsCorrectly()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Card.RankType.Nine, Card.SuitType.Spades));
        hand.AddCard(new Card(Card.RankType.Seven, Card.SuitType.Hearts));
        Assert.AreEqual(16, hand.GetTotalValue());
    }

    [Test]
    public void Hand_SingleAce_TreatedAs11WhenPossible()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Card.RankType.Ace, Card.SuitType.Clubs));
        hand.AddCard(new Card(Card.RankType.Eight, Card.SuitType.Diamonds));
        Assert.AreEqual(19, hand.GetTotalValue());
    }

    [Test]
    public void Hand_MultipleAces_OptimallyAdjusted()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Card.RankType.Ace, Card.SuitType.Clubs));
        hand.AddCard(new Card(Card.RankType.Ace, Card.SuitType.Diamonds));
        hand.AddCard(new Card(Card.RankType.Nine, Card.SuitType.Hearts));
        Assert.AreEqual(21, hand.GetTotalValue());
        hand.AddCard(new Card(Card.RankType.King, Card.SuitType.Spades));
        Assert.AreEqual(21, hand.GetTotalValue());
    }

    [Test]
    public void Hand_BustDetection_WorksAsExpected()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Card.RankType.Ten, Card.SuitType.Clubs));
        hand.AddCard(new Card(Card.RankType.Nine, Card.SuitType.Hearts));
        hand.AddCard(new Card(Card.RankType.Four, Card.SuitType.Spades));
        Assert.IsTrue(hand.IsBust());
    }
}
