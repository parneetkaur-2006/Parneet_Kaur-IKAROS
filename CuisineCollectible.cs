using UnityEngine;
using System.Collections;

public class CuisineCollectible : MonoBehaviour
{
    [Header("Cuisine Info")]
    [SerializeField] private string cuisineName;
    [SerializeField] private string worldOrigin; // Which world this cuisine is from
    [SerializeField] private Sprite cuisineSprite;
    [SerializeField] private string description;
    
    [Header("Effects")]
    [SerializeField] private CuisineEffectType effectType;
    [SerializeField] private int healAmount = 0;
    [SerializeField] private float buffAmount = 0f;
    [SerializeField] private float buffDuration = 0f;
    
    [Header("Collection")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private int scoreValue = 50;
    
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    
    public enum CuisineEffectType
    {
        Heal,
        SpeedBuff,
        AttackBuff,
        DefenseBuff,
        JumpBuff,
        MultiEffect
    }
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        
        // Set sprite if provided
        if (cuisineSprite != null)
        {
            spriteRenderer.sprite = cuisineSprite;
        }
    }
    
    private void Start()
    {
        // Floating animation
        StartCoroutine(FloatingAnimation());
    }
    
    private IEnumerator FloatingAnimation()
    {
        Vector3 startPosition = transform.position;
        float time = 0f;
        
        while (true)
        {
            // Simple up and down floating motion
            time += Time.deltaTime;
            float yOffset = Mathf.Sin(time * 2f) * 0.15f;
            transform.position = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);
            
            yield return null;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Apply effect to player
            PlayerController player = collision.GetComponent<PlayerController>();
            
            if (player != null)
            {
                ApplyEffect(player);
            }
            
            // Play collection effects
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }
            
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
            
            // Add to score
            GameManager.Instance?.AddScore(scoreValue);
            
            // Register cuisine collection with the GameManager or InventorySystem
            InventoryManager.Instance?.AddCuisine(this);
            
            // Deactivate visuals but keep script active for buff duration
            if (effectType != CuisineEffectType.Heal && buffDuration > 0)
            {
                spriteRenderer.enabled = false;
                circleCollider.enabled = false;
                
                // Destroy after buff duration
                Destroy(gameObject, buffDuration);
            }
            else
            {
                // Destroy immediately for instant effects like healing
                Destroy(gameObject);
            }
        }
    }
    
    private void ApplyEffect(PlayerController player)
    {
        switch (effectType)
        {
            case CuisineEffectType.Heal:
                // Apply healing
                player.Heal(healAmount);
                
                // Show healing effect
                UIManager.Instance?.ShowHealingEffect();
                break;
                
            case CuisineEffectType.SpeedBuff:
                // Apply speed buff
                StartCoroutine(ApplySpeedBuff(player));
                break;
                
            case CuisineEffectType.AttackBuff:
                // Apply attack buff
                StartCoroutine(ApplyAttackBuff(player));
                break;
                
            case CuisineEffectType.DefenseBuff:
                // Apply defense buff
                StartCoroutine(ApplyDefenseBuff(player));
                break;
                
            case CuisineEffectType.JumpBuff:
                // Apply jump buff
                StartCoroutine(ApplyJumpBuff(player));
                break;
                
            case CuisineEffectType.MultiEffect:
                // Apply multiple effects
                player.Heal(healAmount);
                StartCoroutine(ApplyMultiBuff(player));
                break;
        }
        
        // Display buff information
        UIManager.Instance?.ShowBuffNotification(cuisineName, description, buffDuration);
    }
    
    private IEnumerator ApplySpeedBuff(PlayerController player)
    {
        // Apply speed buff to player
        player.AddSpeedBuff(buffAmount);
        
        // Show buff effect
        UIManager.Instance?.ShowBuffEffect(CuisineEffectType.SpeedBuff);
        
        // Wait for duration
        yield return new WaitForSeconds(buffDuration);
        
        // Remove buff
        player.RemoveSpeedBuff(buffAmount);
    }
    
    private IEnumerator ApplyAttackBuff(PlayerController player)
    {
        // Apply attack buff to player
        player.AddAttackBuff(Mathf.RoundToInt(buffAmount));
        
        // Show buff effect
        UIManager.Instance?.ShowBuffEffect(CuisineEffectType.AttackBuff);
        
        // Wait for duration
        yield return new WaitForSeconds(buffDuration);
        
        // Remove buff
        player.RemoveAttackBuff(Mathf.RoundToInt(buffAmount));
    }
    
    private IEnumerator ApplyDefenseBuff(PlayerController player)
    {
        // Apply defense buff to player
        player.AddDefenseBuff(buffAmount);
        
        // Show buff effect
        UIManager.Instance?.ShowBuffEffect(CuisineEffectType.DefenseBuff);
        
        // Wait for duration
        yield return new WaitForSeconds(buffDuration);
        
        // Remove buff
        player.RemoveDefenseBuff(buffAmount);
    }
    
    private IEnumerator ApplyJumpBuff(PlayerController player)
    {
        // Apply jump buff to player
        player.AddJumpBuff(buffAmount);
        
        // Show buff effect
        UIManager.Instance?.ShowBuffEffect(CuisineEffectType.JumpBuff);
        
        // Wait for duration
        yield return new WaitForSeconds(buffDuration);
        
        // Remove buff
        player.RemoveJumpBuff(buffAmount);
    }
    
    private IEnumerator ApplyMultiBuff(PlayerController player)
    {
        // Apply multiple buffs
        player.AddSpeedBuff(buffAmount * 0.5f);
        player.AddAttackBuff(Mathf.RoundToInt(buffAmount * 0.3f));
        player.AddDefenseBuff(buffAmount * 0.4f);
        
        // Show buff effect
        UIManager.Instance?.ShowBuffEffect(CuisineEffectType.MultiEffect);
        
        // Wait for duration
        yield return new WaitForSeconds(buffDuration);
        
        // Remove buffs
        player.RemoveSpeedBuff(buffAmount * 0.5f);
        player.RemoveAttackBuff(Mathf.RoundToInt(buffAmount * 0.3f));
        player.RemoveDefenseBuff(buffAmount * 0.4f);
    }
    
    // Getter methods for the inventory system
    public string GetCuisineName() => cuisineName;
    public string GetWorldOrigin() => worldOrigin;
    public Sprite GetCuisineSprite() => cuisineSprite;
    public string GetDescription() => description;
    public CuisineEffectType GetEffectType() => effectType;
    public int GetHealAmount() => healAmount;
    public float GetBuffAmount() => buffAmount;
    public float GetBuffDuration() => buffDuration;
} 
