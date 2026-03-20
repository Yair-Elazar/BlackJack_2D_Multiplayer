using Firebase.Firestore;
using UnityEngine;
using System.Threading.Tasks;
using System;
public class FirestoreManager : MonoBehaviour
{
    public static FirestoreManager Instance;
    private FirebaseFirestore db;
    public FirebaseFirestore Db => db;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            db = FirebaseFirestore.DefaultInstance;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // שמירת PlayerData
    public async Task SavePlayer(PlayerData player)
{
    try
    {
        await db.Collection("players").Document(player.Id).SetAsync(player);
        Debug.Log("Player saved to Firestore");
        Debug.Log("Saving player with ID: " + player.Id + " balance: " + player.Balance);
        Debug.Log("🚨 SAVE CALLED FROM: " + Environment.StackTrace);
    }
    catch (System.Exception e)
    {
        Debug.LogError("Failed to save player: " + e);
    }
}

    // שמירת RoundData
    public Task SaveRound(RoundData round)
    {
        return db.Collection("rounds").AddAsync(round)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                    Debug.Log("Round saved to Firestore");
                else
                    Debug.LogError("Failed to save round: " + task.Exception);
            });
    }

    // טעינת PlayerData
    public async Task<PlayerData> LoadPlayer(string playerId)
    {
        var docRef = db.Collection("players").Document(playerId);
        var snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            Debug.Log("hayyy");
            Debug.Log("Loading player with ID: " + playerId);
            return snapshot.ConvertTo<PlayerData>();
            
        }

        return null;
    }
}