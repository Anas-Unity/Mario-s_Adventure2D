using UnityEngine;

public class HomeManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void StartGame()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        gameManager.OnStartGameButtonClicked();
    }
    public void ExitGame()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        gameManager.OnExitButtonClicked();
    }
}
