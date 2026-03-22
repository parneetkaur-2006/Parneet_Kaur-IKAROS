using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommanderController : MonoBehaviour
{
    [Header("Commander Info")]
    [SerializeField] private string commanderName;
    [SerializeField] private string worldName; // Which world this commander belongs to
    
    [Header("Stats")]
    [SerializeField] private int maxHealth = 500;
    [SerializeField] private int currentHealth;
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("Attack Configuration")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int basicAttackDamage = 20;
    [SerializeField] private int specialAttackDamage = 40;
    
    [Header("Phase Control")]
    [SerializeField] private int phaseChangeHealthThreshold = 250; // 50% health
    [SerializeField] private bool isSecondPhase = false;
    [SerializeField] private ParticleSystem phaseChangeEffect;
    
    [Header("Special Abilities")]
    [SerializeField] private float specialAttackCooldown = 8f;
    [SerializeField] private GameObject specialAttackPrefab;
    private float specialAttackTimer;
    
    [Header("Attack Patterns")]
    [SerializeField] private List<AttackPattern> phase1Attacks = new List<AttackPattern>();
    [SerializeField] private List<AttackPattern> phase2Attacks = new List<AttackPattern>();
    private int currentAttackIndex = 0;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isFacingRight = true;
    private Transform player;
    private bool isAttacking = false;
    private bool isInvulnerable = false;
    
    [System.Serializable]
    public class AttackPattern
    {
        public string name;
        public int damage;
        public float range;
        public float cooldown;
        public string animationTrigger;
    }
    
    private enum CommanderState { Idle, Chase, Attack, SpecialAttack, Hurt, Death }
    private CommanderState currentState;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        currentState = CommanderState.Idle;
        specialAttackTimer = specialAttackCooldown;
        
        // Find player in scene
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private void Update()
    {
        if (currentState == CommanderState.Death)
            return;
            
        // Update special attack cooldown
        if (specialAttackTimer > 0)
        {
            specialAttackTimer -= Time.deltaTime;
        }
        
        // State machine
        switch (currentState)
        {
            case CommanderState.Idle:
                Idle();
                break;
            case CommanderState.Chase:
                ChasePlayer();
                break;
            case CommanderState.Attack:
                AttackPlayer();
                break;
            case CommanderState.SpecialAttack:
                // Special attack is handled within attack coroutines
                break;
        }
        
        // Update animations
        animator.SetInteger("state", (int)currentState);
    }
    
    private void Idle()
    {
        if (player == null)
            return;
            
        // Start chase if player is detected
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < 10f) // Detection range
        {
            currentState = CommanderState.Chase;
        }
    }
    
    private void ChasePlayer()
    {
        if (player == null)
        {
            currentState = CommanderState.Idle;
            return;
        }
        
        // Move towards player if not too close
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer > attackRange)
        {
            float direction = player.position.x - transform.position.x;
            rb.velocity = new Vector2(Mathf.Sign(direction) * moveSpeed, rb.velocity.y);
            
            // Flip based on direction
            if (direction > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (direction < 0 && isFacingRight)
            {
                Flip();
            }
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            
            // Choose between normal and special attack
            if (specialAttackTimer <= 0 && Random.value < 0.7f)
            {
                currentState = CommanderState.SpecialAttack;
                StartCoroutine(PerformSpecialAttack());
            }
            else
            {
                currentState = CommanderState.Attack;
            }
        }
    }
    
    private void AttackPlayer()
    {
        if (player == null || isAttacking)
        {
            if (!isAttacking)
                currentState = CommanderState.Idle;
            return;
        }
        
        // Face the player
        float direction = player.position.x - transform.position.x;
        if (direction > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (direction < 0 && isFacingRight)
        {
            Flip();
        }
        
        // Perform attack
        StartCoroutine(PerformAttack());
    }
    
    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        
        // Select attack pattern based on current phase
        List<AttackPattern> currentPhaseAttacks = isSecondPhase ? phase2Attacks : phase1Attacks;
        
        if (currentPhaseAttacks.Count == 0)
        {
            isAttacking = false;
            currentState = CommanderState.Chase;
            yield break;
        }
        
        // Cycle through attack patterns
        AttackPattern attackPattern = currentPhaseAttacks[currentAttackIndex];
        currentAttackIndex = (currentAttackIndex + 1) % currentPhaseAttacks.Count;
        
        // Trigger animation
        animator.SetTrigger(attackPattern.animationTrigger);
        
        // Wait for animation timing
        yield return new WaitForSeconds(0.3f); // Adjust based on animation
        
        // Deal damage if player is in range
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackPattern.range)
        {
            Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackPattern.range, playerLayer);
            if (hitPlayer != null)
            {
                hitPlayer.GetComponent<PlayerController>()?.TakeDamage(attackPattern.damage);
            }
        }
        
        // Cooldown
        yield return new WaitForSeconds(attackPattern.cooldown);
        
        isAttacking = false;
        currentState = CommanderState.Chase;
    }
    
    private IEnumerator PerformSpecialAttack()
    {
        isAttacking = true;
        
        // Play special attack animation
        animator.SetTrigger("specialAttack");
        
        // Wait for animation timing
        yield return new WaitForSeconds(0.5f);
        
        // Instantiate special attack effect/projectile
        if (specialAttackPrefab != null)
        {
            Instantiate(specialAttackPrefab, attackPoint.position, Quaternion.identity);
        }
        
        // Deal damage in a larger area
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange * 1.5f, playerLayer);
        foreach (Collider2D player in hitPlayers)
        {
            player.GetComponent<PlayerController>()?.TakeDamage(specialAttackDamage);
        }
        
        // Reset special attack timer
        specialAttackTimer = specialAttackCooldown;
        
        // Cooldown
        yield return new WaitForSeconds(1f);
        
        isAttacking = false;
        currentState = CommanderState.Chase;
    }
    
    public void TakeDamage(int damage)
    {
        if (currentState == CommanderState.Death || isInvulnerable)
            return;
            
        currentHealth -= damage;
        
        // Play hurt animation
        animator.SetTrigger("hurt");
        currentState = CommanderState.Hurt;
        
        // Check for phase change
        if (!isSecondPhase && currentHealth <= phaseChangeHealthThreshold)
        {
            StartCoroutine(ChangePhase());
        }
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Small knockback
            StartCoroutine(Knockback());
        }
    }
    
    private IEnumerator ChangePhase()
    {
        isSecondPhase = true;
        isInvulnerable = true;
        
        // Stop movement
        rb.velocity = Vector2.zero;
        
        // Play phase change animation/effect
        animator.SetTrigger("phaseChange");
        if (phaseChangeEffect != null)
        {
            phaseChangeEffect.Play();
        }
        
        // Wait for animation
        yield return new WaitForSeconds(2f);
        
        // Possibly heal slightly
        currentHealth += maxHealth / 10;
        
        // Increase stats for second phase
        moveSpeed *= 1.2f;
        
        isInvulnerable = false;
        currentState = CommanderState.Chase;
    }
    
    private IEnumerator Knockback()
    {
        // Apply small knockback
        Vector2 knockbackDirection = player != null ? 
            new Vector2(transform.position.x - player.position.x, 0).normalized : 
            new Vector2(isFacingRight ? -1 : 1, 0);
            
        rb.velocity = new Vector2(knockbackDirection.x * 2f, rb.velocity.y);
        
        yield return new WaitForSeconds(0.2f);
        
        // Return to chase state if not dead
        if (currentState != CommanderState.Death)
        {
            currentState = CommanderState.Chase;
        }
    }
    
    private void Die()
    {
        // Set death state
        currentState = CommanderState.Death;
        
        // Play death animation
        animator.SetTrigger("death");
        
        // Disable physics and collisions
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        
        // Register commander defeat with GameManager
        GameManager.Instance?.DefeatCommander(worldName);
        
        // Destroy after animation or spawn reward
        Destroy(gameObject, 5f);
    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
            
        // Draw basic attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        
        // Draw special attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange * 1.5f);
    }
} 
