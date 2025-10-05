using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.Animations;
using System;

public class CharacterCtrl : MonoBehaviour
{

    public float JumpForce { get; set; } = 34f;
    public float MoveSpeed { get; set; } = 9f;
    public LayerMask groundLayer; // Ground layer
    public LayerMask crossLayer;
    public float groundCheckDistance = 1.9f; // Raycast distance for ground check
    public float crossCheckDistance = 1.0f; // Raycast distance for cross platform check

    [Header("Scale Effect Settings")]
    public Vector3 jumpScale = new Vector3(0.8f, 1.2f, 1f); // ÌøÔ¾Ê±µÄËõ·Å£ºÊÝ³¤
    public Vector3 landScale = new Vector3(1.2f, 0.8f, 1f); // ÂäµØÊ±µÄËõ·Å£º°«ÅÖ
    public float jumpScaleDuration = 0.2f; // ÌøÔ¾Ëõ·Å¶¯»­Ê±³¤
    public float landScaleDuration = 0.15f; // ÂäµØËõ·Å¶¯»­Ê±³¤
    public float landRecoverDuration = 0.5f; // ÂäµØºó»Ö¸´Õý³£µÄÊ±³¤

    [Header("Raycast Settings")]
    public Vector2 groundCheckOffset = Vector2.zero; // ÉäÏß¼ì²âÆðµãµÄÆ«ÒÆÁ¿

    private Rigidbody2D rb;
    private Animator an;
    private CapsuleCollider2D col;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    private float _time;
    private FrameInput _frameInput;
    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    //variables for jumping
    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;
    private float _frameLeftGrounded = float.MinValue;
    public float JumpBuffer = 0.2f;
    public float CoyoteTime = 0.15f;
    [Tooltip("//The detection distance for grounding and roof detection"), Range(0f, 0.5f)]
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
    Collider2D LastOneWay;   // last one-way we were standing on
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[2];
    private bool isGrounded = false;
    private bool wasGrounded = false; // ÓÃÓÚ¼ì²âÂäµØË²¼ä
    private Vector3 originalScale = Vector3.one; // ´æ´¢Ô­Ê¼Ëõ·Å
    private int currentFacingDirection = 1; // 1 = right, -1 = left

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !isGrounded && _time < _frameLeftGrounded + CoyoteTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        an = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        originalScale = new Vector3(1, 1, 1); // ±£´æÔ­Ê¼Ëõ·Å£¨²»º¬·­×ª£©
        currentFacingDirection = transform.localScale.x > 0 ? 1 : -1;
    }

    void Update()
    {
        _time += Time.deltaTime;
        //CheckGroundWithRaycast();
        Move();
        //CheckLanding();
    }
    private void FixedUpdate()
    {
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    private void CheckCollisions()
    {
        // Ground check: hit both solid + one-way, only “up-ish” surfaces
        var groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundLayer | crossLayer,
            useTriggers = false,
            useNormalAngle = true
        };
        groundFilter.minNormalAngle = 60f;   // tweak slope tolerance
        groundFilter.maxNormalAngle = 120f;

        int count = col.Cast(Vector2.down, groundFilter, _hits, GrounderDistance);

        bool onSolid = false, onOneWay = false;
        Collider2D hitOneWay = null;

        for (int i = 0; i < count; i++)
        {
            int bit = 1 << _hits[i].collider.gameObject.layer;
            if ((crossLayer.value & bit) != 0) { onOneWay = true; hitOneWay = _hits[i].collider; }
            else { onSolid = true; }
        }

        // One-ways only count as ground when falling/resting
        bool groundHit = onSolid || (onOneWay && _frameVelocity.y <= 0f);

        // Ceiling: solid only (do NOT include one-way)
        var ceilingFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundLayer,
            useTriggers = false,
            useNormalAngle = true
        };
        ceilingFilter.minNormalAngle = 240f; // “down-ish”
        ceilingFilter.maxNormalAngle = 300f;

        bool ceilingHit = col.Cast(Vector2.up, ceilingFilter, _hits, GrounderDistance) > 0;
        if (ceilingHit) _frameVelocity.y = Mathf.Min(0f, _frameVelocity.y);

        // Grounded state transitions
        if (!isGrounded && groundHit)
        {
            isGrounded = true;
            _coyoteUsable = _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            LastOneWay = onOneWay ? hitOneWay : null;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        else if (isGrounded && !groundHit)
        {
            isGrounded = false;
            LastOneWay = null;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }
    }
    private void HandleJump()
    {
        if (!_endedJumpEarly && !isGrounded && !_frameInput.JumpHeld && rb.velocity.y > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (isGrounded || CanUseCoyote) ExecuteJump();

        _jumpToConsume = false;
    }
    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = JumpForce;

        //play animation
        an.SetTrigger("jump");
        // ÌøÔ¾Ê±±äÊÝ³¤µÄ¶¯»­
        DOTween.Kill(transform); // É±ËÀµ±Ç°transformÉÏµÄËùÓÐTween£¬±ÜÃâ³åÍ»
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

    //private void CheckGroundWithRaycast()
    //{
    //    // Ê¹ÓÃÉäÏß¼ì²âµØÃæ£¬²»ÊÜscale±ä»¯Ó°Ïì
    //    Vector2 origin = (Vector2)transform.position + groundCheckOffset;
    //    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

    //    isGrounded = hit.collider != null;

    //    // ¿ÉÑ¡£ºÔÚSceneÊÓÍ¼ÖÐ»æÖÆµ÷ÊÔÉäÏß
    //    Debug.DrawRay(origin, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    //}

    private void Move()
    {
        // Horizontal movement
        float moveInput = 0f;
        Vector2 direction = Vector2.zero;

        if (Input.GetKey(KeyCode.A) && (!Input.GetKey(KeyCode.D)))
        {
            //moveInput = -1f;
            currentFacingDirection = -1;
            ApplyFacingDirection();
            an.SetBool("isrunning", true);
            direction = Vector2.left;
        }
        else if (Input.GetKey(KeyCode.D) && !(Input.GetKey(KeyCode.A)))
        {
            //moveInput = 1f;
            currentFacingDirection = 1;
            ApplyFacingDirection();
            an.SetBool("isrunning", true);
            direction = Vector2.right;
        }
        else
        {
            moveInput = 0f;
            an.SetBool("isrunning", false);
        }
        //Vector2 velocity = rb.velocity;
        //velocity.x = moveInput * MoveSpeed;
        //rb.velocity = velocity;

        //// Jump
        //if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        //{
        //    Jump();
        //}
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
    }


    //private void Jump()
    //{
    //    rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
    //    an.SetTrigger("jump");
    //    // ÌøÔ¾Ê±±äÊÝ³¤µÄ¶¯»­
    //    DOTween.Kill(transform); // É±ËÀµ±Ç°transformÉÏµÄËùÓÐTween£¬±ÜÃâ³åÍ»
    //    Vector3 targetScale = new Vector3(jumpScale.x * currentFacingDirection, jumpScale.y, jumpScale.z);
    //    transform.DOScale(targetScale, jumpScaleDuration).SetEase(Ease.OutQuad);
    //}

    //private void CheckLanding()
    //{
    //    // ¼ì²â´Ó¿ÕÖÐµ½µØÃæµÄË²¼ä
    //    if (isGrounded && !wasGrounded)
    //    {
    //        OnLand();
    //    }
    //    wasGrounded = isGrounded;
    //}

    //private void OnLand()
    //{
    //    // ÂäµØÊ±ÏÈ±ä°«ÅÖ£¬È»ºó»Ö¸´Õý³£
    //    DOTween.Kill(transform); // É±ËÀµ±Ç°transformÉÏµÄËùÓÐTween

    //    Vector3 landScaleWithDirection = new Vector3(landScale.x * currentFacingDirection, landScale.y, landScale.z);
    //    Vector3 normalScaleWithDirection = new Vector3(originalScale.x * currentFacingDirection, originalScale.y, originalScale.z);

    //    // ´´½¨ÂäµØËõ·ÅÐòÁÐ
    //    Sequence landSequence = DOTween.Sequence();
    //    landSequence.Append(transform.DOScale(landScaleWithDirection, landScaleDuration).SetEase(Ease.OutQuad));
    //    landSequence.Append(transform.DOScale(normalScaleWithDirection, landRecoverDuration).SetEase(Ease.OutBack));
    //}

    private void ApplyFacingDirection()
    {
        // Ó¦ÓÃ³¯Ïò£¬Í¬Ê±±£³Öµ±Ç°µÄYºÍZËõ·Å
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * currentFacingDirection, currentScale.y, currentScale.z);
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
