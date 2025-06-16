using System.IO; // Required for Path.GetFileNameWithoutExtension
using UnityEngine; // Core Unity functionalities like MonoBehaviour, GameObject, Debug
using UnityEngine.SceneManagement; // For loading scenes (SceneManager, SceneUtility)
using UnityEngine.UI; // For UI elements like Text
// using TMPro; // Uncomment if you are using TextMeshPro for your UI Text components

/// Manages overall game state, level progression, player statistics (lives, coins),
/// UI canvas visibility, and persistence across scenes.
/// Implements a Singleton pattern to ensure only one instance exists.
[DefaultExecutionOrder(-1)] // Ensures this script runs before most other scripts
public class GameManager : MonoBehaviour
{
    // --- Singleton Instance ---
    /// Static reference to the single instance of the GameManager.
    /// Allows other scripts to easily access GameManager functionality.
    public static GameManager Instance { get; private set; }

    // --- Game State Variables ---
    /// The current world number the player is in.
    private int _world = 1;
    public int world
    {
        get { return _world; }
        private set
        {
            if (_world != value)
            {
                Debug.Log($"GameManager: World changed from {_world} to {value}.");
                _world = value;
            }
        }
    }

    /// The current stage (level) number within the current world.
    private int _stage = 1;
    public int stage
    {
        get { return _stage; }
        private set
        {
            if (_stage != value)
            {
                Debug.Log($"GameManager: Stage changed from {_stage} to {value}.");
                _stage = value;
            }
        }
    }

    /// The player's current number of lives.
    public int lives { get; private set; } = 5;

    /// The player's current number of collected coins.
    public int coins { get; private set; } = 0;

    /// Indicates whether active gameplay is currently in progress (true) or if a menu/canvas is displayed (false).
    public bool IsGameActive { get; private set; } = false;

    // --- Scene Management ---
    [Header("Scene Management")]
    /// The name of your main menu scene. Ensure this matches the scene name in Build Settings.
    public string mainMenuSceneName = "MainMenu";

    // --- Life Regeneration Settings ---
    [Header("Life Regeneration Settings")]
    /// The maximum number of lives the player can accumulate.
    public int maxLives = 5;
    /// The time in seconds it takes for one life to regenerate.
    public float liveRegenInterval = 60f;
    private float nextLiveRegenTime = 0f; // Internal timer for life regeneration

    // --- Animation Delay Settings ---
    [Header("Animation Delay Settings")]
    /// Delay in seconds before game over or level complete canvases appear,
    /// allowing death or completion animations to play out.
    [Tooltip("Delay in seconds before Game Over or Level Complete canvas appears.")]
    public float canvasShowDelay = 3f;

    // --- UI References (Assigned in Inspector for MainMenu, Found Dynamically for Levels) ---
    [Header("UI Canvases")]
    /// Reference to the main start game canvas found only in the MainMenu scene.
    /// Must be assigned manually in the Inspector in the MainMenu scene.
    public GameObject startGameCanvas;

    // Main Menu Button References (for programmatic re-wiring)
    [Tooltip("Assign the 'Start Game' button from your StartGameCanvas in the MainMenu scene.")]
    public Button startGameButton;
    [Tooltip("Assign the 'Exit' button from your StartGameCanvas in the MainMenu scene.")]
    public Button exitGameButton;

    /// Reference to the Game Over canvas, dynamically found in gameplay scenes.
    public GameObject gameOverCanvas;
    /// Reference to the Level Complete canvas, dynamically found in gameplay scenes.
    public GameObject levelCompleteCanvas;
    /// Reference to the All Levels Complete canvas, dynamically found in gameplay scenes.
    public GameObject allLevelsCompleteCanvas;
    /// Reference to the Text component displaying the current coin count in gameplay scenes.
    public Text coinCounterText;
    /// Reference to the Text component displaying the current lives count in gameplay scenes.
    public Text livesCounterText;

    // --- Unity Lifecycle Methods ---

    /// Called when the script instance is being loaded. Initializes the Singleton pattern
    /// and subscribes to scene loaded events.
    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null)
        {
            Debug.LogWarning("GameManager: Duplicate instance found, destroying this GameObject to prevent conflicts.");
            DestroyImmediate(gameObject); // Destroy duplicate instance
        }
        else
        {
            Instance = this; // Set this as the singleton instance
            DontDestroyOnLoad(gameObject); // Persist this GameObject across scene loads
            Debug.Log("GameManager: Instance set and DontDestroyOnLoad applied.");
        }

        // Initial check for the startGameCanvas assignment in the Inspector
        CheckAssignedCanvas(startGameCanvas, "Start Game Canvas");

        // Subscribe to scene loaded event to handle UI and game state updates on scene change
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// Called when a new scene is loaded. Manages UI references and game state based on the loaded scene.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameManager: Scene loaded: {scene.name}. Current World: {world}, Current Stage: {stage}");

        if (scene.name != mainMenuSceneName)
        {
            // If a gameplay scene is loaded, find dynamic UI, enable player movement, and update UI counters.
            FindDynamicUIReferences();
            EnablePlayerMovement();
            UpdateCoinText();
            UpdateLivesText();
        }
        else // If the MainMenu scene is loaded
        {
            DisablePlayerMovement(); // Game is not active in menu mode

            // If startGameCanvas reference was lost (common for persistent objects referencing scene objects), find it dynamically.
            if (startGameCanvas == null)
            {
                Debug.LogWarning("GameManager: startGameCanvas was NULL on scene load, attempting to find it dynamically.");
                GameObject foundStartCanvasGO = GameObject.Find("StartGameCanvas");
                if (foundStartCanvasGO != null)
                {
                    startGameCanvas = foundStartCanvasGO;
                    Debug.Log("GameManager: startGameCanvas found dynamically and reassigned.");
                }
                else
                {
                    Debug.LogError("GameManager: startGameCanvas still not found dynamically. UI won't show!");
                }
            }

            // Show the main menu canvas and re-wire its buttons.
            if (startGameCanvas != null)
            {
                ShowCanvas(startGameCanvas);
                WireUpMainMenuButtons();
            }
            else
            {
                Debug.LogError("GameManager: Start Game Canvas is NULL after scene load and dynamic find. UI won't show!");
            }
        }
    }

    /// Called on the first frame the script is enabled. Sets the target frame rate
    /// and initializes the main menu UI if starting directly in the MainMenu scene.
    private void Start()
    {
        Application.targetFrameRate = 60; // Standard target frame rate for smooth gameplay
        Debug.Log("GameManager: Target frame rate set to 60.");

        // If the game starts directly in the MainMenu scene, ensure UI is set up.
        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
        {
            IsGameActive = false;
            if (startGameCanvas == null)
            {
                Debug.LogError("GameManager: Start Game Canvas is NULL in Start()! Please assign it in the Inspector in the MainMenu scene. Buttons won't work.");
            }
            else
            {
                ShowCanvas(startGameCanvas);
                WireUpMainMenuButtons();
            }
        }

        // Update initial UI displays (these will be updated by OnSceneLoaded for in-game UI)
        UpdateCoinText();
        UpdateLivesText();

        // Load persistent lives data if available
        if (PlayerPrefs.HasKey("Lives"))
        {
            lives = PlayerPrefs.GetInt("Lives");
            Debug.Log($"GameManager: Loaded lives from PlayerPrefs: {lives}");
        }
        if (PlayerPrefs.HasKey("NextLiveRegenTime"))
        {
            nextLiveRegenTime = PlayerPrefs.GetFloat("NextLiveRegenTime");
            Debug.Log($"GameManager: Loaded nextLiveRegenTime from PlayerPrefs: {nextLiveRegenTime}");
        }
        // If lives are at max, ensure the regeneration timer is not active.
        if (lives >= maxLives)
        {
            nextLiveRegenTime = 0f;
            PlayerPrefs.SetFloat("NextLiveRegenTime", nextLiveRegenTime);
            PlayerPrefs.Save();
        }
    }

    /// Called once per frame. Handles passive life regeneration based on time.
    private void Update()
    {
        // Regenerate lives if below max and a timer is active
        if (lives < maxLives && nextLiveRegenTime > 0f)
        {
            if (Time.time >= nextLiveRegenTime)
            {
                AddLife(); // Grant a life
                Debug.Log("GameManager: A life has regenerated!");

                // If still not at max, schedule the next regeneration
                if (lives < maxLives)
                {
                    nextLiveRegenTime = Time.time + liveRegenInterval;
                    PlayerPrefs.SetFloat("NextLiveRegenTime", nextLiveRegenTime);
                    PlayerPrefs.Save();
                    Debug.Log($"GameManager: Next life will regenerate at: {nextLiveRegenTime} (in {liveRegenInterval}s)");
                }
                else // If now at max lives, stop the timer
                {
                    nextLiveRegenTime = 0f;
                    PlayerPrefs.SetFloat("NextLiveRegenTime", nextLiveRegenTime);
                    PlayerPrefs.Save();
                    Debug.Log("GameManager: Lives are now at maximum, regeneration timer stopped.");
                }
            }
        }
    }

    /// Called when the GameObject is being destroyed. Cleans up the Singleton instance
    /// and unsubscribes from events to prevent memory leaks.
    private void OnDestroy()
    {
        // Clear the static instance reference if this is the one being destroyed
        if (Instance == this)
        {
            Instance = null;
            Debug.Log("GameManager: Instance cleared on Destroy.");
        }
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from scene loaded event
    }

    // --- UI Canvas Management Helper Methods ---

    /// Hides all dynamically found UI canvases (Game Over, Level Complete, All Levels Complete).
    private void HideAllCanvases()
    {
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        if (levelCompleteCanvas != null) levelCompleteCanvas.SetActive(false);
        if (allLevelsCompleteCanvas != null) allLevelsCompleteCanvas.SetActive(false); 
        Debug.Log("GameManager: Attempted to hide all dynamically found UI canvases.");
    }

    /// Shows a specific UI canvas after hiding all others.
    /// Sets IsGameActive to false as UI is displayed.
    private void ShowCanvas(GameObject canvasToShow)
    {
        if (canvasToShow == null)
        {
            Debug.LogError($"GameManager: Cannot show a null canvas. Please ensure the canvas GameObject for this state is in the current scene and named correctly.");
            return;
        }

        HideAllCanvases(); // Ensure only one canvas is active at a time
        canvasToShow.SetActive(true);
        IsGameActive = false; // Game is paused or in a menu state
        Debug.Log($"GameManager: Showing canvas: {canvasToShow.name}, IsGameActive: {IsGameActive}");
    }

    /// Sets IsGameActive to true, indicating active gameplay.
    /// Player movement scripts should read this flag to enable/disable player control.
    private void EnablePlayerMovement()
    {
        IsGameActive = true;
        Debug.Log($"GameManager: Game is active, IsGameActive: {IsGameActive}. Player movement should now be enabled by player script.");
    }

    /// Sets IsGameActive to false, indicating game is paused or in a menu.
    /// Player movement scripts should read this flag to enable/disable player control.
    private void DisablePlayerMovement()
    {
        IsGameActive = false;
        Debug.Log($"GameManager: Game is not active, IsGameActive: {IsGameActive}. Player movement should now be disabled by player script.");
    }

    /// Finds and assigns references to all dynamic UI canvases (Game Over, Level Complete, All Levels Complete)
    /// and Text components for coins and lives in the currently loaded gameplay scene.
    /// This uses `FindObjectsOfType<T>(true)` to find active and inactive objects.
    private void FindDynamicUIReferences()
    {
        Debug.Log("GameManager: Attempting to find dynamic UI references in current scene...");

        // Find GameOverCanvas
        foreach (Canvas canvas in FindObjectsOfType<Canvas>(true))
        {
            if (canvas.name == "GameOverCanvas")
            {
                gameOverCanvas = canvas.gameObject;
                break;
            }
        }
        CheckFoundCanvas(gameOverCanvas, "Game Over Canvas");

        // Find LevelCompleteCanvas
        foreach (Canvas canvas in FindObjectsOfType<Canvas>(true))
        {
            if (canvas.name == "LevelCompleteCanvas")
            {
                levelCompleteCanvas = canvas.gameObject;
                break;
            }
        }
        CheckFoundCanvas(levelCompleteCanvas, "Level Complete Canvas");

        // Find AllLevelsCompleteCanvas
        foreach (Canvas canvas in FindObjectsOfType<Canvas>(true))
        {
            if (canvas.name == "AllLevelsCompleteCanvas")
            {
                allLevelsCompleteCanvas = canvas.gameObject;
                break;
            }
        }
        CheckFoundCanvas(allLevelsCompleteCanvas, "All Levels Complete Canvas");

        // Find Coin Counter Text
        foreach (Text textComponent in FindObjectsOfType<Text>(true))
        {
            if (textComponent.gameObject.name == "CoinsCounter")
            {
                coinCounterText = textComponent;
                break;
            }
        }
        if (coinCounterText == null) Debug.LogWarning("GameManager: 'CoinsCounter' Text component not found directly in current scene. In-game coin UI won't update.");
        else Debug.Log("GameManager: CoinCounterText assigned successfully.");

        // Find Lives Counter Text
        foreach (Text textComponent in FindObjectsOfType<Text>(true))
        {
            if (textComponent.gameObject.name == "LivesCounter")
            {
                livesCounterText = textComponent;
                break;
            }
        }
        if (livesCounterText == null) Debug.LogWarning("GameManager: 'LivesCounter' Text component not found directly in current scene. In-game lives UI won't update.");
        else Debug.Log("GameManager: LivesCounterText assigned successfully.");
    }

    /// Helper to check if a canvas reference was assigned in the Inspector.
    /// Used for statically assigned UI like the Main Menu canvas.
    private void CheckAssignedCanvas(GameObject canvas, string canvasName)
    {
        if (canvas == null)
        {
            Debug.LogError($"GameManager: '{canvasName}' canvas is NOT assigned in the Inspector! Please drag the corresponding GameObject into the slot on the GameManager in the MainMenu scene.");
        }
        else
        {
            Debug.Log($"GameManager: '{canvasName}' canvas is assigned successfully.");
        }
    }

    /// Helper to check if a canvas GameObject was successfully found dynamically in the scene.
    /// Disables the canvas by default if found.
    private void CheckFoundCanvas(GameObject canvas, string canvasName)
    {
        if (canvas == null)
        {
            Debug.LogError($"GameManager: '{canvasName}' canvas was NOT found in the current scene. Please ensure it exists and is named correctly.");
        }
        else
        {
            Debug.Log($"GameManager: '{canvasName}' canvas found and assigned dynamically.");
            canvas.SetActive(false); // Ensure it's hidden by default when found
        }
    }

    /// Programmatically wires up the OnClick events for the main menu buttons.
    /// This is necessary because button references can be lost when scenes unload/reload.
    private void WireUpMainMenuButtons()
    {
        Debug.Log("GameManager: Attempting to wire up Main Menu buttons...");
        if (startGameCanvas != null)
        {
            // Find and wire up the "Start Game" button
            Transform startGameButtonTransform = FindChildRecursive(startGameCanvas.transform, "StartGameButton");
            if (startGameButtonTransform != null)
            {
                startGameButton = startGameButtonTransform.GetComponent<Button>();
                if (startGameButton != null)
                {
                    startGameButton.onClick.RemoveAllListeners(); // Clear existing to prevent duplicate calls
                    startGameButton.onClick.AddListener(OnStartGameButtonClicked);
                    Debug.Log("GameManager: 'Start Game' button re-wired successfully.");
                }
                else
                {
                    Debug.LogWarning("GameManager: 'StartGameButton' GameObject found but has no Button component. Check its components.");
                }
            }
            else
            {
                Debug.LogWarning("GameManager: 'StartGameButton' GameObject not found as child of StartGameCanvas. Check name and hierarchy.");
            }

            // Find and wire up the "Exit" button
            Transform exitGameButtonTransform = FindChildRecursive(startGameCanvas.transform, "ExitGameButton");
            if (exitGameButtonTransform != null)
            {
                exitGameButton = exitGameButtonTransform.GetComponent<Button>();
                if (exitGameButton != null)
                {
                    exitGameButton.onClick.RemoveAllListeners(); // Clear existing to prevent duplicate calls
                    exitGameButton.onClick.AddListener(OnExitButtonClicked);
                    Debug.Log("GameManager: 'Exit Game' button re-wired successfully.");
                }
                else
                {
                    Debug.LogWarning("GameManager: 'ExitGameButton' GameObject found but has no Button component. Check its components.");
                }
            }
            else
            {
                Debug.LogWarning("GameManager: 'ExitGameButton' GameObject not found as child of StartGameCanvas. Check name and hierarchy.");
            }
        }
        else
        {
            Debug.LogWarning("GameManager: startGameCanvas is null, cannot wire up main menu buttons.");
        }
    }

    /// Recursively searches for a child GameObject by name under a given parent Transform.
    /// <param name="parent">The parent Transform to start searching from.</param>
    /// <param name="childName">The name of the child GameObject to find.</param>
    /// <returns>The Transform of the found child, or null if not found.</returns>
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            Transform found = FindChildRecursive(child, childName); // Recursive call
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    // --- Game State Management Methods ---

    /// Resets all core game statistics (lives, coins, world, stage) to their initial "new game" values.
    public void ResetGameStats()
    {
        lives = 5;
        coins = 0;
        world = 1; // Resets world via custom setter
        stage = 1; // Resets stage via custom setter
        nextLiveRegenTime = 0f; // Reset regeneration timer on new game
        PlayerPrefs.SetInt("Lives", lives); // Save initial lives
        PlayerPrefs.SetFloat("NextLiveRegenTime", nextLiveRegenTime); // Save timer state
        PlayerPrefs.Save(); // Persist changes to PlayerPrefs
        Debug.Log("GameManager: Game stats reset to initial values.");
    }

    /// Initiates a new game by resetting stats and loading the first level (1-1).
    /// Typically called when the "Start Game" button is clicked.
    public void StartGame()
    {
        Debug.Log("GameManager: StartGame called. Resetting stats to World 1, Stage 1.");
        ResetGameStats(); // Resets stats (sets stage to 1)
        LoadLevel(world, stage); // Load the first level (1-1)
        Debug.Log("GameManager: New game initiated, loading first level.");
    }

    /// Handles player death or level failure. Deducts a life and displays the Game Over canvas after a delay.
    /// If no lives remain, it will only display the Game Over canvas.
    public void PlayerFailedLevelAttempt()
    {
        Debug.Log($"GameManager: PlayerFailedLevelAttempt called. Current World: {world}, Current Stage: {stage}. Lives before deduction: {lives}");
        if (lives > 0)
        {
            lives--; // Deduct one life
            PlayerPrefs.SetInt("Lives", lives); // Save lives
            // Start/reset life regeneration timer if not at max lives
            if (lives < maxLives)
            {
                nextLiveRegenTime = Time.time + liveRegenInterval;
                PlayerPrefs.SetFloat("NextLiveRegenTime", nextLiveRegenTime);
            }
            else
            {
                nextLiveRegenTime = 0f; // Clear timer if somehow at max lives after deduction
                PlayerPrefs.SetFloat("NextLiveRegenTime", nextLiveRegenTime);
            }
            PlayerPrefs.Save(); // Persist changes
            Debug.Log($"GameManager: Player failed level attempt. Lives remaining: {lives}. Current stage is still: {stage}");
            UpdateLivesText(); // Update lives UI
        }
        else
        {
            Debug.Log("GameManager: Player failed level attempt but has no lives to deduct. Showing Game Over.");
        }

        Invoke(nameof(DelayedShowGameOverCanvas), canvasShowDelay); // Delay showing canvas for death animation
    }

    /// Internal method invoked after a delay to show the Game Over canvas.
    private void DelayedShowGameOverCanvas()
    {
        Debug.Log($"GameManager: DelayedShowGameOverCanvas called. Current World: {world}, Current Stage: {stage}");
        ShowCanvas(gameOverCanvas);
    }

    /// Handles successful level completion. Displays the Level Complete canvas after a delay.
    public void LevelComplete()
    {
        Debug.Log($"GameManager: Level Completed! Displaying Level Complete canvas. Current World: {world}, Current Stage: {stage}");
        Invoke(nameof(DelayedShowLevelCompleteCanvas), canvasShowDelay); // Delay showing canvas for completion animation
    }

    /// Internal method invoked after a delay to show the Level Complete canvas.
    private void DelayedShowLevelCompleteCanvas()
    {
        Debug.Log($"GameManager: DelayedShowLevelCompleteCanvas called. Current World: {world}, Current Stage: {stage}");
        ShowCanvas(levelCompleteCanvas);
    }

    /// Called when all levels in the game have been completed. Displays the All Levels Complete canvas
    /// and resets game statistics for a new playthrough.
    public void AllLevelsCompleted()
    {
        Debug.Log($"GameManager: All levels completed! Displaying All Levels Complete canvas. Current World: {world}, Current Stage: {stage}");
        ShowCanvas(allLevelsCompleteCanvas);
        ResetGameStats(); // Reset stats for a potential new playthrough
    }

    /// Loads a specific game level based on provided world and stage numbers.
    /// Checks if the scene exists in Build Settings before attempting to load.
    private void LoadLevel(int world, int stage)
    {
        this.world = world; // Update current world property
        this.stage = stage; // Update current stage property

        Debug.Log($"GameManager: LoadLevel called. Target World: {this.world}, Target Stage: {this.stage}");

        string sceneName = $"{this.world}-{this.stage}"; // Construct scene name (e.g., "1-1")

        // Check if the target scene exists in Unity's Build Settings
        bool sceneFoundInBuildSettings = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string nameOnly = Path.GetFileNameWithoutExtension(path);

            if (nameOnly == sceneName)
            {
                sceneFoundInBuildSettings = true;
                break;
            }
        }

        if (sceneFoundInBuildSettings)
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log($"GameManager: Successfully requested to load scene '{sceneName}'.");
        }
        else
        {
            Debug.LogError($"GameManager: Scene '{sceneName}' not found in Build Settings!");

            // Fallback to Main Menu if a genuinely missing level is attempted to be loaded directly.
            // This case handles direct calls to LoadLevel for non-existent scenes, not progression from NextLevel.
            Debug.LogWarning("GameManager: Encountered a genuinely missing level during direct load. Returning to MainMenu.");
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    /// Advances the game to the next stage. Checks if the next stage scene exists in Build Settings.
    /// If the next scene exists, it loads it. If not, it assumes all levels are completed and calls
    /// <see cref="AllLevelsCompleted"/>.
    public void NextLevel()
    {
        Debug.Log($"GameManager: NextLevel called. Initial check: World: {world}, Stage: {stage}");

        int nextStageAttempt = stage + 1;
        string nextSceneName = $"{world}-{nextStageAttempt}";

        Debug.Log($"GameManager: NextLevel - Attempting to find scene: {nextSceneName}");

        bool nextSceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string nameOnly = Path.GetFileNameWithoutExtension(path);

            if (nameOnly == nextSceneName)
            {
                nextSceneExists = true;
                break;
            }
        }

        if (nextSceneExists)
        {
            stage = nextStageAttempt; // Increment stage only if next scene exists
            Debug.Log($"GameManager: NextLevel - Advancing to next level. New stage will be {stage}.");
            LoadLevel(world, stage);
        }
        else
        {
            Debug.Log($"GameManager: NextLevel - Scene '{nextSceneName}' (next level) not found. Current stage: {stage}. Assuming all levels completed.");
            AllLevelsCompleted(); // All levels completed if next scene does not exist
        }
    }

    // --- Player Stat Management ---

    /// Adds a coin to the player's count. If 100 coins are collected, a life is added.
    /// Updates the UI display for coins.
    public void AddCoin()
    {
        coins++;
        Debug.Log($"GameManager: Coin collected. Total coins: {coins}. Current Stage: {stage}");

        if (coins >= 100)
        {
            coins -= 100;
            AddLife();
            Debug.Log("GameManager: 100 coins collected! Coins adjusted, life added.");
        }
        UpdateCoinText();
    }

    /// Adds one life to the player's current life count, up to <see cref="maxLives"/>.
    /// Updates the UI display for lives and manages the life regeneration timer.
    public void AddLife()
    {
        if (lives < maxLives)
        {
            lives++;
            PlayerPrefs.SetInt("Lives", lives); // Save lives
            PlayerPrefs.Save();
            Debug.Log($"GameManager: Life added. Total lives: {lives}. Current Stage: {stage}");

            // If max lives reached, stop the regeneration timer. Otherwise, reset it.
            if (lives >= maxLives)
            {
                nextLiveRegenTime = 0f;
                PlayerPrefs.SetFloat("NextLiveRegenTime", nextLiveRegenTime);
                PlayerPrefs.Save();
                Debug.Log("GameManager: Reached max lives, regeneration timer stopped.");
            }
        }
        else
        {
            Debug.Log("GameManager: Already at max lives, cannot add more.");
        }
        UpdateLivesText();
    }

    // --- UI Update Helper Methods ---

    /// Updates the UI Text element to display the current coin count.
    /// Logs a warning if the `coinCounterText` reference is null.
    private void UpdateCoinText()
    {
        if (coinCounterText != null)
        {
            coinCounterText.text = coins.ToString();
        }
        else
        {
            Debug.LogWarning("GameManager: coinCounterText is NOT assigned or found. Cannot update UI.");
        }
    }

    /// Updates the UI Text element to display the current lives count.
    /// Logs a warning if the `livesCounterText` reference is null.
    private void UpdateLivesText()
    {
        if (livesCounterText != null)
        {
            livesCounterText.text = lives.ToString();
        }
        else
        {
            Debug.LogWarning("GameManager: livesCounterText is NOT assigned or found. Cannot update UI.");
        }
    }

    // --- Public methods for UI Buttons (assign these to Button.onClick() events) ---

    /// Handles the "Start Game" button click event. Initiates a new game.
    public void OnStartGameButtonClicked()
    {
        Debug.Log("GameManager: 'Start Game' button clicked.");
        StartGame();
    }

    /// Handles the "Play Again" button click event (from Game Over or Level Complete screens).
    /// Resets coins and reloads the current level. Prevents replaying if no lives.
    public void OnPlayAgainCurrentLevelButtonClicked()
    {
        Debug.Log($"GameManager: 'Play Again Current Level' button clicked. Initiating replay of World: {world}, Stage: {stage}");

        if (lives <= 0 && gameOverCanvas != null && gameOverCanvas.activeSelf)
        {
            Debug.LogWarning("GameManager: Cannot play again, no lives remaining. Stay on Game Over screen or go to menu.");
            return;
        }

        coins = 0; // Reset coins for the new attempt on the current level
        LoadLevel(world, stage); // Reload the current level
    }

    /// Handles the "Next Level" button click event (from Level Complete screen).
    /// Calls the internal <see cref="NextLevel"/> logic.
    public void OnNextLevelButtonClicked()
    {
        Debug.Log("GameManager: 'Next Level' button clicked.");
        NextLevel();
    }

    /// Handles the "Menu" button click event. Resets game stats and loads the Main Menu scene.
    public void OnMenuButtonClicked()
    {
        Debug.Log("GameManager: 'Menu' button clicked. Resetting stats and loading MainMenu scene.");
        ResetGameStats();
        SceneManager.LoadScene(mainMenuSceneName);
        IsGameActive = false; // Game is not active in menu
    }

    /// Handles the "Exit" button click event. Quits the application.
    /// In the Unity Editor, it stops play mode.
    public void OnExitButtonClicked()
    {
        Debug.Log("GameManager: 'Exit' button clicked. Quitting application.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in Editor
#endif
    }
}
