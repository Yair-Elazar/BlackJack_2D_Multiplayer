using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Card Sounds")]
    [SerializeField] private AudioClip cardDealClip;

    [Header("Chip Sounds")]
    [SerializeField] private AudioClip chipPlacedClip;

    

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayCardDeal()
    {
        Debug.Log("PLAY CARD SOUND");
        if (cardDealClip != null)
            sfxSource.PlayOneShot(cardDealClip);
    }

    public void PlayChipPlace()
{
    sfxSource.PlayOneShot(chipPlacedClip);
}
}