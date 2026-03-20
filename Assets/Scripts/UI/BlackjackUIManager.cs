using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;

public class BlackjackUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform playerHandContainer;
    [SerializeField] private Transform dealerHandContainer;
    [SerializeField] private Transform deckPosition;
    [SerializeField] private GameObject cardViewPrefab;
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI dealerText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button hitButton;
    [SerializeField] private Button standButton;

    [Header("Betting UI")]
    [SerializeField] private GameObject bettingPanel;
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI currentBetText;
    [SerializeField] private Button chip10Button;
    [SerializeField] private Button chip50Button;
    [SerializeField] private Button chip100Button;
    [SerializeField] private Button chip200Button;
    [SerializeField] private Button chip500Button;
    [SerializeField] private Button confirmBetButton;
    [SerializeField] private Button clearBetButton;

    [Header("Animation Settings")]
    [SerializeField] private float cardDealDuration = 0.1f;
    [SerializeField] private float delayBetweenCards = 0.1f;

    private BlackjackGameManager gameManager;
    private string playerId = "player1"; 
    private PlayerData currentPlayerData = null;
    private bool isDealing = false;

    // Singleton pattern
    public static BlackjackUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (deckPosition != null)
        {
            Image deckImage = deckPosition.GetComponent<Image>();
            if (deckImage != null)
                deckImage.sprite = CardSpriteResolver.GetCardBack();
        }

        SetupBettingUI();

        hitButton.onClick.AddListener(OnHit);
        standButton.onClick.AddListener(OnStand);

        StartCoroutine(InitializeGame());
    }

    private void SetupBettingUI()
    {
        if (chip10Button != null) chip10Button.onClick.AddListener(() => OnChipClicked(10));
        if (chip50Button != null) chip50Button.onClick.AddListener(() => OnChipClicked(50));
        if (chip100Button != null) chip100Button.onClick.AddListener(() => OnChipClicked(100));
        if (chip200Button != null) chip200Button.onClick.AddListener(() => OnChipClicked(200));
        if (chip500Button != null) chip500Button.onClick.AddListener(() => OnChipClicked(500));

        if (confirmBetButton != null) confirmBetButton.onClick.AddListener(OnConfirmBet);
        if (clearBetButton != null) clearBetButton.onClick.AddListener(OnClearBet);
    }

    private void UpdateResultText()
    {
        var result = gameManager.GetRoundResult();
        if (!result.HasValue)
        {
            resultText.text = "";
            return;
        }

        string resultMessage = result.Value switch
        {
            BlackjackGameManager.RoundResult.PlayerWins => "Player Wins!",
            BlackjackGameManager.RoundResult.DealerWins => "Dealer Wins!",
            BlackjackGameManager.RoundResult.Push => "Push!",
            BlackjackGameManager.RoundResult.PlayerBlackjack => "Blackjack! Player Wins!",
            BlackjackGameManager.RoundResult.DealerBlackjack => "Blackjack! Dealer Wins!",
            _ => ""
        };

        resultText.text = $"{resultMessage}\nBalance: ${gameManager.PlayerBalance}";
    }

    public void StartNewRound()
    {
        if (isDealing) return;

        gameManager.StartNewRound(playerId);

        ClearHandUI(playerHandContainer);
        ClearHandUI(dealerHandContainer);

        hitButton.interactable = false;
        standButton.interactable = false;

        resultText.text = "";

        ShowBettingUI();
    }

    private void ShowBettingUI()
    {
        if (bettingPanel != null) bettingPanel.SetActive(true);
        UpdateBettingUI();
    }

    private void HideBettingUI()
    {
        if (bettingPanel != null) bettingPanel.SetActive(false);
    }

    private void UpdateBettingUI()
    {
        if (balanceText != null)
            balanceText.text = $"Balance: ${gameManager.PlayerBalance}";

        if (currentBetText != null)
            currentBetText.text = $"Bet: ${gameManager.CurrentBet}";

        int balance = gameManager.PlayerBalance;
        int currentBet = gameManager.CurrentBet;
        int availableBalance = balance - currentBet;

        if (chip10Button != null) chip10Button.interactable = availableBalance >= 10;
        if (chip50Button != null) chip50Button.interactable = availableBalance >= 50;
        if (chip100Button != null) chip100Button.interactable = availableBalance >= 100;
        if (chip200Button != null) chip200Button.interactable = availableBalance >= 200;
        if (chip500Button != null) chip500Button.interactable = availableBalance >= 500;

        if (confirmBetButton != null) confirmBetButton.interactable = currentBet > 0;
    }

    private void OnChipClicked(int chipValue)
    {
        if (gameManager.AddToBet(chipValue))
        {
            UpdateBettingUI();

            if (currentPlayerData != null)
            {
                currentPlayerData.CurrentBet = gameManager.CurrentBet;
                FirestoreManager.Instance?.SavePlayer(currentPlayerData);
            }
        }
    }

    private void OnClearBet()
    {
        gameManager.ClearBet();
        UpdateBettingUI();
    }

    private void OnConfirmBet()
    {
        if (gameManager.ConfirmBet())
        {
            if (currentPlayerData != null)
            {
                currentPlayerData.CurrentBet = gameManager.CurrentBet;
                FirestoreManager.Instance?.SavePlayer(currentPlayerData);
            }

            HideBettingUI();
            StartCoroutine(DealInitialCards());
        }
    }

    private void ClearHandUI(Transform container)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    private IEnumerator DealInitialCards()
    {
        isDealing = true;

        Card card1 = gameManager.DealCardToPlayer();
        yield return StartCoroutine(AnimateCardDeal(card1, playerHandContainer, false));
        yield return new WaitForSeconds(delayBetweenCards);

        Card card2 = gameManager.DealCardToDealer();
        yield return StartCoroutine(AnimateCardDeal(card2, dealerHandContainer, true));
        yield return new WaitForSeconds(delayBetweenCards);

        Card card3 = gameManager.DealCardToPlayer();
        yield return StartCoroutine(AnimateCardDeal(card3, playerHandContainer, false));
        yield return new WaitForSeconds(delayBetweenCards);

        Card card4 = gameManager.DealCardToDealer();
        yield return StartCoroutine(AnimateCardDeal(card4, dealerHandContainer, false));

        gameManager.CheckInitialBlackjack();
        UpdateUIText();

        if (gameManager.CurrentState != BlackjackGameManager.GameState.Finished)
        {
            hitButton.interactable = true;
            standButton.interactable = true;
        }
        else
        {
            EndRound();
        }

        isDealing = false;
    }

    private IEnumerator AnimateCardDeal(Card card, Transform targetContainer, bool faceDown)
    {
        if (card == null || deckPosition == null) yield break;

        Canvas canvas = deckPosition.GetComponentInParent<Canvas>();
        Transform canvasRoot = canvas != null ? canvas.transform : deckPosition.root;

        GameObject cardGO = Instantiate(cardViewPrefab, canvasRoot);
        CardView view = cardGO.GetComponent<CardView>();
        view.SetCard(card, true);

        RectTransform cardRect = cardGO.GetComponent<RectTransform>();
        RectTransform deckRect = deckPosition.GetComponent<RectTransform>();
        RectTransform targetRect = targetContainer.GetComponent<RectTransform>();

        Vector2 startWorldPos = deckRect.position;
        Vector2 endWorldPos = targetRect.position;

        float elapsed = 0f;
        while (elapsed < cardDealDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / cardDealDuration);
            cardRect.position = Vector2.Lerp(startWorldPos, endWorldPos, t);
            yield return null;
        }

        cardGO.transform.SetParent(targetContainer, false);
        cardRect.anchoredPosition = Vector2.zero;

        if (!faceDown)
            yield return StartCoroutine(view.FlipCard(card));
    }

    private void UpdateUIText()
    {
        playerText.text = $"Player: {gameManager.Player.Hand}";

        if (gameManager.Dealer.Hand.Cards.Count > 0 &&
            gameManager.CurrentState == BlackjackGameManager.GameState.PlayerTurn)
        {
            if (gameManager.Dealer.Hand.Cards.Count == 1)
                dealerText.text = "Dealer: ?";
            else
            {
                var visibleCard = gameManager.Dealer.Hand.Cards[1];
                dealerText.text = $"Dealer: ?, {visibleCard} (Total: ?)";
            }
        }
        else
            dealerText.text = $"Dealer: {gameManager.Dealer.Hand}";

        if (bettingPanel != null && bettingPanel.activeSelf)
            UpdateBettingUI();
    }

    private void OnHit()
    {
        if (isDealing) return;

        bool busted = gameManager.PlayerHit();
        StartCoroutine(DealHitCard());

        if (busted || gameManager.CurrentState == BlackjackGameManager.GameState.Finished)
            EndRound();
    }

    private IEnumerator DealHitCard()
    {
        isDealing = true;

        Card card = gameManager.Player.Hand.Cards[^1];
        yield return StartCoroutine(AnimateCardDeal(card, playerHandContainer, false));

        UpdateUIText();
        isDealing = false;
    }

    private void OnStand()
    {
        if (isDealing) return;

        StartCoroutine(RevealDealerAndPlay());
    }

    private IEnumerator RevealDealerAndPlay()
    {
        isDealing = true;

        hitButton.interactable = false;
        standButton.interactable = false;

        if (dealerHandContainer.childCount > 0)
        {
            Transform firstCard = dealerHandContainer.GetChild(0);
            CardView view = firstCard.GetComponent<CardView>();

            if (view != null && gameManager.Dealer.Hand.Cards.Count > 0)
                view.SetCard(gameManager.Dealer.Hand.Cards[0], false);
        }

        UpdateUIText();
        yield return new WaitForSeconds(0.5f);

        int initialDealerCardCount = gameManager.Dealer.Hand.Cards.Count;
        gameManager.PlayerStand();
        int currentCardCount = gameManager.Dealer.Hand.Cards.Count;

        for (int i = initialDealerCardCount; i < currentCardCount; i++)
        {
            Card card = gameManager.Dealer.Hand.Cards[i];
            yield return StartCoroutine(AnimateCardDeal(card, dealerHandContainer, false));
            yield return new WaitForSeconds(delayBetweenCards);
        }

        UpdateUIText();
        isDealing = false;

        EndRound();
    }

    private void EndRound()
    {
        UpdateResultText();

        hitButton.interactable = false;
        standButton.interactable = false;

        SaveRoundToFirestore();

        StartCoroutine(ReturnToBettingAfterDelay());
    }

    private IEnumerator ReturnToBettingAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        if (gameManager.CurrentState != BlackjackGameManager.GameState.Finished)
            yield break;

        gameManager.PrepareNextRound();

        ClearHandUI(playerHandContainer);
        ClearHandUI(dealerHandContainer);

        resultText.text = "";

        ShowBettingUI();
    }

    private IEnumerator InitializeGame()
    {
        yield return new WaitUntil(() => FirestoreManager.Instance != null);

        Debug.Log("Loading player with ID: " + playerId);
        var task = FirestoreManager.Instance.LoadPlayer(playerId);

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("Firestore load failed: " + task.Exception);
            yield break;
        }

        if (task.Result != null)
        {
            currentPlayerData = task.Result;
            gameManager = new BlackjackGameManager();
            gameManager.SetBalance(currentPlayerData.Balance);
            Debug.Log("✅ Loaded balance: " + currentPlayerData.Balance);
        }
        else
        {
            Debug.LogError("Player not found in DB!");
            yield break;
        }

        StartNewRound();
    }

    private void SaveRoundToFirestore()
    {
        if (FirestoreManager.Instance == null || currentPlayerData == null) return;

        var result = gameManager.GetRoundResult();
        string resultString = result.HasValue ? result.Value.ToString() : "Unknown";

        int balance = gameManager.PlayerBalance;
        int bet = gameManager.CurrentBet;

        currentPlayerData.Balance = balance;
        currentPlayerData.CurrentBet = 0;

        StartCoroutine(SavePlayerCoroutine());

        RoundData round = new RoundData(playerId, resultString, bet, balance);
        FirestoreManager.Instance.SaveRound(round);
    }

    private IEnumerator SavePlayerCoroutine()
    {
        var task = FirestoreManager.Instance.SavePlayer(currentPlayerData);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
            Debug.LogError("Save failed: " + task.Exception);
        else
            Debug.Log("✅ Save completed to Firestore!");
    }
    // בתוך BlackjackUIManager
public void SetCurrentPlayerData(PlayerData playerData)
{
    currentPlayerData = playerData;

    // עדכון ה־GameManager עם ה־balance של השחקן
    if (gameManager == null) gameManager = new BlackjackGameManager();
    gameManager.SetBalance(playerData.Balance);

    // עדכון UI במידת הצורך
    UpdateBettingUI();
}
}