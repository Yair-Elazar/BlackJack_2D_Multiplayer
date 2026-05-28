using UnityEngine;

public class HandLayoutController : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float spacing = 40f;
    [SerializeField] private float fanAngle = 10f;

    public void Arrange()
    {
        int count = transform.childCount;

        if (count == 0) return;

        float center = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            Transform card = transform.GetChild(i);
            RectTransform rt = card.GetComponent<RectTransform>();

            float offset = i - center;

            // position (overlap / fan mix)
            rt.anchoredPosition = new Vector2(offset * spacing, 0);

            // rotation (fan effect)
            rt.localRotation = Quaternion.Euler(0, 0, -offset * fanAngle);
        }
    }
}