using UnityEngine;

public class PowerUp : MonoBehaviour
{
    AudioManager audioManager;
    public enum Type
    {
        Coin,
        ExtraLife,
        MagicMushroom,
        Starpower,
    }

    public Type type;

    private void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player player)) {
            Collect(player);
        }
    }

    private void Collect(Player player)
    {
        switch (type)
        {
            case Type.Coin:
                audioManager.PlaySoundEffect(audioManager.Coin);
                GameManager.Instance.AddCoin();
                break;

            case Type.ExtraLife:
                GameManager.Instance.AddLife();
                break;

            case Type.MagicMushroom:
                player.Grow();
                break;

            case Type.Starpower:
                player.Starpower();
                break;
        }

        Destroy(gameObject);
    }

}
