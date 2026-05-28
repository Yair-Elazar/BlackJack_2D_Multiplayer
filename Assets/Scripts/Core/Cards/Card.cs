public class Card
{
    public enum SuitType
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }

    public enum RankType
    {
        Ace = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    public SuitType Suit { get; private set; }
    public RankType Rank { get; private set; }

    public Card(RankType rank, SuitType suit)
    {
        Rank = rank;
        Suit = suit;
    }

    public int GetValue()
    {
        if (Rank == RankType.Ace)
            return 1;

        if (Rank >= RankType.Jack)
            return 10;

        return (int)Rank;
    }

    public int MaxValue => Rank == RankType.Ace ? 11 : GetValue();

    public override string ToString()
    {
        return $"{Rank} of {Suit}";
    }
}