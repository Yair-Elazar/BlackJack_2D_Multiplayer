using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages Blackjack game UI and connects UI elements to game logic.
/// Uses the pure C# BlackjackGameManager as a backend engine.
/// </summary>
public class BlackjackUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI dealerText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button hitButton;
    [SerializeField] private Button standButton;

    private BlackjackGameManager gameManager;
    private const string defaultPlayerName = "Player";

    private void Start()
    {
        gameManager = new BlackjackGameManager();
        gameManager.StartNewRound(defaultPlayerName);
        UpdateUI();

        hitButton.onClick.AddListener(OnHit);
        standButton.onClick.AddListener(OnStand);
    }

    /// <summary>
    /// Handler for Hit button: draws card, updates UI, checks for bust.
    /// </summary>
    public void OnHit()
    {
        if (gameManager.PlayerHit())
        {
            resultText.text = "Busted! Dealer Wins!";
            hitButton.interactable = false;
            standButton.interactable = false;
        }
        UpdateUI();
    }

    /// <summary>
    /// Handler for Stand button: ends player turn, lets dealer play, checks outcome.
    /// </summary>
    public void OnStand()
    {
        gameManager.PlayerStand();
        string outcome = gameManager.CheckOutcome();
        resultText.text = outcome;
        hitButton.interactable = false;
        standButton.interactable = false;
        UpdateUI();
    }

    /// <summary>
    /// Updates the UI elements to reflect the current game state.
    /// </summary>
    private void UpdateUI()
    {
        var playerHand = gameManager.Player.Hand;
        var dealerHand = gameManager.Dealer.Hand;

        playerText.text = $"Player: {playerHand.ToString()}";
        dealerText.text = $"Dealer: {dealerHand.ToString()}";
    }

    /// <summary>
    /// Optional method to start a new round (can be connected to a UI button).
    /// </summary>
    public void StartNewRound()
    {
        gameManager.StartNewRound(defaultPlayerName);
        hitButton.interactable = true;
        standButton.interactable = true;
        resultText.text = string.Empty;
        UpdateUI();
    }
}
