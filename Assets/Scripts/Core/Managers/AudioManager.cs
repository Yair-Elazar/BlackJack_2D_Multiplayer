using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Card Sounds")]
    [SerializeField] private AudioClip cardDealClip;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayCardDeal()
    {
        if (cardDealClip != null)
            sfxSource.PlayOneShot(cardDealClip);
    }
}