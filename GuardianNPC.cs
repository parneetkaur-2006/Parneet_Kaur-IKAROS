using UnityEngine;
using System.Collections;

public class GuardianNPC : MonoBehaviour
{
    [Header("Guardian Info")]
    [SerializeField] private string guardianName;
    [SerializeField] private string worldName; // Which world this guardian belongs to
    [SerializeField] private string festivalName;
    
    [Header("State")]
    [SerializeField] private bool isImprisoned = true;
    [SerializeField] private GameObject prisonCage;
    [SerializeField] private GameObject magicBarrier;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem freedomEffect;
    [SerializeField] private Light auraLight;
    [SerializeField] private Material imprisonedMaterial;
    [SerializeField] private Material freedMaterial;
    
    [Header("Audio")]
    [SerializeField] private AudioClip imprisonedVoice;
    [SerializeField] private AudioClip freedVoice;
    [SerializeField] private AudioClip rescueSound;
    
    [Header("Dialogue")]
    [SerializeField] private string[] imprisonedDialogue;
    [SerializeField] private string[] freedDialogue;
    
    [Header("Special Power")]
    [SerializeField] private GuardianPower guardianPower;
    [SerializeField] private float powerCooldown = 30f;
    [SerializeField] private ParticleSystem powerEffect;
    private float powerTimer = 0f;
    
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool playerInRange = false;
    private Transform player;
    
    public enum GuardianPower
    {
        Healing,
        DamageShield,
        AttackBoost,
        TimeFreeze,
        AreaAttack,
        ElementalBurst
    }
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Initialize based on game state
        if (GameManager.Instance != null && GameManager.Instance.IsGuardianRescued(worldName))
        {
            isImprisoned = false;
            SetFreedVisuals();
        }
        else
        {
            SetImprisonedVisuals();
        }
    }
    
    private void Update()
    {
        // Update power cooldown
        if (!isImprisoned && powerTimer > 0)
        {
            powerTimer -= Time.deltaTime;
        }
        
        // Check for player interaction
        if (playerInRange && Input.GetButtonDown("Interact"))
        {
            if (isImprisoned)
            {
                Rescue();
            }
            else
            {
                ShowDialogue();
                
                // If power is available, offer to use it
                if (powerTimer <= 0)
                {
                    OfferGuardianPower();
                }
            }
        }
        
        // Make liberated guardian look at player
        if (!isImprisoned && playerInRange && player != null)
        {
            LookAtPlayer();
        }
    }
    
    private void SetImprisonedVisuals()
    {
        // Set imprisoned animations
        if (animator != null)
        {
            animator.SetBool("imprisoned", true);
        }
        
        // Enable prison objects
        if (prisonCage != null)
        {
            prisonCage.SetActive(true);
        }
        
        if (magicBarrier != null)
        {
            magicBarrier.SetActive(true);
        }
        
        // Set darker material
        if (spriteRenderer != null && imprisonedMaterial != null)
        {
            spriteRenderer.material = imprisonedMaterial;
        }
        
        // Dim aura light
        if (auraLight != null)
        {
            auraLight.intensity = 0.3f;
            auraLight.range = 2f;
        }
    }
    
    private void SetFreedVisuals()
    {
        // Set freed animations
        if (animator != null)
        {
            animator.SetBool("imprisoned", false);
        }
        
        // Disable prison objects
        if (prisonCage != null)
        {
            prisonCage.SetActive(false);
        }
        
        if (magicBarrier != null)
        {
            magicBarrier.SetActive(false);
        }
        
        // Set glowing material
        if (spriteRenderer != null && freedMaterial != null)
        {
            spriteRenderer.material = freedMaterial;
        }
        
        // Brighten aura light
        if (auraLight != null)
        {
            auraLight.intensity = 1.2f;
            auraLight.range = 5f;
        }
    }
    
    private void Rescue()
    {
        isImprisoned = false;
        
        // Play rescue animation
        if (animator != null)
        {
            animator.SetTrigger("rescue");
        }
        
        // Play effects
        if (freedomEffect != null)
        {
            freedomEffect.Play();
        }
        
        // Play audio
        if (rescueSound != null)
        {
            AudioSource.PlayClipAtPoint(rescueSound, transform.position);
        }
        
        // Play freed voice after a delay
        if (freedVoice != null && audioSource != null)
        {
            StartCoroutine(PlayDelayedVoice(freedVoice, 1.5f));
        }
        
        // Update visuals
        SetFreedVisuals();
        
        // Update game state
        GameManager.Instance?.RescueGuardian(worldName);
        
        // Show dialogue
        ShowDialogue();
    }
    
    private IEnumerator PlayDelayedVoice(AudioClip voice, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.clip = voice;
        audioSource.Play();
    }
    
    private void ShowDialogue()
    {
        string[] currentDialogue = isImprisoned ? imprisonedDialogue : freedDialogue;
        
        if (currentDialogue.Length > 0)
        {
            int randomIndex = Random.Range(0, currentDialogue.Length);
            string dialogue = currentDialogue[randomIndex];
            
            // Show dialogue UI
            UIManager.Instance?.ShowDialogue(guardianName, dialogue);
        }
    }
    
    private void OfferGuardianPower()
    {
        // Show power option UI
        UIManager.Instance?.ShowGuardianPowerOption(guardianName, guardianPower.ToString());
        
        // Listen for player confirmation
        // This would be handled by the UI system calling back to UseGuardianPower()
    }
    
    public void UseGuardianPower()
    {
        if (!isImprisoned && powerTimer <= 0)
        {
            // Play power animation
            if (animator != null)
            {
                animator.SetTrigger("usePower");
            }
            
            // Play power effect
            if (powerEffect != null)
            {
                powerEffect.Play();
            }
            
            // Apply power effect based on type
            switch (guardianPower)
            {
                case GuardianPower.Healing:
                    ApplyHealingPower();
                    break;
                case GuardianPower.DamageShield:
                    ApplyShieldPower();
                    break;
                case GuardianPower.AttackBoost:
                    ApplyAttackBoostPower();
                    break;
                case GuardianPower.TimeFreeze:
                    ApplyTimeFreezePower();
                    break;
                case GuardianPower.AreaAttack:
                    ApplyAreaAttackPower();
                    break;
                case GuardianPower.ElementalBurst:
                    ApplyElementalBurstPower();
                    break;
            }
            
            // Set cooldown
            powerTimer = powerCooldown;
        }
    }
    
    private void ApplyHealingPower()
    {
        // Heal player to full health
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.Heal(playerController.GetMaxHealth());
            }
        }
    }
    
    private void ApplyShieldPower()
    {
        // Create a damage shield for player
        if (player != null)
        {
            StartCoroutine(DamageShieldEffect());
        }
    }
    
    private IEnumerator DamageShieldEffect()
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Make player temporarily invincible
            playerController.MakeInvincible(true);
            
            // Create shield visual effect around player
            GameObject shield = new GameObject("GuardianShield");
            shield.transform.position = player.position;
            shield.transform.parent = player;
            
            // Add a visual component to the shield
            SpriteRenderer shieldRenderer = shield.AddComponent<SpriteRenderer>();
            shieldRenderer.sprite = Resources.Load<Sprite>("Shield");
            shieldRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            
            // Maintain shield for 10 seconds
            yield return new WaitForSeconds(10f);
            
            // Remove shield
            Destroy(shield);
            playerController.MakeInvincible(false);
        }
    }
    
    private void ApplyAttackBoostPower()
    {
        // Boost player attack power
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Add significant attack boost for 20 seconds
                playerController.AddAttackBuff(50);
                StartCoroutine(RemoveAttackBoost(playerController));
            }
        }
    }
    
    private IEnumerator RemoveAttackBoost(PlayerController playerController)
    {
        yield return new WaitForSeconds(20f);
        playerController.RemoveAttackBuff(50);
    }
    
    private void ApplyTimeFreezePower()
    {
        // Freeze time for enemies
        StartCoroutine(FreezeTimeEffect());
    }
    
    private IEnumerator FreezeTimeEffect()
    {
        // Find all enemies in the scene
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        CommanderController[] commanders = FindObjectsOfType<CommanderController>();
        
        // Freeze their movement and actions
        foreach (EnemyController enemy in enemies)
        {
            enemy.enabled = false;
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }
        }
        
        foreach (CommanderController commander in commanders)
        {
            commander.enabled = false;
            Rigidbody2D rb = commander.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }
        }
        
        // Apply visual time freeze effect
        // This could be a post-processing effect or shader
        
        // Wait for duration
        yield return new WaitForSeconds(8f);
        
        // Unfreeze everything
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null) // Check if they haven't been destroyed
            {
                enemy.enabled = true;
                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
            }
        }
        
        foreach (CommanderController commander in commanders)
        {
            if (commander != null)
            {
                commander.enabled = true;
                Rigidbody2D rb = commander.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
            }
        }
    }
    
    private void ApplyAreaAttackPower()
    {
        // Damage all enemies in a large area
        
        // Find all enemies within range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 15f, LayerMask.GetMask("Enemy"));
        
        // Apply massive damage to each
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyController enemy = enemyCollider.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(100);
            }
            
            CommanderController commander = enemyCollider.GetComponent<CommanderController>();
            if (commander != null)
            {
                commander.TakeDamage(100);
            }
        }
        
        // Visual effect for area attack
        // Could be an expanding shockwave or explosion
    }
    
    private void ApplyElementalBurstPower()
    {
        // Apply elemental effect to player's weapon
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Add both attack and speed buffs
                playerController.AddAttackBuff(25);
                playerController.AddSpeedBuff(0.5f);
                
                // Add visual effect to player's weapon
                // This would be better with a weapon system
                
                StartCoroutine(RemoveElementalEffect(playerController));
            }
        }
    }
    
    private IEnumerator RemoveElementalEffect(PlayerController playerController)
    {
        yield return new WaitForSeconds(15f);
        
        playerController.RemoveAttackBuff(25);
        playerController.RemoveSpeedBuff(0.5f);
    }
    
    private void LookAtPlayer()
    {
        // Simple 2D look at - just flip sprite based on relative position
        bool shouldFaceRight = player.position.x > transform.position.x;
        
        // Check if we need to flip
        if ((shouldFaceRight && transform.localScale.x < 0) ||
            (!shouldFaceRight && transform.localScale.x > 0))
        {
            // Flip the local scale to face the right direction
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            player = collision.transform;
            
            // Show interaction prompt
            UIManager.Instance?.ShowInteractionPrompt(isImprisoned ? "Rescue " + guardianName : "Talk to " + guardianName);
            
            // Play appropriate voice clip
            if (audioSource != null)
            {
                audioSource.clip = isImprisoned ? imprisonedVoice : freedVoice;
                audioSource.Play();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Hide interaction prompt
            UIManager.Instance?.HideInteractionPrompt();
            
            // Stop voice
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
} 
