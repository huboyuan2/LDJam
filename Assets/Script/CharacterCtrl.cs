using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCtrl : MonoBehaviour
{
    public float JumpForce { get; set; } = 10f;
    public float MoveSpeed { get; set; } = 5f;
    public LayerMask groundLayer; // Ground layer
    public LayerMask crossLayer;
    public float groundCheckDistance = 1.9f; // Raycast distance for ground check
    public float crossCheckDistance = 1.0f; // Raycast distance for cross platform check

    private Rigidbody2D rb;
    private bool isGrounded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        // Horizontal movement
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            moveInput = -1f;
            transform.localScale = new Vector3(-1, 1, 1); // Face left
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveInput = 1f;
            transform.localScale = new Vector3(1, 1, 1); // Face right
        }
        Vector2 velocity = rb.velocity;
        velocity.x = moveInput * MoveSpeed;
        rb.velocity = velocity;

        // Jump
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
    }

    // Per-frame ground check
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Check if the collided object is in the groundLayer
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Reset isGrounded when leaving the ground
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
        }
    }
}
