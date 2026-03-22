using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    public bool gameIsPaused = false;
    public bool gameIsOver = false;
    
    [Header("World Progress")]
    [SerializeField] private List<string> worldNames = new List<string>
    {
        "Hellas",      // Greece
        "Hispania",    // Spain
        "Pindorama",   // Brazil
        "Kemet",       // Egypt
        "Jambudweep",  // India
        "Zhongguo",    // China
        "Wano"         // Japan
    };
    
    [SerializeField] private Dictionary<string, bool> worldsLiberated = new Dictionary<string, bool>();
    [SerializeField] private Dictionary<string, bool> commandersDefeated = new Dictionary<string, bool>();
    [SerializeField] private Dictionary<string, bool> guardiansRescued = new Dictionary<string, bool>();
    
    private string currentWorld;
    private int score = 0;
    
    [Header("Festival System")]
    [SerializeField] private bool festivalActive = false;
    
    private void Awake()
    {
        // Singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize world dictionaries
            InitializeWorldTracking();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeWorldTracking()
    {
        foreach (string world in worldNames)
        {
            worldsLiberated[world] = false;
            commandersDefeated[world] = false;
            guardiansRescued[world] = false;
        }
    }
    
    public void LoadWorld(string worldName)
    {
        currentWorld = worldName;
        SceneManager.LoadScene(worldName);
    }
    
    public void DefeatCommander(string worldName)
    {
        if (worldNames.Contains(worldName))
        {
            commandersDefeated[worldName] = true;
            score += 1000;
            
            // Check if guardian is also rescued
            if (guardiansRescued[worldName])
            {
                LiberateWorld(worldName);
            }
        }
    }
    
    public void RescueGuardian(string worldName)
    {
        if (worldNames.Contains(worldName))
        {
            guardiansRescued[worldName] = true;
            score += 500;
            
            // Check if commander is also defeated
            if (commandersDefeated[worldName])
            {
                LiberateWorld(worldName);
            }
        }
    }
    
    private void LiberateWorld(string worldName)
    {
        worldsLiberated[worldName] = true;
        score += 2000;
        
        // Start festival
        StartFestival(worldName);
        
        // Check if all worlds are liberated
        CheckGameProgress();
    }
    
    public void StartFestival(string worldName)
    {
        festivalActive = true;
        
        // Play celebration music
        AudioManager.Instance?.PlayFestivalMusic(worldName);
        
        // Change environment visuals
        EnvironmentManager.Instance?.ActivateFestival(worldName);
        
        // TODO: Add more festival effects
        Debug.Log($"Festival has started in {worldName}!");
    }
    
    private void CheckGameProgress()
    {
        bool allWorldsLiberated = true;
        
        foreach (string world in worldNames)
        {
            if (!worldsLiberated[world])
            {
                allWorldsLiberated = false;
                break;
            }
        }
        
        if (allWorldsLiberated)
        {
            // Unlock final level
            UnlockFinalBattle();
        }
    }
    
    private void UnlockFinalBattle()
    {
        Debug.Log("All worlds liberated! Final battle against Chyronis unlocked!");
        // TODO: Show notification, unlock final level
    }
    
    public void StartFinalBattle()
    {
        SceneManager.LoadScene("Kruvija");
    }
    
    public void GameOver()
    {
        gameIsOver = true;
        
        // Show game over screen
        UIManager.Instance?.ShowGameOverScreen();
        
        // Stop gameplay
        Time.timeScale = 0;
    }
    
    public void Victory()
    {
        // Show victory screen
        UIManager.Instance?.ShowVictoryScreen();
        
        // Calculate final score
        int finalScore = score;
        
        // Save game stats
        SaveGameStats(finalScore);
    }
    
    private void SaveGameStats(int finalScore)
    {
        // TODO: Implement save system
        Debug.Log($"Game completed with score: {finalScore}");
    }
    
    public void PauseGame()
    {
        gameIsPaused = true;
        Time.timeScale = 0;
        
        // Show pause menu
        UIManager.Instance?.ShowPauseMenu();
    }
    
    public void ResumeGame()
    {
        gameIsPaused = false;
        Time.timeScale = 1;
        
        // Hide pause menu
        UIManager.Instance?.HidePauseMenu();
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }
    
    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    // World status checking methods
    public bool IsWorldLiberated(string worldName)
    {
        return worldsLiberated.ContainsKey(worldName) && worldsLiberated[worldName];
    }
    
    public bool IsCommanderDefeated(string worldName)
    {
        return commandersDefeated.ContainsKey(worldName) && commandersDefeated[worldName];
    }
    
    public bool IsGuardianRescued(string worldName)
    {
        return guardiansRescued.ContainsKey(worldName) && guardiansRescued[worldName];
    }
    
    public int GetLiberatedWorldCount()
    {
        int count = 0;
        foreach (bool liberated in worldsLiberated.Values)
        {
            if (liberated) count++;
        }
        return count;
    }
    
    public int GetScore()
    {
        return score;
    }
    
    public void AddScore(int points)
    {
        score += points;
        UIManager.Instance?.UpdateScoreDisplay(score);
    }
} 
