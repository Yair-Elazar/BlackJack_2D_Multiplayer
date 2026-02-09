using UnityEngine;

public static class CardSpriteResolver
{
    public static Sprite GetCardSprite(Card card)
    {
        string suit = card.Suit switch
        {
            Card.SuitType.Clubs => "Clubs",
            Card.SuitType.Diamonds => "Diamonds",
            Card.SuitType.Hearts => "Hearts",
            Card.SuitType.Spades => "Spades",
            _ => ""
        };

        string rank = card.Rank switch
        {
            Card.RankType.Ace => "A",
            Card.RankType.King => "K",
            Card.RankType.Queen => "Q",
            Card.RankType.Jack => "J",
            Card.RankType.Ten => "10",
            Card.RankType.Nine => "9",
            Card.RankType.Eight => "8",
            Card.RankType.Seven => "7",
            Card.RankType.Six => "6",
            Card.RankType.Five => "5",
            Card.RankType.Four => "4",
            Card.RankType.Three => "3",
            Card.RankType.Two => "2",
            _ => ""
        };

        string path = $"CardSprites/card{suit}{rank}";
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogError($"Card sprite not found at path: {path}");
        }

        return sprite;
    }

    public static Sprite GetCardBack()
    {
        return Resources.Load<Sprite>("CardSprites/cardBack_red4");
    }
}
