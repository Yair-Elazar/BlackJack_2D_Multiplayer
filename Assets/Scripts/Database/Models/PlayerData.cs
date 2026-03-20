using Firebase.Firestore;

[FirestoreData]
public class PlayerData
{
    [FirestoreProperty]
    public string Id { get; set; }

    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public int Balance { get; set; }

    [FirestoreProperty]
    public int CurrentBet { get; set; }  // נוספה שמירת ה-bet

    public PlayerData() { } // נדרש על ידי Firestore

    public PlayerData(string id, string name, int balance, int currentBet = 0)
    {
        Id = id;
        Name = name;
        Balance = balance;
        CurrentBet = currentBet;
    }
}