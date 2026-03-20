using Firebase;
using Firebase.Extensions;
using UnityEngine;
using System.Threading.Tasks;

public class FirebaseInitializer : MonoBehaviour
{
    private async void Start()
    {
        // בדוק שה־Firebase מוכן
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase initialized successfully");

            // נסה לטעון את השחקן הקיים
            PlayerData loadedPlayer = await FirestoreManager.Instance.LoadPlayer("player1");

            if (loadedPlayer != null)
            {
                Debug.Log("Player loaded: " + loadedPlayer.Balance);
                // עדכן את ה־UIManager / GameManager עם הנתונים
                if (BlackjackUIManager.Instance != null)
                    BlackjackUIManager.Instance.SetCurrentPlayerData(loadedPlayer);
            }
            else
            {
                // אם השחקן לא קיים, צור ושמור
                PlayerData newPlayer = new PlayerData("player1", "Yair", 1000);
                await FirestoreManager.Instance.SavePlayer(newPlayer);
                if (BlackjackUIManager.Instance != null)
                    BlackjackUIManager.Instance.SetCurrentPlayerData(newPlayer);

                Debug.Log("New player created with balance: " + newPlayer.Balance);
            }
        }
        else
        {
            Debug.LogError("Firebase initialization failed: " + dependencyStatus);
        }
    }
}