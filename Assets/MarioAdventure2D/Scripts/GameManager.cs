using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    #region GameManager
    /*public static GameManager Instance { get; private set; }

    public int world { get; private set; } = 1;
    public int stage { get; private set; } = 1;
    public int lives { get; private set; } = 3;
    public int coins { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        NewGame();
    }

    public void NewGame()
    {
        lives = 3;
        coins = 0;

        LoadLevel(world, stage);
    }

    public void GameOver()
    {
        NewGame();
    }

    public void LoadLevel(int world, int stage)
    {
        this.world = world;
        this.stage = stage;

        SceneManager.LoadScene($"{world}-{stage}");
    }

    public void NextLevel()
    {
        LoadLevel(world, stage + 1);
    }

    public void ResetLevel(float delay)
    {
        CancelInvoke(nameof(ResetLevel));
        Invoke(nameof(ResetLevel), delay);
    }

    public void ResetLevel()
    {
        lives--;

        if (lives > 0) {
            LoadLevel(world, stage);
        } else {
            GameOver();
        }
    }

    public void AddCoin()
    {
        coins++;

        if (coins == 100)
        {
            coins = 0;
            AddLife();
        }
    }

    public void AddLife()
    {
        lives++;
    }*/
    #endregion

    // Singleton instance to allow easy access from other scripts
    public static GameManager Instance { get; private set; }

    // Game state variables
    public int world { get; private set; } = 1;
    public int stage { get; private set; } = 1;
    public int lives { get; private set; } = 3;
    public int coins { get; private set; } = 0;

    private void Awake()
    {
        // Implement the singleton pattern
        if (Instance != null)
        {
            // If another instance already exists, destroy this one
            Debug.LogWarning("GameManager: Duplicate instance found, destroying this GameObject.");
            DestroyImmediate(gameObject);
        }
        else
        {
            // Set this as the singleton instance and prevent it from being destroyed on scene load
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager: Instance set and DontDestroyOnLoad applied.");
        }
    }

    private void OnDestroy()
    {
        // Clean up the singleton instance when this object is destroyed
        if (Instance == this)
        {
            Instance = null;
            Debug.Log("GameManager: Instance cleared on Destroy.");
        }
    }

    private void Start()
    {
        // Set the target frame rate for consistent performance
        Application.targetFrameRate = 60;
        Debug.Log("GameManager: Target frame rate set to 60.");
        NewGame(); // Start a new game when the GameManager initializes
    }

    // Resets game state and loads the first level
    public void NewGame()
    {
        lives = 3;
        coins = 0;
        world = 1; // Ensure world is reset for a new game
        stage = 1; // Ensure stage is reset for a new game

        Debug.Log($"GameManager: Starting new game. Loading World {world}, Stage {stage}.");
        LoadLevel(world, stage);
    }

    // Handles game over condition, typically by starting a new game
    public void GameOver()
    {
        Debug.Log("GameManager: Game Over! Starting new game.");
        NewGame();
    }

    // Loads a specific level based on world and stage numbers
    public void LoadLevel(int world, int stage)
    {
        this.world = world; // Update the current world
        this.stage = stage; // Update the current stage

        string sceneName = $"{this.world}-{this.stage}"; // Construct the scene name
        Debug.Log($"GameManager: Attempting to load scene: '{sceneName}'");

        // LOGIC FOR SCENE EXISTENCE CHECK 
        bool sceneFoundInBuildSettings = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string nameOnly = Path.GetFileNameWithoutExtension(path); // Get just the name without .unity extension

            if (nameOnly == sceneName)
            {
                sceneFoundInBuildSettings = true;
                break;
            }
        }

        if (sceneFoundInBuildSettings)
        {
            SceneManager.LoadScene(sceneName); // Load the scene
            Debug.Log($"GameManager: Successfully requested to load scene '{sceneName}'.");
        }
        else
        {
            // If the scene is not found, log an error and start a new game
            Debug.LogError($"GameManager: Scene '{sceneName}' not found in Build Settings! Falling back to New Game (World 1, Stage 1).");
            NewGame();
        }
    }

    // Advances to the next stage in the current world
    public void NextLevel()
    {
        // Increment the stage before loading the next level
        stage++;
        Debug.Log($"GameManager: Advancing to next level. New stage will be {stage}.");
        LoadLevel(world, stage);
    }

    // Resets the current level after a delay
    public void ResetLevel(float delay)
    {
        CancelInvoke(nameof(ResetLevel)); // Cancel any pending invokes to prevent multiple calls
        Invoke(nameof(ResetLevel), delay); // Schedule the ResetLevel call after the delay
        Debug.Log($"GameManager: Scheduling level reset in {delay} seconds.");
    }

    // Resets the current level (decreases lives or triggers game over)
    public void ResetLevel()
    {
        lives--; // Decrease a life
        Debug.Log($"GameManager: Resetting level. Lives remaining: {lives}.");

        if (lives > 0)
        {
            LoadLevel(world, stage); // Reload the current level if lives remain
        }
        else
        {
            GameOver(); // Trigger game over if no lives left
        }
    }

    // Adds a coin and potentially a life
    public void AddCoin()
    {
        coins++;
        Debug.Log($"GameManager: Coin collected. Total coins: {coins}.");

        if (coins == 100)
        {
            coins = 0;
            AddLife(); // Add a life if 100 coins are collected
            Debug.Log("GameManager: 100 coins collected! Coins reset, life added.");
        }
    }

    // Adds a life
    public void AddLife()
    {
        lives++;
        Debug.Log($"GameManager: Life added. Total lives: {lives}.");
    }
}
