using NUnit.Framework;

[TestFixture]
public class BlackjackGameManagerTests
{
    [Test]
    public void NewRound_DealsTwoCardsToPlayerAndDealer()
    {
        var manager = new BlackjackGameManager();

        manager.StartNewRound("TestPlayer");

        Assert.AreEqual(2, manager.Player.Hands[0].Hand.Cards.Count);
        Assert.AreEqual(2, manager.Dealer.Hand.Cards.Count);
    }

    [Test]
    public void PlayerHit_IncreasesPlayerCardCount()
    {
        var manager = new BlackjackGameManager();

        manager.StartNewRound("Tester");

        int before = manager.Player.Hands[0].Hand.Cards.Count;

        manager.PlayerHit(0);

        int after = manager.Player.Hands[0].Hand.Cards.Count;

        Assert.AreEqual(before + 1, after);
    }

    [Test]
    public void PlayerBust_EndsRoundWithDealerWins()
    {
        var manager = new BlackjackGameManager();

        manager.StartNewRound("Buster");

        var hand = manager.Player.Hands[0].Hand;

        hand.AddCard(new Card(Card.RankType.King, Card.SuitType.Spades));
        hand.AddCard(new Card(Card.RankType.Queen, Card.SuitType.Hearts));
        hand.AddCard(new Card(Card.RankType.Two, Card.SuitType.Diamonds));

        manager.PlayerHit(0);

        Assert.AreEqual(BlackjackGameManager.GameState.Finished, manager.CurrentState);
        Assert.AreEqual(BlackjackGameManager.RoundResult.DealerWins, manager.GetRoundResult());
    }

    [Test]
public void DealerTurn_FinishesRound()
{
    var manager = new BlackjackGameManager();

    manager.StartNewRound("DealerDealer");

    manager.ConfirmBet();

    manager.DealCardToPlayer(0);
    manager.DealCardToDealer();

    manager.DealCardToPlayer(0);
    manager.DealCardToDealer();

    manager.PlayerStand();

    Assert.AreEqual(
        BlackjackGameManager.GameState.Finished,
        manager.CurrentState
    );
}

    [Test]
    public void ImmediateBlackjack_DetectedCorrectly()
    {
        var manager = new BlackjackGameManager();

        manager.StartNewRound("BlackjackPlayer");

        var playerHand = manager.Player.Hands[0].Hand;
        var dealerHand = manager.Dealer.Hand;

        if (playerHand.GetTotalValue() == 21 && playerHand.Cards.Count == 2)
        {
            Assert.AreEqual(BlackjackGameManager.RoundResult.PlayerBlackjack, manager.GetRoundResult());
        }

        if (dealerHand.GetTotalValue() == 21 && dealerHand.Cards.Count == 2)
        {
            Assert.AreEqual(BlackjackGameManager.RoundResult.DealerBlackjack, manager.GetRoundResult());
        }
    }
}