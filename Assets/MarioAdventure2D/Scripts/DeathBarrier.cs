using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DeathBarrier : MonoBehaviour
{
    AudioManager audioManager;

    private void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            audioManager.musicSource.Stop();
            audioManager.PlaySoundEffect(audioManager.GameOver);
            

            other.gameObject.SetActive(false);
            GameManager.Instance.PlayerFailedLevelAttempt();
        }
        else
        {
            Destroy(other.gameObject);
        }
    }

}
