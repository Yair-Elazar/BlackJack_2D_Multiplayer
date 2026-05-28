using UnityEngine;

public class PlayerHandsLayout : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private RectTransform hand0Root;
    [SerializeField] private RectTransform hand1Root;

    [SerializeField] private float spacing = 30f;

    public void UpdateLayout(int handCount)
{
    if (handCount <= 1)
    {
        hand0Root.anchoredPosition = Vector2.zero;

        if (hand1Root != null)
            hand1Root.gameObject.SetActive(false);

        return;
    }

    hand1Root.gameObject.SetActive(true);

    hand0Root.anchoredPosition = new Vector2(-spacing / 2f, 0);
    hand1Root.anchoredPosition = new Vector2(spacing / 2f, 0);
}
}