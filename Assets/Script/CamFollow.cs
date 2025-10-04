using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Import DOTween namespace

public class CamFollow : MonoBehaviour
{
    public Transform target; // The target to follow
    public Vector3 offset = new Vector3(0, 0, -10); // For 2D camera, Z is usually -10
    
    [Header("Follow Settings")]
    [Tooltip("Camera follow speed, higher value means tighter follow")]
    public float followSpeed = 5f; // Follow speed (tightness)
    [Tooltip("Use smooth damp for following")]
    public bool useSmoothDamp = true; // Whether to use smooth interpolation
    
    [Header("Anti-Shake Settings")]
    [Tooltip("Enable physics interpolation to reduce shake")]
    public bool useFixedUpdate = true; // Use FixedUpdate for physics synchronization
    [Tooltip("Minimum movement threshold, camera won't move below this value")]
    public float moveThreshold = 0.01f; // Movement threshold to reduce micro-jitter
    [Tooltip("Enable pixel perfect alignment (for pixel art games)")]
    public bool pixelPerfect = false; // Pixel perfect
    [Tooltip("Pixels Per Unit")]
    public float pixelsPerUnit = 16f; // Pixel units

    private Transform initialTarget;
    private Vector3 velocity = Vector3.zero; // For SmoothDamp
    private Vector3 targetPosition; // Cache target position
    
    void Start()
    {
        if (target != null)
        {
            // Set initial camera position (follow X and Y only, Z remains offset.z)
            Vector3 startPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);
            transform.position = startPos;
            targetPosition = startPos;
            initialTarget = target;
        }
    }

    void Update()
    {
        // Handle input
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestSwitchTarget();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTarget();
        }
        
        // If not using FixedUpdate mode, update camera in Update
        if (!useFixedUpdate)
        {
            UpdateCameraPosition();
        }
    }

    void FixedUpdate()
    {
        // If using FixedUpdate mode, synchronize with physics update
        if (useFixedUpdate && target != null)
        {
            CalculateTargetPosition();
        }
    }

    void LateUpdate()
    {
        // If using FixedUpdate mode, apply interpolation in LateUpdate
        if (useFixedUpdate && target != null)
        {
            ApplyCameraMovement();
        }
    }

    private void UpdateCameraPosition()
    {
        if (target == null) return;
        
        CalculateTargetPosition();
        ApplyCameraMovement();
    }

    private void CalculateTargetPosition()
    {
        // Calculate target position
        targetPosition = new Vector3(
            target.position.x + offset.x, 
            target.position.y + offset.y, 
            offset.z
        );
    }

    private void ApplyCameraMovement()
    {
        Vector3 newPosition;
        
        if (useSmoothDamp)
        {
            // Use SmoothDamp for smooth follow (recommended for reducing shake)
            newPosition = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref velocity, 
                1f / followSpeed
            );
        }
        else
        {
            // Use Lerp for linear interpolation follow
            newPosition = Vector3.Lerp(
                transform.position, 
                targetPosition, 
                followSpeed * Time.deltaTime
            );
        }
        
        // Apply movement threshold to filter micro-jitter
        if (Vector3.Distance(transform.position, newPosition) > moveThreshold)
        {
            if (pixelPerfect)
            {
                // Pixel perfect alignment
                newPosition.x = Mathf.Round(newPosition.x * pixelsPerUnit) / pixelsPerUnit;
                newPosition.y = Mathf.Round(newPosition.y * pixelsPerUnit) / pixelsPerUnit;
            }
            
            transform.position = newPosition;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        velocity = Vector3.zero; // Reset velocity to avoid sudden changes
    }

    void TestSwitchTarget()
    {
        // Test switching target
        GameObject newTargetObj = GameObject.Find("NewTarget");
        if (newTargetObj != null)
        {
            SetTarget(newTargetObj.transform);
        }
    }

    void ResetTarget()
    {
        if (initialTarget != null)
            SetTarget(initialTarget);
    }

    void OnDestroy()
    {
        // Clean up DOTween animations to prevent memory leaks
        DOTween.Kill(transform);
    }
}
/*
 private bool IsGrounded()
    {
        Vector2 origin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }
 */