public class Dealer
{
    private readonly Hand hand = new Hand();

    public Hand Hand => hand;

    public void PlayTurn(Deck deck)
    {
        if (deck == null)
            return;

        while (!hand.IsBust())
        {
            int total = hand.GetTotalValue();

            if (total >= 17)
                break;

            Card card = deck.DrawCard();

            if (card == null)
                break;

            hand.AddCard(card);
        }
    }

    public Hand RevealHand()
    {
        return hand;
    }

    public void ResetHand()
    {
        hand.ResetHand();
    }
}