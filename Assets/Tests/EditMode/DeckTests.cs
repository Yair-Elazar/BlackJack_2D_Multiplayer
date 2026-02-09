using NUnit.Framework;

[TestFixture]
public class DeckTests
{
    [Test]
    public void Deck_InitializesWith52Cards()
    {
        var deck = new Deck();
        Assert.AreEqual(52, deck.Count);
    }

    [Test]
    public void DrawCard_ReducesDeckCountByOne()
    {
        var deck = new Deck();
        int initialCount = deck.Count;
        var card = deck.DrawCard();
        Assert.IsNotNull(card);
        Assert.AreEqual(initialCount - 1, deck.Count);
    }

    [Test]
    public void DrawAllCards_ThenReturnsNull()
    {
        var deck = new Deck();
        for (int i = 0; i < 52; i++)
        {
            Assert.IsNotNull(deck.DrawCard());
        }
        Assert.IsNull(deck.DrawCard());
    }

    [Test]
    public void Shuffle_DoesNotChangeCardCount()
    {
        var deck = new Deck();
        deck.Shuffle();
        Assert.AreEqual(52, deck.Count);
    }
}
