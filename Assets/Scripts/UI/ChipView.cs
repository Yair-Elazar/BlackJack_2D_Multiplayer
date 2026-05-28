using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChipView : MonoBehaviour
{
    [SerializeField] private Image chipImage;
    [SerializeField] private TMP_Text amountText;

    public int Amount { get; private set; }
    public Sprite Sprite { get; private set; }

    public void SetChip(int amount, Sprite sprite)
    {
        Amount = amount;
        Sprite = sprite;

        chipImage.sprite = sprite;
        amountText.text = "$" + amount;
    }
}