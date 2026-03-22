using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FestivalManager : MonoBehaviour
{
    // Singleton pattern
    public static FestivalManager Instance { get; private set; }
    
    [Header("Festival States")]
    [SerializeField] private List<WorldFestival> worldFestivals = new List<WorldFestival>();
    [SerializeField] private bool transitioningFestival = false;
    
    [Header("Visual Effects")]
    [SerializeField] private float transitionDuration = 2f;
    [SerializeField] private ParticleSystem festivalParticles;
    [SerializeField] private Light festivalLight;
    
    // Current active world
    private string currentWorld;
    
    [System.Serializable]
    public class WorldFestival
    {
        public string worldName;
        public string festivalName;
        
        [Header("Before Liberation")]
        public GameObject darkEnvironment;
        public GameObject suppressedNPCs;
        public Material darkSkyboxMaterial;
        public Color darkAmbientLight = new Color(0.2f, 0.2f, 0.25f);
        
        [Header("After Liberation")]
        public GameObject festivalEnvironment;
        public GameObject celebratingNPCs;
        public Material festivalSkyboxMaterial;
        public Color festivalAmbientLight = new Color(0.8f, 0.8f, 1f);
        
        [Header("Audio")]
        public AudioClip beforeMusic;
        public AudioClip afterMusic;
        public AudioClip transitionSound;
        public List<AudioClip> festivalSounds;
        
        [HideInInspector]
        public bool isLiberated = false;
    }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Get current world from scene name or GameManager
        currentWorld = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Check if this world's festival is already liberated
        InitializeWorldState();
    }
    
    private void InitializeWorldState()
    {
        foreach (WorldFestival festival in worldFestivals)
        {
            // Set initial state based on GameManager data
            if (GameManager.Instance != null && GameManager.Instance.IsWorldLiberated(festival.worldName))
            {
                festival.isLiberated = true;
                
                // Apply festive visuals immediately if this is the current world
                if (festival.worldName == currentWorld)
                {
                    ApplyFestivalState(festival, false); // No transition
                }
            }
            else
            {
                festival.isLiberated = false;
                
                // Apply dark visuals immediately if this is the current world
                if (festival.worldName == currentWorld)
                {
                    ApplySuppressedState(festival, false); // No transition
                }
            }
        }
    }
    
    public void StartFestival(string worldName)
    {
        if (transitioningFestival)
            return;
            
        // Find matching world festival
        WorldFestival festival = worldFestivals.Find(f => f.worldName == worldName);
        
        if (festival != null && !festival.isLiberated)
        {
            // Mark as liberated
            festival.isLiberated = true;
            
            // Start transition if this is the current world
            if (worldName == currentWorld)
            {
                StartCoroutine(TransitionToFestival(festival));
            }
        }
    }
    
    private IEnumerator TransitionToFestival(WorldFestival festival)
    {
        transitioningFestival = true;
        
        // Play transition sound
        if (festival.transitionSound != null)
        {
            AudioSource.PlayClipAtPoint(festival.transitionSound, Camera.main.transform.position);
        }
        
        // Start particles
        if (festivalParticles != null)
        {
            festivalParticles.Play();
        }
        
        // Fade out current music
        // AudioManager.Instance?.FadeOutMusic(1f);
        
        // Gradually change lighting
        Color startColor = RenderSettings.ambientLight;
        Color targetColor = festival.festivalAmbientLight;
        float startIntensity = 0f;
        float targetIntensity = 1f;
        
        if (festivalLight != null)
        {
            startIntensity = festivalLight.intensity;
            festivalLight.gameObject.SetActive(true);
        }
        
        // Gradual transition
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            
            // Smooth easing
            float smoothT = t * t * (3f - 2f * t);
            
            // Update lighting
            RenderSettings.ambientLight = Color.Lerp(startColor, targetColor, smoothT);
            
            if (festivalLight != null)
            {
                festivalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, smoothT);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Apply full festival state after transition
        ApplyFestivalState(festival, false);
        
        // Start festival music
        if (festival.afterMusic != null)
        {
            // AudioManager.Instance?.PlayMusic(festival.afterMusic, true);
        }
        
        // Schedule random festival sounds
        StartCoroutine(PlayRandomFestivalSounds(festival));
        
        transitioningFestival = false;
    }
    
    private void ApplySuppressedState(WorldFestival festival, bool withTransition)
    {
        // Set skybox
        if (festival.darkSkyboxMaterial != null)
        {
            RenderSettings.skybox = festival.darkSkyboxMaterial;
        }
        
        // Set lighting
        if (!withTransition)
        {
            RenderSettings.ambientLight = festival.darkAmbientLight;
        }
        
        // Enable/disable game objects
        if (festival.darkEnvironment != null)
        {
            festival.darkEnvironment.SetActive(true);
        }
        
        if (festival.festivalEnvironment != null)
        {
            festival.festivalEnvironment.SetActive(false);
        }
        
        if (festival.suppressedNPCs != null)
        {
            festival.suppressedNPCs.SetActive(true);
        }
        
        if (festival.celebratingNPCs != null)
        {
            festival.celebratingNPCs.SetActive(false);
        }
        
        // Play appropriate music
        if (festival.beforeMusic != null && !withTransition)
        {
            // AudioManager.Instance?.PlayMusic(festival.beforeMusic, true);
        }
    }
    
    private void ApplyFestivalState(WorldFestival festival, bool withTransition)
    {
        // Set skybox
        if (festival.festivalSkyboxMaterial != null)
        {
            RenderSettings.skybox = festival.festivalSkyboxMaterial;
        }
        
        // Set lighting
        if (!withTransition)
        {
            RenderSettings.ambientLight = festival.festivalAmbientLight;
        }
        
        // Enable/disable game objects
        if (festival.darkEnvironment != null)
        {
            festival.darkEnvironment.SetActive(false);
        }
        
        if (festival.festivalEnvironment != null)
        {
            festival.festivalEnvironment.SetActive(true);
        }
        
        if (festival.suppressedNPCs != null)
        {
            festival.suppressedNPCs.SetActive(false);
        }
        
        if (festival.celebratingNPCs != null)
        {
            festival.celebratingNPCs.SetActive(true);
        }
        
        // Play appropriate music
        if (festival.afterMusic != null && !withTransition)
        {
            // AudioManager.Instance?.PlayMusic(festival.afterMusic, true);
        }
    }
    
    private IEnumerator PlayRandomFestivalSounds(WorldFestival festival)
    {
        if (festival.festivalSounds == null || festival.festivalSounds.Count == 0)
            yield break;
            
        while (festival.isLiberated)
        {
            // Wait a random amount of time
            yield return new WaitForSeconds(Random.Range(5f, 15f));
            
            // Play a random festival sound
            int soundIndex = Random.Range(0, festival.festivalSounds.Count);
            AudioClip sound = festival.festivalSounds[soundIndex];
            
            if (sound != null)
            {
                // Get a random position within the scene for spatial audio
                Vector3 randomPos = Camera.main.transform.position + 
                    new Vector3(Random.Range(-10f, 10f), Random.Range(-5f, 5f), Random.Range(-10f, 10f));
                
                AudioSource.PlayClipAtPoint(sound, randomPos, 0.5f);
            }
        }
    }
    
    // Called when changing worlds
    public void SetCurrentWorld(string worldName)
    {
        currentWorld = worldName;
        
        // Initialize state for the new world
        WorldFestival festival = worldFestivals.Find(f => f.worldName == worldName);
        
        if (festival != null)
        {
            if (festival.isLiberated)
            {
                ApplyFestivalState(festival, false);
            }
            else
            {
                ApplySuppressedState(festival, false);
            }
        }
    }
    
    // Method to check if a world's festival is liberated
    public bool IsFestivalLiberated(string worldName)
    {
        WorldFestival festival = worldFestivals.Find(f => f.worldName == worldName);
        return festival != null && festival.isLiberated;
    }
} 
