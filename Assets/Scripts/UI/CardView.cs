using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual representation of a single playing card.
/// </summary>
public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;

    public void SetCard(Card card, bool faceDown = false)
    {
        if (faceDown)
        {
            cardImage.sprite = CardSpriteResolver.GetCardBack();
        }
        else
        {
            cardImage.sprite = CardSpriteResolver.GetCardSprite(card);
        }
    }
}
