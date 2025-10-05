using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.Animations;
using System;

public class CharacterCtrl : MonoBehaviour
{
    private static CharacterCtrl _instance;

    public static CharacterCtrl Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CharacterCtrl>();

                if (_instance == null)
                {
                    Debug.LogWarning("[CharacterCtrl] No CharacterCtrl found in scene!");
                }
            }
            return _instance;   
        }
    }
    Vector2 curRebirthPlace;
    public Vector2 CurRebirthPlace
    {
        get => curRebirthPlace;
        set
        {
            curRebirthPlace = value;
        }
    }
    public float JumpForce { get; set; } = 36f;
    public float MoveSpeed { get; set; } = 9f;
    public LayerMask groundLayer; // Ground layer
    public LayerMask crossLayer; // One-way platform layer
    public LayerMask movingPlatformLayer; // Moving platform layer
    public float groundCheckDistance = 1.9f; // Raycast distance for ground check
    public float crossCheckDistance = 1.0f; // Raycast distance for cross platform check

    [Header("Scale Effect Settings")]
    public Vector3 jumpScale = new Vector3(0.8f, 1.2f, 1f); // Jump scale effect (thin and tall)
    public Vector3 landScale = new Vector3(1.2f, 0.8f, 1f); // Land scale effect (short and wide)
    public float jumpScaleDuration = 0.2f; // Jump scale animation duration
    public float landScaleDuration = 0.15f; // Land scale animation duration
    public float landRecoverDuration = 0.5f; // Land recover to normal duration

    [Header("Raycast Settings")]
    public Vector2 groundCheckOffset = Vector2.zero; // Ground check offset

    [Header("Moving Platform Settings")]
    public bool compensatePlatformRotation = false; // Compensate for platform rotation (usually not needed)

    private Rigidbody2D rb;
    private Animator an;
    private CapsuleCollider2D col;
    private Vector2 _frameVelocity;
    private float _time;
    private FrameInput _frameInput;
    public Vector2 FrameInput()
    {
        return _frameInput.Move;
    }
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public float velocityY;
    // Variables for jumping
    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;
    private float _frameLeftGrounded = float.MinValue;
    public float JumpBuffer = 0.2f;
    public float CoyoteTime = 0.15f;
    [Tooltip("The detection distance for grounding and roof detection"), Range(0f, 0.5f)]
    public float GrounderDistance = 0.05f;
    public LayerMask PlayerLayer;
    [Tooltip("The pace at which the player comes to a stop")]
    public float GroundDeceleration = 60;
    [Tooltip("Deceleration in air only after stopping input mid-air")]
    public float AirDeceleration = 30;
    [Tooltip("The player's capacity to gain horizontal speed")]
    public float Acceleration = 120;
    [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
    public float GroundingForce = -1.5f;
    [Tooltip("The gravity multiplier added when jump is released early")]
    public float JumpEndEarlyGravityModifier = 3;
    [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
    public float FallAcceleration = 110;
    [Tooltip("The maximum vertical movement speed")]
    public float MaxFallSpeed = 40;

    Collider2D LastOneWay; // Last one-way platform we were standing on
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[2];
    private bool isGrounded = false;
    private bool wasGrounded = false; // Was grounded last frame
    private Vector3 originalScale = Vector3.one; // Original scale (without flip)
    private int currentFacingDirection = 1; // 1 = right, -1 = left

    // Moving platform related
    private Transform currentPlatform; // Current moving platform we're standing on
    private Vector3 lastPlatformPosition; // Last frame platform world position
    private Quaternion lastPlatformRotation; // Last frame platform rotation (optional)

    private CharacterModule abilities;
    private GameLogic manager;
    private bool HasBufferedJump()
    {
        if (_bufferedJumpUsable)
        {
            if (_time < _timeJumpWasPressed + JumpBuffer)
            {
                return true;
            }
        }
        return false;
    }

    private bool CanUseCoyote()
    {
        return _coyoteUsable && !isGrounded && _time < _frameLeftGrounded + CoyoteTime;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        an = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();

        originalScale = new Vector3(1, 1, 1); // Save original scale (without flip)
        currentFacingDirection = transform.localScale.x > 0 ? 1 : -1;
        _bufferedJumpUsable = false;
        _timeJumpWasPressed = float.NegativeInfinity; // so the check is false at spawn
        abilities = GetComponent<CharacterModule>();
        manager = FindFirstObjectByType<GameLogic>();
        curRebirthPlace = transform.position;
    }

    void Update()
    {
        if (abilities.isDashing)
        {
            return;
        }
        velocityY = rb.velocity.y;
        _time += Time.deltaTime;
        GatherInput();
    }

    private void FixedUpdate()
    {
        if (abilities.isDashing)
        {
            return;
        }
        CheckCollisions();
        HandleMovingPlatform();
        HandleJump();
        HandleDirection();
        HandleGravity();
        ApplyMovement();
    }

    private void CheckCollisions()
    {
        // Ground check: hit both solid + one-way, only "up-ish" surfaces
        var groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundLayer | crossLayer | movingPlatformLayer, // Include moving platforms
            useTriggers = false,
            useNormalAngle = true
        };
        groundFilter.minNormalAngle = 60f;   // Tweak slope tolerance
        groundFilter.maxNormalAngle = 120f;

        int count = col.Cast(Vector2.down, groundFilter, _hits, GrounderDistance);

        bool onSolid = false, onOneWay = false, onMovingPlatform = false;
        Collider2D hitOneWay = null;
        Collider2D hitMovingPlatform = null;

        for (int i = 0; i < count; i++)
        {
            int bit = 1 << _hits[i].collider.gameObject.layer;
            if ((movingPlatformLayer.value & bit) != 0)
            {
                onMovingPlatform = true;
                hitMovingPlatform = _hits[i].collider;
            }
            else if ((crossLayer.value & bit) != 0)
            {
                onOneWay = true;
                hitOneWay = _hits[i].collider;
            }
            else
            {
                onSolid = true;
            }
        }

        // One-ways only count as ground when falling/resting
        bool groundHit = onSolid || onMovingPlatform || (onOneWay && _frameVelocity.y <= 0f);

        // Handle moving platform attachment
        if (onMovingPlatform && _frameVelocity.y <= 0f)
        {
            Transform platformTransform = hitMovingPlatform.transform;
            if (currentPlatform != platformTransform)
            {
                AttachToPlatform(platformTransform);
            }
        }
        else
        {
            // Not on moving platform, detach if necessary
            DetachFromPlatform();
        }

        // Ceiling: solid only (do NOT include one-way)
        var ceilingFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundLayer | movingPlatformLayer, // Include moving platforms for ceiling
            useTriggers = false,
            useNormalAngle = true
        };
        ceilingFilter.minNormalAngle = 240f; // "down-ish"
        ceilingFilter.maxNormalAngle = 300f;

        bool ceilingHit = col.Cast(Vector2.up, ceilingFilter, _hits, GrounderDistance) > 0;
        if (ceilingHit) _frameVelocity.y = Mathf.Min(0f, _frameVelocity.y);

        // Grounded state transitions
        if (!isGrounded && groundHit)
        {
            if (!wasGrounded)
            {
                // Play landing animation
                DOTween.Kill(transform); // Kill all current transform tweens

                Vector3 landScaleWithDirection = new Vector3(landScale.x * currentFacingDirection, landScale.y, landScale.z);
                Vector3 normalScaleWithDirection = new Vector3(originalScale.x * currentFacingDirection, originalScale.y, originalScale.z);

                // Create landing scale sequence
                Sequence landSequence = DOTween.Sequence();
                landSequence.Append(transform.DOScale(landScaleWithDirection, landScaleDuration).SetEase(Ease.OutQuad));
                landSequence.Append(transform.DOScale(normalScaleWithDirection, landRecoverDuration).SetEase(Ease.OutBack));
                an.SetBool("jump", true);
                wasGrounded = true;
            }
            isGrounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            LastOneWay = onOneWay ? hitOneWay : null;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        else if (isGrounded && !groundHit)
        {
            isGrounded = false;
            wasGrounded = false;
            LastOneWay = null;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }
    }

    private void HandleMovingPlatform()
    {
        if (currentPlatform == null) return;

        // Calculate platform movement delta (world space)
        Vector3 platformDelta = currentPlatform.position - lastPlatformPosition;

        // Method: Directly modify Rigidbody position (recommended, physics-friendly)
        rb.position += (Vector2)platformDelta;

        // Optional: Handle platform rotation (if platform rotates and character should follow)
        if (compensatePlatformRotation)
        {
            Quaternion rotationDelta = currentPlatform.rotation * Quaternion.Inverse(lastPlatformRotation);
            Vector3 relativePos = transform.position - currentPlatform.position;
            Vector3 rotatedPos = rotationDelta * relativePos;
            Vector3 rotationOffset = rotatedPos - relativePos;
            rb.position += (Vector2)rotationOffset;
        }

        // Update recorded platform state
        lastPlatformPosition = currentPlatform.position;
        lastPlatformRotation = currentPlatform.rotation;
    }

    private void AttachToPlatform(Transform platform)
    {
        // Avoid duplicate attachment
        if (currentPlatform == platform) return;

        currentPlatform = platform;
        lastPlatformPosition = platform.position;
        lastPlatformRotation = platform.rotation;

        Debug.Log($"[CharacterCtrl] Attached to moving platform: {platform.name}");
    }

    private void DetachFromPlatform()
    {
        if (currentPlatform == null) return;

        Debug.Log($"[CharacterCtrl] Detached from platform: {currentPlatform.name}");
        currentPlatform = null;
    }

    private void HandleJump()
    {
        if (!_endedJumpEarly && !isGrounded && !_frameInput.JumpHeld && rb.velocity.y > 0)
        {
            _endedJumpEarly = true;
        }

        if (!_jumpToConsume && !HasBufferedJump())
        {
            return;
        }

        if (isGrounded || CanUseCoyote())
        {
            ExecuteJump();
        }

        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        // Detach from platform immediately when jumping
        DetachFromPlatform();

        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = JumpForce;

        // Play jump animation
        an.SetBool("jump", true);

        // Jump scale animation (thin and tall)
        DOTween.Kill(transform); // Kill all current transform tweens to avoid conflicts
        Vector3 targetScale = new Vector3(jumpScale.x * currentFacingDirection, jumpScale.y, jumpScale.z);
        transform.DOScale(targetScale, jumpScaleDuration).SetEase(Ease.OutQuad);

        Jumped?.Invoke();
    }

    private void HandleDirection()
    {
        if (_frameInput.Move.x == 0)
        {
            var deceleration = isGrounded ? GroundDeceleration : AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * MoveSpeed, Acceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        if (isGrounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = GroundingForce;
        }
        else
        {
            var inAirGravity = FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= JumpEndEarlyGravityModifier;
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovement()
    {
        rb.velocity = _frameVelocity;
    }

    private void GatherInput()
    {
        // Horizontal movement input
        Vector2 direction = Vector2.zero;

        if (Input.GetKey(KeyCode.A) && (!Input.GetKey(KeyCode.D)))
        {
            currentFacingDirection = -1;
            ApplyFacingDirection();
            an.SetBool("isrunning", true);
            direction = Vector2.left;
        }
        else if (Input.GetKey(KeyCode.D) && !(Input.GetKey(KeyCode.A)))
        {
            currentFacingDirection = 1;
            ApplyFacingDirection();
            an.SetBool("isrunning", true);
            direction = Vector2.right;
        }
        else
        {
            an.SetBool("isrunning", false);
        }

        _frameInput = new FrameInput
        {
            JumpDown = Input.GetKeyDown(KeyCode.W),
            JumpHeld = Input.GetKey(KeyCode.W),
            Move = direction
        };
        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }
        //KEY INPUT FOR TIME STOP SKILL
        if (abilities.SkillStop > 0 && abilities.canStop && Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(TimeStop());
        }
        //KEY INPUT FOR DASH
        if (abilities.SkillDash > 0 && abilities.canDash && Input.GetKeyDown(KeyCode.LeftShift) )
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator TimeStop()
    {
        abilities.SkillStop -= 1;
        manager.pauseTime();
        abilities.canStop = false;
        yield return new WaitForSeconds(abilities.TimeStopDuration);
        manager.returnTimeState();
        abilities.canStop = true;
    }

    private IEnumerator Dash()
    {
        abilities.canDash = false;
        abilities.isDashing = true;
        abilities.SkillDash -= 1;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * abilities.dashingPower, 0f);

        yield return new WaitForSeconds(abilities.dashingTime);

        rb.gravityScale = originalGravity;
        abilities.isDashing = false;
        yield return new WaitForSeconds(abilities.dashingCooldown);
        abilities.canDash = true;
    }
    public void CallRebirth()
    {
        //an.SetBool("death", true);
        StartCoroutine(Rebirth());
    }
    IEnumerator Rebirth()
    {
        yield return new WaitForSeconds(0.5f);
        transform.position = CurRebirthPlace;
        rb.velocity = Vector2.zero;
        an.SetBool("death", false);
  //      manager.returnTimeState();
        abilities.canDash = true;
        abilities.canStop = true;
        abilities.isDashing = false;
    }

    private void ApplyFacingDirection()
    {
        // Apply facing direction while maintaining current Y and Z scale
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * currentFacingDirection, currentScale.y, currentScale.z);
    }

    private void OnDestroy()
    {
        // Clean up when destroyed
        DetachFromPlatform();
    }
}

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 Move;
}

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public Vector2 FrameInput { get; }
}