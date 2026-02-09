using NUnit.Framework;

[TestFixture]
public class BlackjackGameManagerTests
{
    [Test]
    public void NewRound_DealsTwoCardsToPlayerAndDealer()
    {
        var manager = new BlackjackGameManager();
        manager.StartNewRound("TestPlayer");
        Assert.AreEqual(2, manager.Player.Hand.Cards.Count);
        Assert.AreEqual(2, manager.Dealer.Hand.Cards.Count);
    }

    [Test]
    public void PlayerHit_IncreasesPlayerCardCount()
    {
        var manager = new BlackjackGameManager();
        manager.StartNewRound("Tester");
        int before = manager.Player.Hand.Cards.Count;
        manager.PlayerHit();
        int after = manager.Player.Hand.Cards.Count;
        Assert.AreEqual(before + 1, after);
    }

    [Test]
    public void PlayerBust_EndsRoundWithDealerWins()
    {
        var manager = new BlackjackGameManager();
        manager.StartNewRound("Buster");
        // Manually set hand to bust
        var hand = manager.Player.Hand;
        hand.AddCard(new Card(Card.RankType.King, Card.SuitType.Spades));
        hand.AddCard(new Card(Card.RankType.Queen, Card.SuitType.Hearts));
        hand.AddCard(new Card(Card.RankType.Two, Card.SuitType.Diamonds));
        // Next hit triggers check
        manager.PlayerHit();
        Assert.AreEqual(BlackjackGameManager.GameState.Finished, manager.CurrentState);
        Assert.AreEqual(BlackjackGameManager.RoundResult.DealerWins, manager.GetRoundResult());
    }

    [Test]
    public void PlayerStand_TriggersDealerTurnAndFinishesRound()
    {
        var manager = new BlackjackGameManager();
        manager.StartNewRound("DealerDealer");
        manager.PlayerStand();
        Assert.AreEqual(BlackjackGameManager.GameState.Finished, manager.CurrentState);
        Assert.IsNotNull(manager.GetRoundResult());
    }

    [Test]
    public void ImmediateBlackjack_DetectedCorrectly()
    {
        var manager = new BlackjackGameManager();
        manager.StartNewRound("BlackjackPlayer");
        var playerValue = manager.Player.Hand.GetTotalValue();
        var dealerValue = manager.Dealer.Hand.GetTotalValue();
        if (playerValue == 21 && manager.Player.Hand.Cards.Count == 2)
        {
            Assert.AreEqual(BlackjackGameManager.RoundResult.PlayerBlackjack, manager.GetRoundResult());
        }
        if (dealerValue == 21 && manager.Dealer.Hand.Cards.Count == 2)
        {
            Assert.AreEqual(BlackjackGameManager.RoundResult.DealerBlackjack, manager.GetRoundResult());
        }
    }
}
