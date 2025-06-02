using System.Collections;
using UnityEngine;

public class FlagPole : MonoBehaviour
{
    #region FlagePole
    /*public Transform flag;
    public Transform poleBottom;
    public Transform castle;
    public float speed = 6f;
    public int nextWorld = 1;
    public int nextStage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            StartCoroutine(MoveTo(flag, poleBottom.position));
            StartCoroutine(LevelCompleteSequence(player));
        }
    }

    private IEnumerator LevelCompleteSequence(Player player)
    {
        player.movement.enabled = false;

        yield return MoveTo(player.transform, poleBottom.position);
        yield return MoveTo(player.transform, player.transform.position + Vector3.right);
        yield return MoveTo(player.transform, player.transform.position + Vector3.right + Vector3.down);
        yield return MoveTo(player.transform, castle.position);

        player.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        GameManager.Instance.LoadLevel(nextWorld, nextStage);
    }

    private IEnumerator MoveTo(Transform subject, Vector3 position)
    {
        while (Vector3.Distance(subject.position, position) > 0.125f)
        {
            subject.position = Vector3.MoveTowards(subject.position, position, speed * Time.deltaTime);
            yield return null;
        }

        subject.position = position;
    }*/
    #endregion

    AudioManager audioManager;

    public Transform flag;
    public Transform poleBottom;
    public Transform castle;
    public float speed = 6f;

    private void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            // Disable the collider immediately so it doesn't trigger multiple times
            GetComponent<Collider2D>().enabled = false;

            StartCoroutine(MoveTo(flag, poleBottom.position));
            StartCoroutine(LevelCompleteSequence(player));
        }
    }

    private IEnumerator LevelCompleteSequence(Player player)
    {
        audioManager.PlaySoundEffect(audioManager.FlagPole);

        player.movement.enabled = false; // Disable player movement

        // Player animation sequence
        yield return MoveTo(player.transform, poleBottom.position);
        yield return MoveTo(player.transform, player.transform.position + Vector3.right);
        yield return MoveTo(player.transform, player.transform.position + Vector3.right + Vector3.down);
        yield return MoveTo(player.transform, castle.position);

        player.gameObject.SetActive(false); // Hide the player

        yield return new WaitForSeconds(2f); // Wait for 2 seconds

        // *** THIS IS THE CRUCIAL CHANGE ***
        // Instead of hardcoding 1-1, tell the GameManager to advance to the next level
        Debug.Log("FlagPole: Level completed! Requesting next level from GameManager.");
        GameManager.Instance.NextLevel();
    }

    private IEnumerator MoveTo(Transform subject, Vector3 position)
    {
        while (Vector3.Distance(subject.position, position) > 0.125f)
        {
            subject.position = Vector3.MoveTowards(subject.position, position, speed * Time.deltaTime);
            yield return null;
        }

        subject.position = position; // Ensure the subject is at the exact position
    }

}
