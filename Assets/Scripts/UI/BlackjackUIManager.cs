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
    [SerializeField] private Button chip500Button;
    [SerializeField] private Button confirmBetButton;
    [SerializeField] private Button clearBetButton;

    [Header("Animation Settings")]
    [SerializeField] private float cardDealDuration = 0.1f;
    [SerializeField] private float delayBetweenCards = 0.1f;

    private BlackjackGameManager gameManager;
    private PlayerData currentPlayerData;
    private bool isDealing = false;

    // Singleton
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

    // =========================
    // INIT
    // =========================
    private IEnumerator InitializeGame()
    {
        yield return new WaitUntil(() => FirestoreManager.Instance != null);

        if (!PlayerSession.IsLoggedIn)
        {
            Debug.LogError("❌ No logged-in player found in PlayerSession");
            yield break;
        }

        currentPlayerData = PlayerSession.CurrentPlayer;

        gameManager = new BlackjackGameManager();
        gameManager.SetBalance(currentPlayerData.Balance);
        UpdateBettingUI();

        Debug.Log("✅ Game initialized for: " + currentPlayerData.Name);

        StartNewRound();
    }

    // =========================
    // GAME FLOW
    // =========================
    public void StartNewRound()
    {
        if (isDealing) return;

        gameManager.StartNewRound(currentPlayerData.Id);

        ClearHandUI(playerHandContainer);
        ClearHandUI(dealerHandContainer);

        hitButton.interactable = false;
        standButton.interactable = false;

        resultText.text = "";

        ShowBettingUI();
    }

    private void OnHit()
{
    if (isDealing) return;

    StartCoroutine(HandleHit());
}

private IEnumerator HandleHit()
{
    isDealing = true;

    bool busted = gameManager.PlayerHit();

    Card card = gameManager.Player.Hand.Cards[^1];
    yield return AnimateCardDeal(card, playerHandContainer, false);

    UpdateUIText();

    isDealing = false;

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

    // =========================
    // BETTING
    // =========================
    private void SetupBettingUI()
    {
        chip10Button.onClick.AddListener(() => OnChipClicked(10));
        chip50Button.onClick.AddListener(() => OnChipClicked(50));
        chip100Button.onClick.AddListener(() => OnChipClicked(100));
        chip500Button.onClick.AddListener(() => OnChipClicked(500));

        confirmBetButton.onClick.AddListener(OnConfirmBet);
        clearBetButton.onClick.AddListener(OnClearBet);
    }

   private void OnChipClicked(int value)
{
    Debug.Log("Chip clicked: " + value);

    bool success = gameManager.AddToBet(value);

    Debug.Log("Current state: " + gameManager.CurrentState);
    Debug.Log("AddToBet success: " + success);
    Debug.Log("Current bet: " + gameManager.CurrentBet);

    if (success)
    {
        UpdateBettingUI();
    }
}

    private void OnClearBet()
    {
        gameManager.ClearBet();
        UpdateBettingUI();
    }

    private void OnConfirmBet()
{
    Debug.Log("CONFIRM BET CLICKED");

    if (gameManager == null)
    {
        Debug.LogError("❌ gameManager NULL");
        return;
    }

    Debug.Log("Current bet before confirm: " + gameManager.CurrentBet);
    Debug.Log("Balance before confirm: " + gameManager.PlayerBalance);

    bool success = gameManager.ConfirmBet();

    Debug.Log("ConfirmBet result: " + success);

    if (!success)
    {
        Debug.LogError("❌ ConfirmBet failed");
        return;
    }

    Debug.Log("Balance after confirm: " + gameManager.PlayerBalance);

    currentPlayerData.CurrentBet = gameManager.CurrentBet;

    FirestoreManager.Instance?.SavePlayer(currentPlayerData);

    Debug.Log("Hiding betting UI");

    HideBettingUI();

    Debug.Log("Starting DealInitialCards coroutine");

    StartCoroutine(DealInitialCards());
}

    // =========================
    // ROUND END
    // =========================
    private void EndRound()
    {
        UpdateResultText();

        hitButton.interactable = false;
        standButton.interactable = false;

        currentPlayerData.Balance = gameManager.PlayerBalance;
        currentPlayerData.CurrentBet = 0;

        StartCoroutine(SavePlayerCoroutine());

        FirestoreManager.Instance.SaveRound(
            new RoundData(
                currentPlayerData.Id,
                gameManager.GetRoundResult().ToString(),
                gameManager.CurrentBet,
                gameManager.PlayerBalance
            )
        );

        StartCoroutine(ReturnToBettingAfterDelay());
    }

    private IEnumerator SavePlayerCoroutine()
    {
        var task = FirestoreManager.Instance.SavePlayer(currentPlayerData);
        yield return new WaitUntil(() => task.IsCompleted);
    }

    private IEnumerator ReturnToBettingAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        gameManager.PrepareNextRound();

        ClearHandUI(playerHandContainer);
        ClearHandUI(dealerHandContainer);

        resultText.text = "";

        ShowBettingUI();
    }

    // =========================
    // DEALING
    // =========================
    private IEnumerator DealInitialCards()
    {
        isDealing = true;

        yield return DealCardToPlayer();
        yield return new WaitForSeconds(delayBetweenCards);

        yield return DealCardToDealer(true);
        yield return new WaitForSeconds(delayBetweenCards);

        yield return DealCardToPlayer();
        yield return new WaitForSeconds(delayBetweenCards);

        yield return DealCardToDealer(false);

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

    private IEnumerator DealCardToPlayer()
    {
        Card card = gameManager.DealCardToPlayer();
        yield return AnimateCardDeal(card, playerHandContainer, false);
    }

    private IEnumerator DealCardToDealer(bool faceDown)
    {
        Card card = gameManager.DealCardToDealer();
        yield return AnimateCardDeal(card, dealerHandContainer, faceDown);
    }

    // =========================
    // UI HELPERS
    // =========================
    private void ShowBettingUI()
{
    bettingPanel.SetActive(true);
    UpdateBettingUI();
}
    private void HideBettingUI() => bettingPanel.SetActive(false);

    private void ClearHandUI(Transform container)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    private void UpdateBettingUI()
{
    if (gameManager == null)
        return;

    if (balanceText != null)
        balanceText.text = $"Balance: ${gameManager.PlayerBalance}";

    if (currentBetText != null)
        currentBetText.text = $"Bet: ${gameManager.CurrentBet}";
}

    private void UpdateUIText()
    {
        playerText.text = $"Player: {gameManager.Player.Hand}";
        dealerText.text = $"Dealer: {gameManager.Dealer.Hand}";
    }

    private void UpdateResultText()
    {
        var result = gameManager.GetRoundResult();

        resultText.text = result.HasValue
            ? result.Value.ToString()
            : "";
    }

    // =========================
    // ANIMATION
    // =========================
    private IEnumerator AnimateCardDeal(Card card, Transform target, bool faceDown)
{
    if (card == null) yield break;

    GameObject go = Instantiate(cardViewPrefab, deckPosition.parent);
    RectTransform rect = go.GetComponent<RectTransform>();
    CardView view = go.GetComponent<CardView>();

    RectTransform deckRect = deckPosition.GetComponent<RectTransform>();
    RectTransform targetRect = target.GetComponent<RectTransform>();

    Vector3 startPos = deckRect.position;
    Vector3 endPos = targetRect.position;

    rect.position = startPos;
    rect.localScale = Vector3.one;

    // 🔥 IMPORTANT: set card IMMEDIATELY (but face down)
    view.SetCard(card, true);

    float t = 0f;

    while (t < cardDealDuration)
    {
        t += Time.deltaTime;
        float p = t / cardDealDuration;

        rect.position = Vector3.Lerp(startPos, endPos, p);

        yield return null;
    }

    rect.position = endPos;

    // flip reveal only at end
    if (!faceDown)
        yield return StartCoroutine(view.FlipCard(card));
    else
        view.SetCard(card, true);

    rect.SetParent(target, false);
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
private IEnumerator FlipCard(CardView view, bool faceDown)
{
    RectTransform rect = view.GetComponent<RectTransform>();

    float t = 0f;
    float duration = 0.15f;

    while (t < duration)
    {
        t += Time.deltaTime;
        float p = t / duration;

        rect.localScale = new Vector3(1 - p, 1, 1);

        yield return null;
    }

    //view.SetFaceDown(faceDown);

    t = 0f;

    while (t < duration)
    {
        t += Time.deltaTime;
        float p = t / duration;

        rect.localScale = new Vector3(p, 1, 1);

        yield return null;
    }

    rect.localScale = Vector3.one;
}
}