using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages Blackjack game UI and connects UI elements to game logic.
/// Fully compatible with the new BlackjackGameManager using GameState and RoundResult.
/// </summary>
public class BlackjackUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform playerHandContainer;
    [SerializeField] private Transform dealerHandContainer;
    [SerializeField] private GameObject cardViewPrefab;
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI dealerText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button hitButton;
    [SerializeField] private Button standButton;
    [SerializeField] private Button newRoundButton;

    private BlackjackGameManager gameManager;
    private const string defaultPlayerName = "Player";

    private void Start()
    {
        gameManager = new BlackjackGameManager();
        StartNewRound();

        // Add button listeners
        hitButton.onClick.AddListener(OnHit);
        standButton.onClick.AddListener(OnStand);
        newRoundButton.onClick.AddListener(StartNewRound);
    }

    private void OnHit()
    {
        bool busted = gameManager.PlayerHit();
        UpdateUI();

        // אם השחקן בסט, סיים את הסיבוב והראה תוצאה
        if (busted || gameManager.CurrentState == BlackjackGameManager.GameState.Finished)
        {
            UpdateResultText();
            hitButton.interactable = false;
            standButton.interactable = false;
        }
    }

    private void OnStand()
    {
        gameManager.PlayerStand();
        UpdateUI();
        UpdateResultText();
        hitButton.interactable = false;
        standButton.interactable = false;
    }

    private void UpdateUI()
    {
        RenderHand(gameManager.Player.Hand, playerHandContainer);
        RenderHand(gameManager.Dealer.Hand, dealerHandContainer);
        playerText.text = $"Player: {gameManager.Player.Hand.ToString()}";
        dealerText.text = $"Dealer: {gameManager.Dealer.Hand.ToString()}";
    }

    private void UpdateResultText()
    {
        var result = gameManager.GetRoundResult();
        if (!result.HasValue)
        {
            resultText.text = string.Empty;
            return;
        }

        resultText.text = result.Value switch
        {
            BlackjackGameManager.RoundResult.PlayerWins => "Player Wins!",
            BlackjackGameManager.RoundResult.DealerWins => "Dealer Wins!",
            BlackjackGameManager.RoundResult.Push => "Push!",
            BlackjackGameManager.RoundResult.PlayerBlackjack => "Blackjack! Player Wins!",
            BlackjackGameManager.RoundResult.DealerBlackjack => "Blackjack! Dealer Wins!",
            _ => string.Empty
        };
    }

    public void StartNewRound()
    {
    gameManager.StartNewRound(defaultPlayerName);

    ClearHandUI(playerHandContainer);
    ClearHandUI(dealerHandContainer);

    hitButton.interactable = true;
    standButton.interactable = true;
    resultText.text = string.Empty;

    UpdateUI();
    }


    private void ClearHandUI(Transform container)
    {
    foreach (Transform child in container)
    {
        Destroy(child.gameObject);
    }
    }

    private void RenderHand(Hand hand, Transform container)
    {
    ClearHandUI(container);

    foreach (var card in hand.Cards)
    {
        GameObject cardGO = Instantiate(cardViewPrefab, container);
        CardView view = cardGO.GetComponent<CardView>();
        view.SetCard(card);
    }
    }


}
