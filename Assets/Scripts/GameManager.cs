using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 
using UnityEngine.InputSystem; 

[DefaultExecutionOrder(-100)] 
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scoring System")]
    [SerializeField] public int currentEXP;
    public TextMeshProUGUI expTextUI; 

    [Header("Timer System")]
    public TextMeshProUGUI timerTextUI;
    public float currentTime { get; private set; }
    public bool isTimerRunning = true;

    [Header("Difficulty & Scaling")]
    public float globalDifficultyMultiplier = 1f;
    public int maxEnemies = 1;
    public int activeEnemyCount = 0;
    private float enemyRespawnTimer = 0f; 

    [Header("Spawning System - EXP")]
    public GameObject expShardPrefab; 

    [Header("Spawning System - ENEMIES")]
    public GameObject[] enemyPrefabs; 

    [Header("Arena Setup")]
    public string pillarTag = "ArenaPillar"; 
    public Material redArenaMaterial; // --- NEW: Slot for M_neonRed ---
    private bool hasArenaChangedColor = false;
    
    [Header("SFX (Audio)")]
    public AudioClip expCollectClip;
    private AudioSource audioSource;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalStatsText;
    private bool isGameOver = false;

    [Header("Pause Menu UI")]
    public GameObject pausePanel;
    private bool isPaused = false;

    private GameObject[] allPillars;
    private Transform playerTransform;
    private GameObject currentActiveShard;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        
        audioSource = gameObject.AddComponent<AudioSource>();
        Time.timeScale = 1f; 
    }

    private void Start()
    {
        currentEXP = 0;
        currentTime = 0f;
        activeEnemyCount = 0;
        enemyRespawnTimer = 0f;
        isGameOver = false;
        isPaused = false;
        hasArenaChangedColor = false;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        
        UpdateEXPUI();
        allPillars = GameObject.FindGameObjectsWithTag(pillarTag);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        SpawnSingleShard(); 
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isGameOver) 
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }

        if (isTimerRunning && !isGameOver && !isPaused)
        {
            currentTime += Time.deltaTime;
            UpdateTimerUI();
            
            // --- UPDATED TIMELINE ---
            if (currentTime >= 420f) // 7 Minutes (420 seconds)
            {
                maxEnemies = 4;
                globalDifficultyMultiplier = 1f + ((currentTime - 420f) / 600f); 
                
                // Trigger the visual shift once
                if (!hasArenaChangedColor) TriggerRedArena();
            }
            else if (currentTime >= 300f) // 5 Minutes
            {
                maxEnemies = 4;
                globalDifficultyMultiplier = 1f; 
            }
            else if (currentTime >= 240f) // 4 Minutes
            {
                maxEnemies = 3;
                globalDifficultyMultiplier = 1f; 
            }
            else if (currentTime >= 90f)  // 1 Minute 30 Seconds
            {
                maxEnemies = 2;
                globalDifficultyMultiplier = 1f; 
            }
            else                          // Start of game
            {
                maxEnemies = 1;
                globalDifficultyMultiplier = 1f; 
            }

            if (activeEnemyCount < maxEnemies)
            {
                enemyRespawnTimer += Time.deltaTime;
                if (enemyRespawnTimer >= 2f)
                {
                    SpawnSingleEnemy();
                    enemyRespawnTimer = 0f; 
                }
            }
            else
            {
                enemyRespawnTimer = 0f;
            }
        }
    }


    private void TriggerRedArena()
    {
        hasArenaChangedColor = true;

        if (redArenaMaterial == null || allPillars == null) return;

        foreach (GameObject pillar in allPillars)
        {
            // Search all child objects inside this specific pillar
            Renderer[] childRenderers = pillar.GetComponentsInChildren<Renderer>();
            
            foreach (Renderer rend in childRenderers)
            {
                // Only change the material if the object is exactly named "Pillar low"
                if (rend.gameObject.name == "Pillar low")
                {
                    rend.material = redArenaMaterial;
                    break; // Stop searching this pillar and move to the next one
                }
            }
        }
    }

    public void AddScore(int amount)
    {
        if (isGameOver || isPaused) return;

        if (amount > 0) 
        {
            SpawnSingleShard();
            if (expCollectClip != null && audioSource != null) audioSource.PlayOneShot(expCollectClip);
        }
        
        currentEXP += amount;

        if (currentEXP <= 0) 
        {
            currentEXP = 0;
            if (amount < 0) 
            {
                TriggerGameOver();
            }
        }
        
        UpdateEXPUI();
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        isTimerRunning = false;
        Time.timeScale = 0f; 

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (finalStatsText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60F);
            int seconds = Mathf.FloorToInt(currentTime % 60F);
            int milliseconds = Mathf.FloorToInt((currentTime * 100F) % 100F);
            string timeString = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
            finalStatsText.text = $"SURVIVAL TIME: {timeString}";
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void LoadHomeMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu"); 
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Application Quitting...");
        Application.Quit();
    }

    private void UpdateEXPUI()
    {
        if (expTextUI != null) expTextUI.text = $"EXP: {currentEXP}"; 
    }

    private void UpdateTimerUI()
    {
        if (timerTextUI != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60F);
            int seconds = Mathf.FloorToInt(currentTime % 60F);
            int milliseconds = Mathf.FloorToInt((currentTime * 100F) % 100F);
            timerTextUI.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
    }

    private void SpawnSingleShard()
    {
        if (expShardPrefab == null || allPillars == null || allPillars.Length == 0) return;
        
        int randomIndex = Random.Range(0, allPillars.Length);
        Collider pillarCollider = allPillars[randomIndex].GetComponent<Collider>();
        if (pillarCollider == null) return; 

        Vector3 spawnPos = new Vector3(pillarCollider.bounds.center.x, pillarCollider.bounds.max.y + 0.5f, pillarCollider.bounds.center.z);
        currentActiveShard = Instantiate(expShardPrefab, spawnPos, Quaternion.identity, transform);
    }

    private void SpawnSingleEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || allPillars == null || allPillars.Length == 0) return;
        activeEnemyCount++; 
        
        int randomPrefabIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedEnemyPrefab = enemyPrefabs[randomPrefabIndex];

        if (playerTransform != null && currentActiveShard != null)
        {
            Vector3 midPoint = (playerTransform.position + currentActiveShard.transform.position) / 2f;
            GameObject closestPillar = null;
            float closestDistance = float.MaxValue;

            foreach (GameObject pillar in allPillars)
            {
                float dist = Vector3.Distance(pillar.transform.position, midPoint);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPillar = pillar;
                }
            }

            if (closestPillar != null && closestPillar.TryGetComponent<Collider>(out Collider pillarCollider))
            {
                Vector3 spawnPos = new Vector3(pillarCollider.bounds.center.x, pillarCollider.bounds.max.y + 1.0f, pillarCollider.bounds.center.z);
                Instantiate(selectedEnemyPrefab, spawnPos, Quaternion.identity, transform);
                return; 
            }
        }
        
        int randomIndex = Random.Range(0, allPillars.Length);
        Collider backupCollider = allPillars[randomIndex].GetComponent<Collider>();
        if (backupCollider != null) 
        {
            Vector3 spawnPos = new Vector3(backupCollider.bounds.center.x, backupCollider.bounds.max.y + 1.0f, backupCollider.bounds.center.z);
            Instantiate(selectedEnemyPrefab, spawnPos, Quaternion.identity, transform);
        }
    }
}