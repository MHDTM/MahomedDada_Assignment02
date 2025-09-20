using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyChase2D : MonoBehaviour
{
    public float speed = 2f;
    public float detectionRange = 5f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.3f;
    public Transform groundCheckPoint;

    private Transform targetPlayer;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float moveDir = 1f; // current horizontal direction
    private float turnCooldown = 0f; // prevents jitter at edges

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (turnCooldown > 0)
        {
            turnCooldown -= Time.deltaTime;
        }

        // --- Ground check ---
        bool groundAhead = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);

        if (!groundAhead && turnCooldown <= 0f)
        {
            // Always prioritize edge safety over chasing
            moveDir *= -1f;
            turnCooldown = 0.25f; // add delay before turning again
        }
        else
        {
            // --- Find nearest player ---
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            float shortestDistance = Mathf.Infinity;
            targetPlayer = null;

            foreach (var p in players)
            {
                float distance = Vector2.Distance(transform.position, p.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    targetPlayer = p.transform;
                }
            }

            // If player is within detection range, chase
            if (targetPlayer != null && shortestDistance <= detectionRange)
            {
                moveDir = Mathf.Sign(targetPlayer.position.x - transform.position.x);
            }
        }

        // --- Apply movement ---
        rb.velocity = new Vector2(moveDir * speed, rb.velocity.y);

        // Flip sprite
        if (moveDir != 0)
            spriteRenderer.flipX = moveDir > 0;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckDistance);
        }
    }
}