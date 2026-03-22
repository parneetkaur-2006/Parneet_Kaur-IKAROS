using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float aggroRange = 5f;
    [SerializeField] private float moveSpeed = 2f;
    
    [Header("Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackRate = 1f;
    private float nextAttackTime = 0f;
    
    private int currentHealth;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private bool isFacingRight = true;
    
    private enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Death }
    private EnemyState currentState;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        currentState = EnemyState.Idle;
        
        // Find player in scene
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private void Update()
    {
        if (currentState == EnemyState.Death)
            return;
            
        // State machine
        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Attack:
                AttackPlayer();
                break;
        }
        
        // Set animation based on state
        animator.SetInteger("state", (int)currentState);
    }
    
    private void Idle()
    {
        // Check if player is in aggro range
        if (player != null && Vector2.Distance(transform.position, player.position) <= aggroRange)
        {
            currentState = EnemyState.Chase;
            return;
        }
        
        // Randomly decide to patrol
        if (Random.value < 0.005f) // Small chance per frame to start patrolling
        {
            currentState = EnemyState.Patrol;
        }
    }
    
    private void Patrol()
    {
        // Simple patrol logic (can be expanded)
        rb.velocity = new Vector2(isFacingRight ? moveSpeed/2 : -moveSpeed/2, rb.velocity.y);
        
        // Change direction occasionally
        if (Random.value < 0.01f)
        {
            Flip();
        }
        
        // Check if player is in aggro range
        if (player != null && Vector2.Distance(transform.position, player.position) <= aggroRange)
        {
            currentState = EnemyState.Chase;
        }
        
        // Randomly go back to idle
        if (Random.value < 0.005f)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            currentState = EnemyState.Idle;
        }
    }
    
    private void ChasePlayer()
    {
        if (player == null)
        {
            currentState = EnemyState.Idle;
            return;
        }
        
        // Move towards player
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
        
        // Check if player is out of aggro range
        if (Vector2.Distance(transform.position, player.position) > aggroRange * 1.5f)
        {
            currentState = EnemyState.Idle;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // Check if player is in attack range
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            currentState = EnemyState.Attack;
        }
    }
    
    private void AttackPlayer()
    {
        if (player == null)
        {
            currentState = EnemyState.Idle;
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
        
        // Attack if possible
        if (Time.time >= nextAttackTime)
        {
            // Play attack animation
            animator.SetTrigger("attack");
            
            // Set next attack time
            nextAttackTime = Time.time + 1f / attackRate;
            
            // Apply damage (handled in animation event)
            StartCoroutine(DealDamage());
        }
        
        // Check if player is out of attack range
        if (Vector2.Distance(transform.position, player.position) > attackRange)
        {
            currentState = EnemyState.Chase;
        }
    }
    
    private IEnumerator DealDamage()
    {
        // Wait for attack animation to reach damage frame
        yield return new WaitForSeconds(0.3f);
        
        // Check if still in attack state and player is in range
        if (currentState == EnemyState.Attack && player != null && 
            Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            // Detect player
            Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, 1f, playerLayer);
            
            // Apply damage
            if (hitPlayer != null)
            {
                hitPlayer.GetComponent<PlayerController>()?.TakeDamage(damage);
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Death)
            return;
            
        currentHealth -= damage;
        
        // Play hurt animation
        animator.SetTrigger("hurt");
        currentState = EnemyState.Hurt;
        
        // Knockback
        StartCoroutine(Knockback());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator Knockback()
    {
        // Apply knockback force
        Vector2 knockbackDirection = player != null ? 
            new Vector2(transform.position.x - player.position.x, 0).normalized : 
            new Vector2(isFacingRight ? -1 : 1, 0);
            
        rb.velocity = new Vector2(knockbackDirection.x * 3f, rb.velocity.y);
        
        yield return new WaitForSeconds(0.2f);
        
        // Return to chase state if not dead
        if (currentState != EnemyState.Death)
        {
            currentState = player != null && Vector2.Distance(transform.position, player.position) <= aggroRange ?
                EnemyState.Chase : EnemyState.Idle;
        }
    }
    
    private void Die()
    {
        // Set death state
        currentState = EnemyState.Death;
        
        // Play death animation
        animator.SetTrigger("death");
        
        // Disable physics and collisions
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        
        // Destroy after animation or drop loot
        Destroy(gameObject, 2f);
    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw aggro range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        
        // Draw attack point
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, 1f);
        }
    }
} 
