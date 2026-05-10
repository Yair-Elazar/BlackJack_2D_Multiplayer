using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text statusText;

    private FirebaseAuth auth;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    // ===== LOGIN =====
    public async void OnLoginClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        statusText.text = "Logging in...";

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);

            string userId = result.User.UserId;
            Debug.Log("Login success: " + userId);

            await LoadPlayerAndContinue(userId);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            statusText.text = "Login failed";
        }
    }

    // ===== REGISTER =====
    public async void OnRegisterClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        statusText.text = "Registering...";

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);

            string userId = result.User.UserId;
            Debug.Log("Register success: " + userId);

            await CreateNewPlayer(userId);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            statusText.text = "Register failed";
        }
    }

    // ===== CREATE PLAYER =====
    private async Task CreateNewPlayer(string userId)
    {
        statusText.text = "Creating player...";

        PlayerData newPlayer = new PlayerData
        {
            Id = userId,
            Name = "Player",
            Balance = 1000,
            CurrentBet = 0
        };

        if (FirestoreManager.Instance == null)
        {
            Debug.LogError("FirestoreManager is null!");
            statusText.text = "Firebase not ready";
            return;
        }

        await FirestoreManager.Instance.SavePlayer(newPlayer);

        PlayerSession.Set(newPlayer);

        statusText.text = "Welcome! Loading game...";

        await Task.Delay(300); // קטן כדי למנוע race condition

        SceneManager.LoadScene("GameScene");
    }

    // ===== LOAD PLAYER =====
   private async Task LoadPlayerAndContinue(string userId)
{
    statusText.text = "Loading player...";

    if (FirestoreManager.Instance == null)
    {
        Debug.LogError("FirestoreManager is null!");
        statusText.text = "Firebase not ready";
        return;
    }

    var player = await FirestoreManager.Instance.LoadPlayer(userId);

    // אם השחקן לא קיים ב-Firestore
    if (player == null)
    {
        Debug.Log("Player not found. Creating new player...");

        player = new PlayerData
        {
            Id = userId,
            Name = "Player",
            Balance = 1000,
            CurrentBet = 0
        };

        await FirestoreManager.Instance.SavePlayer(player);
    }

    // שמירת השחקן ב-session
    PlayerSession.Set(player);

    statusText.text = "Welcome!";

    await Task.Delay(300);

    SceneManager.LoadScene("GameScene");
}
}