using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRate = 2f;
    private float nextAttackTime = 0f;
    
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f;
    private bool isInvincible = false;
    
    [Header("Buff System")]
    private float speedBuffModifier = 0f;
    private int attackBuffModifier = 0;
    private float defenseBuffModifier = 0f;
    private float jumpBuffModifier = 0f;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded = false;
    private bool isFacingRight = true;
    private float moveInput;
    
    // Input buffering
    private bool jumpPressed = false;
    private bool attackPressed = false;
    private bool blockPressed = false;
    private bool sprintPressed = false;
    
    private enum MovementState { Idle, Walking, Running, Sprinting, Jumping, Falling }
    private MovementState state;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }
    
    private void Update()
    {
        // Input handling
        moveInput = Input.GetAxisRaw("Horizontal");
        
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }
        
        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            attackPressed = true;
        }
        
        if (Input.GetButtonDown("Fire2"))
        {
            blockPressed = true;
        }
        
        sprintPressed = Input.GetKey(KeyCode.LeftShift);
        
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        
        // Handle animations
        UpdateAnimationState();
    }
    
    private void FixedUpdate()
    {
        // Movement
        float baseSpeed = sprintPressed ? sprintSpeed : (moveInput != 0 ? runSpeed : walkSpeed);
        float appliedSpeed = baseSpeed + (baseSpeed * speedBuffModifier);
        
        rb.velocity = new Vector2(moveInput * appliedSpeed, rb.velocity.y);
        
        // Flip character based on movement direction
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }
        
        // Jump
        if (jumpPressed && isGrounded)
        {
            float totalJumpForce = jumpForce + (jumpForce * jumpBuffModifier);
            rb.velocity = new Vector2(rb.velocity.x, totalJumpForce);
            jumpPressed = false;
        }
        else
        {
            jumpPressed = false;
        }
        
        // Attack
        if (attackPressed)
        {
            Attack();
            attackPressed = false;
        }
        
        // Block
        if (blockPressed)
        {
            Block();
            blockPressed = false;
        }
    }
    
    private void UpdateAnimationState()
    {
        // Determine movement state
        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.Jumping;
        }
        else if (rb.velocity.y < -0.1f)
        {
            state = MovementState.Falling;
        }
        else if (moveInput != 0)
        {
            if (sprintPressed)
                state = MovementState.Sprinting;
            else
                state = MovementState.Running;
        }
        else
        {
            state = MovementState.Idle;
        }
        
        // Update animator
        animator.SetInteger("state", (int)state);
    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }
    
    private void Attack()
    {
        // Play attack animation
        animator.SetTrigger("attack");
        
        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        
        // Calculate total damage with buffs
        int totalDamage = attackDamage + attackBuffModifier;
        
        // Apply damage
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyController>()?.TakeDamage(totalDamage);
        }
        
        // Set cooldown
        nextAttackTime = Time.time + 1f / attackRate;
    }
    
    private void Block()
    {
        // Play block animation
        animator.SetTrigger("block");
        
        // Implement blocking logic here
        StartCoroutine(BlockCooldown());
    }
    
    private IEnumerator BlockCooldown()
    {
        // Add invulnerability or damage reduction logic here
        
        yield return new WaitForSeconds(0.5f);
        
        // End blocking state
    }
    
    public void TakeDamage(int damage)
    {
        // Don't take damage if blocking or invincible
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Block") || isInvincible)
            return;
        
        // Calculate damage reduction from defense buff
        float damageReduction = damage * defenseBuffModifier;
        int actualDamage = Mathf.Max(1, damage - Mathf.RoundToInt(damageReduction));
            
        currentHealth -= actualDamage;
        
        // Play hurt animation
        animator.SetTrigger("hurt");
        
        // Start invincibility
        StartCoroutine(InvincibilityFrames());
        
        // Update UI
        UIManager.Instance?.UpdateHealthDisplay(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        
        // Flash effect to show invincibility
        for (float i = 0; i < invincibilityDuration; i += 0.2f)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        
        isInvincible = false;
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        // Update UI
        UIManager.Instance?.UpdateHealthDisplay(currentHealth, maxHealth);
    }
    
    private void Die()
    {
        // Play death animation
        animator.SetTrigger("death");
        
        // Disable player control
        this.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        
        // Handle game over
        GameManager.Instance?.GameOver();
    }
    
    // Buff system methods
    public void AddSpeedBuff(float amount)
    {
        speedBuffModifier += amount;
    }
    
    public void RemoveSpeedBuff(float amount)
    {
        speedBuffModifier -= amount;
    }
    
    public void AddAttackBuff(int amount)
    {
        attackBuffModifier += amount;
    }
    
    public void RemoveAttackBuff(int amount)
    {
        attackBuffModifier -= amount;
    }
    
    public void AddDefenseBuff(float amount)
    {
        defenseBuffModifier += amount;
    }
    
    public void RemoveDefenseBuff(float amount)
    {
        defenseBuffModifier -= amount;
    }
    
    public void AddJumpBuff(float amount)
    {
        jumpBuffModifier += amount;
    }
    
    public void RemoveJumpBuff(float amount)
    {
        jumpBuffModifier -= amount;
    }
    
    // Getter methods for UI and other systems
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetSpeedBuffModifier() => speedBuffModifier;
    public int GetAttackBuffModifier() => attackBuffModifier;
    public float GetDefenseBuffModifier() => defenseBuffModifier;
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
            
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    
    public void MakeInvincible(bool invincible)
    {
        isInvincible = invincible;
        
        // Visual feedback for invincibility
        if (invincible)
        {
            // Set semi-transparent appearance
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f);
        }
        else
        {
            // Reset to normal appearance
            spriteRenderer.color = Color.white;
        }
    }
} 
