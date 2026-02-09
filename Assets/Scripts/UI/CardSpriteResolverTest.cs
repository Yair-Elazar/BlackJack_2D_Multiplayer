using UnityEngine;

public class CardSpriteResolverTest : MonoBehaviour
{
    private void Start()
    {
        var card = new Card(Card.RankType.Ace, Card.SuitType.Spades);
        var sprite = CardSpriteResolver.GetCardSprite(card);

        Debug.Log(sprite != null
            ? "✅ Card sprite loaded successfully"
            : "❌ Failed to load card sprite");
    }
}
