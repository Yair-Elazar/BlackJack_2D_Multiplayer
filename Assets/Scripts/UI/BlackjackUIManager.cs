using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BlackjackUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform playerHandContainer;
    [SerializeField] private Transform playerSecondHandContainer;
    [SerializeField] private Transform dealerHandContainer;
    [SerializeField] private Transform deckPosition;
    [SerializeField] private RectTransform hand0ChipArea;
    [SerializeField] private RectTransform hand1ChipArea;
    [SerializeField] private CanvasGroup hand0CanvasGroup;
    [SerializeField] private CanvasGroup hand1CanvasGroup;
    [SerializeField] private RectTransform hand0Root;
[SerializeField] private RectTransform hand1Root;
    [SerializeField] private GameObject chipPrefab;
    [SerializeField] private Sprite chip10Sprite;
    [SerializeField] private Sprite chip50Sprite;
    [SerializeField] private Sprite chip100Sprite;
    [SerializeField] private Sprite chip500Sprite;

    [SerializeField] private GameObject cardViewPrefab;

    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI hand1Text;
    [SerializeField] private TextMeshProUGUI hand2Text;
    [SerializeField] private TextMeshProUGUI dealerText;
    [SerializeField] private TextMeshProUGUI resultText;

    [SerializeField] private Button hitButton;
    [SerializeField] private Button standButton;
    [SerializeField] private Button splitButton;

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

    [Header("Animation")]
    [SerializeField] private float cardDealDuration = 0.25f;

    private BlackjackGameManager gameManager;
    private PlayerData currentPlayerData;
    private PlayerHandsLayout handsLayout;

    private int activeHandIndex = 0;
    private bool isSplitGame = false;

    private bool isDealing = false;
    private bool roundEnding = false;

    private Coroutine autoNextRoutine;

    public static BlackjackUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SetupUI();
        handsLayout = FindObjectOfType<PlayerHandsLayout>();

        hitButton.onClick.AddListener(OnHit);
        standButton.onClick.AddListener(OnStand);
        splitButton.onClick.AddListener(OnSplit);

        splitButton.gameObject.SetActive(false);

        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        yield return new WaitUntil(() => FirestoreManager.Instance != null);

        currentPlayerData = PlayerSession.CurrentPlayer;

        gameManager = new BlackjackGameManager();
        gameManager.SetBalance(currentPlayerData.Balance);

        UpdateBettingUI();
        StartNewRound();
    }

    // ================= ROUND =================

    public void StartNewRound()
    {
        hand1Text.text = "";
        hand2Text.text = "";
        ClearChips();
        if (isDealing) return;

        roundEnding = false;
        activeHandIndex = 0;
        if (gameManager != null)
        UpdateActiveHandVisual();
        isSplitGame = false;

        gameManager.StartNewRound(currentPlayerData.Id);

        ClearHandUI(playerHandContainer);
        ClearHandUI(playerSecondHandContainer);
        ClearHandUI(dealerHandContainer);

        playerSecondHandContainer.gameObject.SetActive(false);
        FindObjectOfType<PlayerHandsLayout>()
    ?.UpdateLayout(1);

        resultText.text = "";

        hitButton.interactable = false;
        standButton.interactable = false;
        splitButton.gameObject.SetActive(false);

        ShowBettingUI();
    }

    // ================= HIT =================

    private void OnHit()
    {
        if (isDealing || roundEnding) return;
        StartCoroutine(HandleHit());
    }

    private IEnumerator HandleHit()
    {
        isDealing = true;

        bool busted = gameManager.PlayerHit(activeHandIndex);

        HandState hand = gameManager.Player.Hands[activeHandIndex];
        Card lastCard = hand.Cards[hand.Cards.Count - 1];

        Transform target = activeHandIndex == 0
            ? playerHandContainer
            : playerSecondHandContainer;

        yield return AnimateCardDeal(lastCard, target, false);

        UpdateUIText();

        if (busted || hand.GetTotalValue() >= 21)
        {
            yield return new WaitForSeconds(0.4f);
            AdvanceTurn();
        }

        isDealing = false;
    }

    private void AdvanceTurn()
    {
        if (!isSplitGame)
        {
            OnStand();
            return;
        }

        if (activeHandIndex == 0 && gameManager.Player.Hands.Count > 1)
        {
            activeHandIndex = 1;
            UpdateActiveHandVisual();
            UpdateUIText();
            return;
        }

        StartCoroutine(RevealDealer());
    }

    // ================= STAND =================

    private void OnStand()
    {
        if (isDealing || roundEnding) return;

        if (isSplitGame && activeHandIndex == 0)
        {
            activeHandIndex = 1;
            UpdateActiveHandVisual();
            UpdateUIText();
            return;
        }

        StartCoroutine(RevealDealer());
    }

    // ================= SPLIT =================

    private void OnSplit()
{
    if (isDealing || roundEnding)
        return;

    StartCoroutine(HandleSplit());
}

private IEnumerator HandleSplit()
{
    if (!gameManager.CanSplit())
        yield break;

    if (!gameManager.SplitPlayerHand())
        yield break;

    isDealing = true;

    isSplitGame = true;
    activeHandIndex = 0;

    // ===== מציג רק הקלפים המקוריים =====

    RenderSplitHands();

    // ===== משכפל צ'יפים =====

    foreach (Transform chip in hand0ChipArea)
    {
        SpawnChipVisualClone(chip, hand1ChipArea);
    }

    splitButton.gameObject.SetActive(false);

    // ===== מזיז ידיים =====

    yield return null;

    handsLayout?.UpdateLayout(2);

    yield return new WaitForSeconds(0.15f);

    // ===== קלף חדש ליד ראשונה =====

    Card hand0NewCard =
        gameManager.Player.Hands[0].Cards[1];

    yield return AnimateCardDeal(
        hand0NewCard,
        playerHandContainer,
        false
    );

    // ===== קלף חדש ליד שנייה =====

    Card hand1NewCard =
        gameManager.Player.Hands[1].Cards[1];

    yield return AnimateCardDeal(
        hand1NewCard,
        playerSecondHandContainer,
        false
    );

    // ===== Arrange =====

    playerHandContainer
        .GetComponent<HandLayoutController>()
        ?.Arrange();

    playerSecondHandContainer
        .GetComponent<HandLayoutController>()
        ?.Arrange();

    UpdateUIText();

    isDealing = false;
}

    // ================= DEALER =================

    private IEnumerator RevealDealer()
    {
        isDealing = true;

        hitButton.interactable = false;
        standButton.interactable = false;

        if (dealerHandContainer.childCount > 0)
        {
            var view = dealerHandContainer.GetChild(0).GetComponent<CardView>();
            yield return view.FlipCard(gameManager.Dealer.Hand.Cards[0]);
        }

        int before = gameManager.Dealer.Hand.Cards.Count;

        gameManager.PlayerStand();

        for (int i = before; i < gameManager.Dealer.Hand.Cards.Count; i++)
        {
            yield return AnimateCardDeal(gameManager.Dealer.Hand.Cards[i], dealerHandContainer, false);
        }

        UpdateUIText();

        isDealing = false;

        TriggerEndRound();
    }

    // ================= END ROUND (FIX מרכזי) =================

    private void TriggerEndRound()
{
    if (roundEnding) return;

    roundEnding = true;

    UpdateResultText();

    hitButton.interactable = false;
    standButton.interactable = false;

    StartCoroutine(FinishRoundFlow());
}

private IEnumerator FinishRoundFlow()
{
    yield return new WaitForSeconds(1f);

    gameManager.EndRound();

    currentPlayerData.Balance = gameManager.PlayerBalance;

    UpdateBettingUI();

    yield return SavePlayerData();

    yield return new WaitForSeconds(1f);

    StartNewRound();
}

private IEnumerator SavePlayerData()
{
    if (FirestoreManager.Instance == null)
        yield break;

    currentPlayerData.Balance = gameManager.PlayerBalance;

    var task = FirestoreManager.Instance.SavePlayer(currentPlayerData);

    yield return new WaitUntil(() => task.IsCompleted);
}

    

    // ================= BETTING =================

    private void SetupUI()
    {
        chip10Button.onClick.AddListener(() => OnChip(10));
        chip50Button.onClick.AddListener(() => OnChip(50));
        chip100Button.onClick.AddListener(() => OnChip(100));
        chip500Button.onClick.AddListener(() => OnChip(500));

        confirmBetButton.onClick.AddListener(OnConfirmBet);
        clearBetButton.onClick.AddListener(OnClearBet);
    }

    private void OnChip(int amount)
    {
        if (gameManager.AddToBet(amount))
{
    UpdateBettingUI();
    AudioManager.Instance.PlayChipPlace();
    SpawnChip(amount, activeHandIndex);
}
    }

    private void SpawnChip(int amount, int handIndex)
{
    RectTransform target =
        handIndex == 0 ? hand0ChipArea : hand1ChipArea;

    GameObject chipObj = Instantiate(chipPrefab, target);

    ChipView chipView = chipObj.GetComponent<ChipView>();

    Sprite sprite = GetChipSprite(amount);

    chipView.SetChip(amount, sprite);

    RectTransform rect = chipObj.GetComponent<RectTransform>();

    float stackOffsetY = 10f;

int chipIndex = target.childCount - 1;
float randomX = Random.Range(-4f, 4f);
Vector2 endPos =
    new Vector2(randomX, chipIndex * stackOffsetY);

Vector2 startPos =
    endPos + new Vector2(0, 80);

    rect.anchoredPosition = startPos;

    StartCoroutine(AnimateChipDrop(rect, endPos));
}

    private Sprite GetChipSprite(int amount)
{
    return amount switch
    {
        10 => chip10Sprite,
        50 => chip50Sprite,
        100 => chip100Sprite,
        500 => chip500Sprite,
        _ => chip10Sprite
    };
}

    private void OnClearBet()
    {
        gameManager.ClearBet();
        ClearChips();
        UpdateBettingUI();
    }

    private void OnConfirmBet()
    {
        if (!gameManager.ConfirmBet())
            return;

        UpdateBettingUI();
        HideBettingUI();
        StartCoroutine(DealInitialCards());
    }

    // ================= DEAL =================

    private IEnumerator DealInitialCards()
{
    isDealing = true;

    // קלף 1 לשחקן
    var p1 = gameManager.DealCardToPlayer(0);
    yield return AnimateCardDeal(p1, playerHandContainer, false);

    // קלף לדילר
    var d1 = gameManager.DealCardToDealer();
    yield return AnimateCardDeal(d1, dealerHandContainer, true);

    // קלף 2 לשחקן
    var p2 = gameManager.DealCardToPlayer(0);
    yield return AnimateCardDeal(p2, playerHandContainer, false);

    // קלף 2 לדילר
    var d2 = gameManager.DealCardToDealer();
    yield return AnimateCardDeal(d2, dealerHandContainer, false);

    UpdateUIText();
    FindObjectOfType<PlayerHandsLayout>()
    ?.UpdateLayout(gameManager.Player.Hands.Count);

    hitButton.interactable = true;
    standButton.interactable = true;

    splitButton.gameObject.SetActive(gameManager.CanSplit());

    isDealing = false;
}

  

    // ================= UI =================

    private void UpdateUIText()
    {
        hand1Text.text = "";
        hand2Text.text = "";
        hand1Text.gameObject.SetActive(false);
        hand2Text.gameObject.SetActive(false);
        
     if (gameManager.Player.Hands.Count > 1)
    {
        hand1Text.gameObject.SetActive(true);
        hand2Text.gameObject.SetActive(true);

        hand1Text.text = gameManager.Player.Hands[0].GetTotalValue().ToString();
        hand2Text.text = gameManager.Player.Hands[1].GetTotalValue().ToString();
    }
    else
    {
        hand1Text.gameObject.SetActive(true);
        hand2Text.gameObject.SetActive(false);

        hand1Text.text = gameManager.Player.Hands[0].GetTotalValue().ToString();
    }

        if (gameManager.CurrentState == BlackjackGameManager.GameState.PlayerTurn
            || gameManager.CurrentState == BlackjackGameManager.GameState.SplitTurn)
        {
            dealerText.text = $"Dealer: ?";
        }
        else
        {
            dealerText.text =
                $"Dealer: {gameManager.Dealer.Hand.GetTotalValue()}";
        }
        UpdateActiveHandVisual();
    }

    

    private void UpdateBettingUI()
    {
        balanceText.text = $"Balance: ${gameManager.PlayerBalance}";
        currentBetText.text = $"Bet: ${gameManager.CurrentBet}";
    }

    private void UpdateResultText()
    {
        var r = gameManager.GetRoundResult();
        resultText.text = r.HasValue ? r.ToString() : "";
    }

    // ================= SPLIT UI =================

    private void RenderSplitHands()
{
    playerSecondHandContainer.gameObject.SetActive(true);

    ClearHandUI(playerHandContainer);
    ClearHandUI(playerSecondHandContainer);

    // מציגים רק את הקלף הראשון של כל יד
    var firstHandCard = gameManager.Player.Hands[0].Cards[0];
    var secondHandCard = gameManager.Player.Hands[1].Cards[0];

    Instantiate(cardViewPrefab, playerHandContainer)
        .GetComponent<CardView>()
        .SetCard(firstHandCard, false);

    Instantiate(cardViewPrefab, playerSecondHandContainer)
        .GetComponent<CardView>()
        .SetCard(secondHandCard, false);

        playerHandContainer.GetComponent<HandLayoutController>()?.Arrange();
playerSecondHandContainer.GetComponent<HandLayoutController>()?.Arrange();
}

    // ================= HELPERS =================

    private IEnumerator AnimateCardDeal(Card card, Transform target, bool faceDown)
    {
        if (card == null) yield break;

        GameObject go = Instantiate(cardViewPrefab, deckPosition.parent);

        RectTransform rect = go.GetComponent<RectTransform>();
        CardView view = go.GetComponent<CardView>();

        rect.position = deckPosition.position;
        view.SetCard(card, true);
        AudioManager.Instance.PlayCardDeal();

        float t = 0f;

        while (t < cardDealDuration)
        {
            t += Time.deltaTime;
            rect.position = Vector3.Lerp(deckPosition.position, target.position, t / cardDealDuration);
            yield return null;
        }

        rect.SetParent(target, false);

target.GetComponent<HandLayoutController>()
    ?.Arrange();

if (!faceDown)
    yield return view.FlipCard(card);
    }

    private IEnumerator AnimateChipDrop(RectTransform chip, Vector2 target)
{
    Vector2 start = chip.anchoredPosition;

    float t = 0f;

    while (t < 1f)
    {
        t += Time.deltaTime * 6f;

        chip.anchoredPosition =
            Vector2.Lerp(start, target, t);

        yield return null;
    }

    chip.anchoredPosition = target;
}

    private void ClearHandUI(Transform t)
    {
        foreach (Transform c in t)
            Destroy(c.gameObject);
    }

    private void ClearChips()
{
    foreach (Transform chip in hand0ChipArea)
        Destroy(chip.gameObject);

    foreach (Transform chip in hand1ChipArea)
        Destroy(chip.gameObject);
}
private void SpawnChipVisualClone(Transform original, RectTransform target)
{
    GameObject chipObj = Instantiate(chipPrefab, target);

    ChipView originalView = original.GetComponent<ChipView>();
    ChipView newView = chipObj.GetComponent<ChipView>();

    newView.SetChip(originalView.Amount, originalView.Sprite);

    RectTransform rect = chipObj.GetComponent<RectTransform>();

    rect.anchoredPosition = new Vector2(0, 40);

    StartCoroutine(AnimateChipDrop(rect, Vector2.zero));
}

private void UpdateActiveHandVisual()
{
    if (gameManager == null || gameManager.Player == null)
        return;

    if (hand0CanvasGroup == null || hand1CanvasGroup == null)
        return;

    bool splitActive =
        gameManager.Player.Hands != null &&
        gameManager.Player.Hands.Count > 1;

    if (!splitActive)
    {
        hand0CanvasGroup.alpha = 1f;
        hand1CanvasGroup.alpha = 1f;

        hand0Root.localScale = Vector3.one;
        hand1Root.localScale = Vector3.one;
        return;
    }

    if (activeHandIndex == 0)
    {
        hand0CanvasGroup.alpha = 1f;
        hand1CanvasGroup.alpha = 0.5f;

        hand0Root.localScale = Vector3.one * 1.08f;
        hand1Root.localScale = Vector3.one;
    }
    else
    {
        hand1CanvasGroup.alpha = 1f;
        hand0CanvasGroup.alpha = 0.5f;

        hand1Root.localScale = Vector3.one * 1.08f;
        hand0Root.localScale = Vector3.one;
    }
}

    private void ShowBettingUI() => bettingPanel.SetActive(true);
    private void HideBettingUI() => bettingPanel.SetActive(false);
}