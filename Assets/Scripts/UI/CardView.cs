using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    public IEnumerator FlipCard(Card card)
{
    float duration = 0.2f;
    float elapsed = 0f;

    RectTransform rect = GetComponent<RectTransform>();

    // חצי ראשון – סגירה
    while (elapsed < duration / 2f)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / (duration / 2f);
        rect.localScale = new Vector3(Mathf.Lerp(1f, 0f, t), 1f, 1f);
        yield return null;
    }

    // החלפת ספרייט
    SetCard(card, false);

    elapsed = 0f;

    // חצי שני – פתיחה
    while (elapsed < duration / 2f)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / (duration / 2f);
        rect.localScale = new Vector3(Mathf.Lerp(0f, 1f, t), 1f, 1f);
        yield return null;
    }

    rect.localScale = Vector3.one;
}

}
