using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerID = 1; // Set to 1 or 2 in Inspector
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public int maxHealth = 3;

    [Header("Components")]
    private Rigidbody2D rb;

    [Header("Player State")]
    private int currentHealth;
    private int score = 0;
    public bool isGrounded;

    [Header("Ground Check")]
    public Transform groundCheck;          // Empty object at feet
    public LayerMask groundLayer;          // Set to Ground in Inspector
    public float checkRadius = 0.15f;      // tweak in Inspector

    [Header("Respawn")]
    public Transform spawnPoint;

    [Header("Animation")]
    private Animator animator;
    private string currentState;
    private SpriteRenderer spriteRenderer;

    [Header("Animation states")]
    const string IDLE = "Idle";
    const string RUNNING = "Running";
    const string HIT = "Hit";
    const string JUMPUP = "JumpUp";
    const string JUMPDOWN = "JumpDown";

    private bool isInvincible = false; // for temporary invincibility during respawn
    private float moveInput = 0f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        if (GameManager.instance != null)
            GameManager.instance.RegisterPlayer(this, playerID);
    }

    void Update()
    {
        // Movement input
        moveInput = Input.GetAxisRaw("Horizontal" + playerID);

        // Ground check: small circle at feet
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        HandleMovement();
        HandleJump();
        HandleAttack();
    }

    void FixedUpdate()
    {
        // Apply horizontal velocity
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }

    void HandleMovement()
    {
        float move = Input.GetAxisRaw("Horizontal" + playerID);
        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);

        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(move));

        if (Mathf.Abs(move) > 0.01f & isGrounded)
        {
            ChangeAnimationState(RUNNING);
        }
        if(Mathf.Abs(move) == 0.0f & isGrounded)
        {
            ChangeAnimationState(IDLE);
        }

        if (move < 0) spriteRenderer.flipX = true;
        else if (move > 0) spriteRenderer.flipX = false;
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump" + playerID) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            GameManager.instance.PlayJumpSound();
            ChangeAnimationState(JUMPUP);
        }
        if (!isGrounded)
        {
            if (rb.velocity.y > 0.1f)
            {
                ChangeAnimationState(JUMPUP);
            }
            else if (rb.velocity.y < -0.1f)
            {
                ChangeAnimationState(JUMPDOWN);
            }
        }
    }

    void HandleAttack()
    {
        if (Input.GetButtonDown("Fire" + playerID))
        {
            Vector2 shootDir = spriteRenderer.flipX ? Vector2.left : Vector2.right;

            GameObject proj = Instantiate(GameManager.instance.projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<Projectile2D>().SetDirection(shootDir);

            // Ignore hitting self
            Physics2D.IgnoreCollision(proj.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        GameManager.instance.UpdateScoreUI(playerID, score);
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        currentHealth -= amount;
        ChangeAnimationState(HIT);

        if (currentHealth <= 0)
        {
            currentHealth = 0; // clamp so it never goes negative
            GameManager.instance.UpdateHealthUI(playerID, currentHealth); // force UI update to zero
            GameManager.instance.PlayerDied(playerID);
        }
        else
        {
            GameManager.instance.UpdateHealthUI(playerID, currentHealth);
        }
    }

    public void Respawn()
    {
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y, 0f);
            currentHealth = maxHealth;
            gameObject.SetActive(true);

            if (GameManager.instance != null)
                GameManager.instance.UpdateHealthUI(playerID, currentHealth);
        }
    }

    public int GetScore() => score;
    public int GetHealth() => currentHealth;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Check if player is above the enemy
            if (rb.velocity.y < 0 && transform.position.y > collision.transform.position.y + 0.2f)
            {
                // Kill enemy
                Destroy(collision.gameObject);
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * 0.6f); // bounce up a bit
                GameManager.instance.PlayEnemyDeathSound();
            }
            else
            {
                // Player takes damage
                TakeDamage(1);
            }
        }

        if (collision.gameObject.CompareTag("Damage"))
        {
            TakeDamage(1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Collectable"))
        {
            AddScore(1); // score amount added
            GameManager.instance.PlayCollectSound();
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Damage layer"))
        {
            TakeDamage(3);
        }
    }

}