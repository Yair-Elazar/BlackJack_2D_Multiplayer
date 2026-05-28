using System.Collections.Generic;

public class HandState
{
    public Hand Hand { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsFinished { get; private set; }

    public bool IsBusted => Hand.IsBust();
    public int Bet { get; private set; }


    public HandState(Hand hand)
    {
        Hand = hand;
        IsActive = false;
        IsFinished = false;
        Bet = 0;
    }
    public void SetBet(int amount)
    {
        Bet = amount;
    }

    public void SetActive(bool value)
    {
        if (IsFinished) return;

        IsActive = value;
    }

    public void Finish()
    {
        IsFinished = true;
        IsActive = false;
    }

    public int GetTotalValue()
    {
        return Hand.GetTotalValue();
    }

    public IReadOnlyList<Card> Cards => Hand.Cards;

    // 🔥 תוספת חשובה כדי למנוע שגיאות UI ישנות
    public bool CheckBust()
    {
        if (Hand.IsBust())
        {
            Finish();
            return true;
        }
        return false;
    }

    public void Reset()
{
    IsActive = false;
    IsFinished = false;
    Bet = 0;
    Hand.ResetHand();
}
}