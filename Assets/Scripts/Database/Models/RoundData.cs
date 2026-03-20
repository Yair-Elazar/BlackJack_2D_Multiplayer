using Firebase.Firestore;


[FirestoreData]
public class RoundData
{
    [FirestoreProperty]
    public string PlayerId { get; set; }

    [FirestoreProperty]
    public string Result { get; set; }

    [FirestoreProperty]
    public int Bet { get; set; }

    [FirestoreProperty]
    public int BalanceAfterRound { get; set; }

    [FirestoreProperty]
    public Timestamp Timestamp { get; set; }

    public RoundData() { }

    public RoundData(string playerId, string result, int bet, int balanceAfterRound)
    {
        PlayerId = playerId;
        Result = result;
        Bet = bet;
        BalanceAfterRound = balanceAfterRound;
        Timestamp = Timestamp.GetCurrentTimestamp();
    }
}