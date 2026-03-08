using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages Blackjack game UI and connects UI elements to game logic.
/// Fully compatible with the new BlackjackGameManager using GameState and RoundResult.
/// </summary>
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
    [SerializeField] private Button newRoundButton;

    [Header("Animation Settings")]
    [SerializeField] private float cardDealDuration = 0.5f;
    [SerializeField] private float delayBetweenCards = 0.3f;

    private BlackjackGameManager gameManager;
    private const string defaultPlayerName = "Player";
    private bool isDealing = false;

    private void Start()
    {
        gameManager = new BlackjackGameManager();
        
        // Set deck visual to card back sprite
        if (deckPosition != null)
        {
            Image deckImage = deckPosition.GetComponent<Image>();
            if (deckImage != null)
            {
                deckImage.sprite = CardSpriteResolver.GetCardBack();
            }
        }
        
        StartNewRound();

        // Add button listeners
        hitButton.onClick.AddListener(OnHit);
        standButton.onClick.AddListener(OnStand);
        newRoundButton.onClick.AddListener(StartNewRound);
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
        if (isDealing)
            return; // Prevent starting new round while dealing

        gameManager.StartNewRound(defaultPlayerName);

        ClearHandUI(playerHandContainer);
        ClearHandUI(dealerHandContainer);

        hitButton.interactable = false;
        standButton.interactable = false;
        resultText.text = string.Empty;

        // Start animated dealing
        StartCoroutine(DealInitialCards());
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

    private IEnumerator DealInitialCards()
    {
        isDealing = true;

        // Deal 4 cards in order: Player, Dealer, Player, Dealer
        // Dealer's first card should be face down
        
        // Card 1: Player
        Card card1 = gameManager.DealCardToPlayer();
        yield return StartCoroutine(AnimateCardDeal(card1, playerHandContainer, false));
        yield return new WaitForSeconds(delayBetweenCards);

        // Card 2: Dealer (face down)
        Card card2 = gameManager.DealCardToDealer();
        yield return StartCoroutine(AnimateCardDeal(card2, dealerHandContainer, true));
        yield return new WaitForSeconds(delayBetweenCards);

        // Card 3: Player
        Card card3 = gameManager.DealCardToPlayer();
        yield return StartCoroutine(AnimateCardDeal(card3, playerHandContainer, false));
        yield return new WaitForSeconds(delayBetweenCards);

        // Card 4: Dealer (face up)
        Card card4 = gameManager.DealCardToDealer();
        yield return StartCoroutine(AnimateCardDeal(card4, dealerHandContainer, false));

        // Check for immediate Blackjack
        gameManager.CheckInitialBlackjack();

        // Update UI text
        UpdateUIText();

        // Enable buttons if game is not finished
        if (gameManager.CurrentState != BlackjackGameManager.GameState.Finished)
        {
            hitButton.interactable = true;
            standButton.interactable = true;
        }
        else
        {
            UpdateResultText();
        }

        isDealing = false;
    }

    private IEnumerator AnimateCardDeal(Card card, Transform targetContainer, bool faceDown)
    {
        if (card == null || deckPosition == null)
            yield break;

        // Create card at deck position (as child of Canvas root for proper animation)
        Canvas canvas = deckPosition.GetComponentInParent<Canvas>();
        Transform canvasRoot = canvas != null ? canvas.transform : deckPosition.root;
        
        GameObject cardGO = Instantiate(cardViewPrefab, canvasRoot);
        CardView view = cardGO.GetComponent<CardView>();
        
        // Set card face down initially if needed
        if (faceDown)
        {
            view.SetCard(card, true);
        }
        else
        {
            view.SetCard(card, false);
        }

        RectTransform cardRect = cardGO.GetComponent<RectTransform>();
        RectTransform deckRect = deckPosition.GetComponent<RectTransform>();
        RectTransform targetRect = targetContainer.GetComponent<RectTransform>();

        // Get world positions
        Vector2 startWorldPos = deckRect.position;
        Vector2 endWorldPos = targetRect.position;

        // Animate from deck to target
        float elapsed = 0f;
        while (elapsed < cardDealDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cardDealDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth easing

            // Interpolate world position
            Vector2 currentPos = Vector2.Lerp(startWorldPos, endWorldPos, t);
            cardRect.position = currentPos;

            yield return null;
        }

        // Move to final position in container
        cardGO.transform.SetParent(targetContainer, false);
        cardRect.anchoredPosition = Vector2.zero;

        // If face down, we'll keep it face down until dealer reveals
        // Otherwise, ensure it's face up
        if (!faceDown)
        {
            view.SetCard(card, false);
        }
    }

    private void UpdateUIText()
    {
        playerText.text = $"Player: {gameManager.Player.Hand.ToString()}";
        
        // For dealer, show face down card as "?" if first card is hidden
        if (gameManager.Dealer.Hand.Cards.Count > 0 && 
            gameManager.CurrentState == BlackjackGameManager.GameState.PlayerTurn)
        {
            // Show only the visible card value
            if (gameManager.Dealer.Hand.Cards.Count == 1)
            {
                dealerText.text = "Dealer: ?";
            }
            else
            {
                // Show first card as hidden, second card visible
                var visibleCard = gameManager.Dealer.Hand.Cards[1];
                dealerText.text = $"Dealer: ?, {visibleCard} (Total: ?)";
            }
        }
        else
        {
            dealerText.text = $"Dealer: {gameManager.Dealer.Hand.ToString()}";
        }
    }

    private void OnHit()
    {
        if (isDealing)
            return;

        bool busted = gameManager.PlayerHit();
        
        // Animate the new card being dealt
        StartCoroutine(DealHitCard());

        // אם השחקן בסט, סיים את הסיבוב והראה תוצאה
        if (busted || gameManager.CurrentState == BlackjackGameManager.GameState.Finished)
        {
            UpdateResultText();
            hitButton.interactable = false;
            standButton.interactable = false;
        }
    }

    private IEnumerator DealHitCard()
    {
        isDealing = true;
        Card card = gameManager.Player.Hand.Cards[gameManager.Player.Hand.Cards.Count - 1];
        yield return StartCoroutine(AnimateCardDeal(card, playerHandContainer, false));
        UpdateUIText();
        isDealing = false;
    }

    private void OnStand()
    {
        if (isDealing)
            return;

        // Reveal dealer's face down card and deal remaining cards
        StartCoroutine(RevealDealerAndPlay());
    }

    private IEnumerator RevealDealerAndPlay()
    {
        isDealing = true;
        hitButton.interactable = false;
        standButton.interactable = false;

        // Reveal dealer's face down card
        if (dealerHandContainer.childCount > 0)
        {
            Transform firstCard = dealerHandContainer.GetChild(0);
            CardView view = firstCard.GetComponent<CardView>();
            if (view != null && gameManager.Dealer.Hand.Cards.Count > 0)
            {
                view.SetCard(gameManager.Dealer.Hand.Cards[0], false);
            }
        }

        UpdateUIText();
        yield return new WaitForSeconds(0.5f);

        // Get initial card count before dealer plays
        int initialDealerCardCount = gameManager.Dealer.Hand.Cards.Count;
        
        // Make dealer play (this will add cards to dealer's hand)
        gameManager.PlayerStand();
        
        // Animate any new cards dealer received
        int currentCardCount = gameManager.Dealer.Hand.Cards.Count;
        for (int i = initialDealerCardCount; i < currentCardCount; i++)
        {
            Card card = gameManager.Dealer.Hand.Cards[i];
            yield return StartCoroutine(AnimateCardDeal(card, dealerHandContainer, false));
            yield return new WaitForSeconds(delayBetweenCards);
        }

        UpdateUIText();
        UpdateResultText();
        isDealing = false;
    }
}
