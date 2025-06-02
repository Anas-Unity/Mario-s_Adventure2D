using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("~~~~~~~~~~~ Audio Source ~~~~~~~~~~~")]
    public AudioSource musicSource;
    [SerializeField] AudioSource soundSource;

    [Header("~~~~~~~~~~~ Audio Clip ~~~~~~~~~~~")]
    public AudioClip BackgroundMusic;
    public AudioClip Coin;
    public AudioClip FlagPole;
    public AudioClip GameOver;
    public AudioClip KoopaStomp;
    public AudioClip GoombaDeath;
    public AudioClip Jump;
    public AudioClip KoopaKick;
    public AudioClip PowerUp;
    public AudioClip MarioShrink;


    private void Start()
    {
        musicSource.clip = BackgroundMusic;
        musicSource.Play();
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        soundSource.PlayOneShot(clip);
    }
}
