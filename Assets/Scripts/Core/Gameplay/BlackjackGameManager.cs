public class BlackjackGameManager
{
    public enum GameState { Betting, PlayerTurn, DealerTurn, Finished }
    public enum RoundResult { PlayerWins, DealerWins, Push, PlayerBlackjack, DealerBlackjack }

    private Deck deck;
    private Player player;
    private Dealer dealer;

    private GameState gameState;
    private RoundResult? roundResult;

    private int playerBalance = 0; // ❌ ברירת מחדל 0 כדי למנוע איפוס ל-10
    private int currentBet = 0;

    public Player Player => player;
    public Dealer Dealer => dealer;
    public GameState CurrentState => gameState;
    public RoundResult? GetRoundResult() => roundResult;
    public int PlayerBalance => playerBalance;
    public int CurrentBet => currentBet;

    public void SetBalance(int balance) => playerBalance = balance; // לטעינת balance מה-UIManager
    public void SetCurrentBet(int bet) => currentBet = bet;
    public (int balance, int bet) GetPlayerData() => (playerBalance, currentBet);

    public void StartNewRound(string playerId)
    {
        deck = new Deck();
        deck.Shuffle();

        if (player == null) player = new Player(playerId, playerId);
        else player.ResetHand();

        if (dealer == null) dealer = new Dealer();
        else dealer.ResetHand();

        gameState = GameState.Betting;
        roundResult = null;
        currentBet = 0;
    }

    public void PrepareNextRound()
    {
        if (gameState != GameState.Finished) return;

        player.ResetHand();
        dealer.ResetHand();
        roundResult = null;
        currentBet = 0;
        gameState = GameState.Betting;
    }

    public bool AddToBet(int chipValue)
    {
        if (gameState != GameState.Betting || chipValue <= 0) return false;
        if (currentBet + chipValue > playerBalance) return false;

        currentBet += chipValue;
        return true;
    }

    public void ClearBet()
    {
        if (gameState == GameState.Betting) currentBet = 0;
    }

    public bool ConfirmBet()
    {
        if (gameState != GameState.Betting || currentBet <= 0 || currentBet > playerBalance) return false;

        playerBalance -= currentBet;
        gameState = GameState.PlayerTurn;
        return true;
    }

    public Card DealCardToPlayer()
    {
        if (deck.Count == 0) return null;
        var card = deck.DrawCard();
        player.Hand.AddCard(card);
        return card;
    }

    public Card DealCardToDealer()
    {
        if (deck.Count == 0) return null;
        var card = deck.DrawCard();
        dealer.Hand.AddCard(card);
        return card;
    }

    public void CheckInitialBlackjack()
    {
        bool playerBJ = IsBlackjack(player.Hand);
        bool dealerBJ = IsBlackjack(dealer.Hand);

        if (playerBJ || dealerBJ)
        {
            gameState = GameState.Finished;

            if (playerBJ && dealerBJ) roundResult = RoundResult.Push;
            else if (playerBJ) roundResult = RoundResult.PlayerBlackjack;
            else roundResult = RoundResult.DealerBlackjack;

            UpdateBalance();
        }
    }

    public bool PlayerHit()
    {
        if (gameState != GameState.PlayerTurn) return player.IsBusted;

        player.Hit(deck);

        if (player.IsBusted)
        {
            gameState = GameState.Finished;
            roundResult = RoundResult.DealerWins;
            UpdateBalance();
            return true;
        }

        return false;
    }

    public void PlayerStand()
    {
        if (gameState != GameState.PlayerTurn) return;

        gameState = GameState.DealerTurn;
        dealer.PlayTurn(deck);

        int playerValue = player.Hand.GetTotalValue();
        int dealerValue = dealer.Hand.GetTotalValue();
        bool dealerBusted = dealer.Hand.IsBust();

        gameState = GameState.Finished;

        if (dealerBusted) roundResult = RoundResult.PlayerWins;
        else if (dealerValue > playerValue) roundResult = RoundResult.DealerWins;
        else if (dealerValue < playerValue) roundResult = RoundResult.PlayerWins;
        else roundResult = RoundResult.Push;

        UpdateBalance();
    }

    private void UpdateBalance()
    {
        if (currentBet <= 0) return;

        switch (roundResult)
        {
            case RoundResult.PlayerWins:
                playerBalance += currentBet * 2;
                break;
            case RoundResult.PlayerBlackjack:
                playerBalance += currentBet + (currentBet * 3 / 2);
                break;
            case RoundResult.Push:
                playerBalance += currentBet;
                break;
            case RoundResult.DealerWins:
            case RoundResult.DealerBlackjack:
            default:
                break;
        }
    }

    private static bool IsBlackjack(Hand hand) => hand.Cards.Count == 2 && hand.GetTotalValue() == 21;
}