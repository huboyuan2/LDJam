using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.Animations;

public class CharacterCtrl : MonoBehaviour
{
    public float JumpForce { get; set; } = 10f;
    public float MoveSpeed { get; set; } = 5f;
    public LayerMask groundLayer; // Ground layer
    public LayerMask crossLayer;
    public float groundCheckDistance = 1.9f; // Raycast distance for ground check
    public float crossCheckDistance = 1.0f; // Raycast distance for cross platform check

    [Header("Scale Effect Settings")]
    public Vector3 jumpScale = new Vector3(0.8f, 1.2f, 1f); // 跳跃时的缩放：瘦长
    public Vector3 landScale = new Vector3(1.2f, 0.8f, 1f); // 落地时的缩放：矮胖
    public float jumpScaleDuration = 0.2f; // 跳跃缩放动画时长
    public float landScaleDuration = 0.15f; // 落地缩放动画时长
    public float landRecoverDuration = 0.5f; // 落地后恢复正常的时长

    [Header("Raycast Settings")]
    public Vector2 groundCheckOffset = Vector2.zero; // 射线检测起点的偏移量

    private Rigidbody2D rb;
    private Animator an;
    private bool isGrounded = false;
    private bool wasGrounded = false; // 用于检测落地瞬间
    private Vector3 originalScale = Vector3.one; // 存储原始缩放
    private int currentFacingDirection = 1; // 1 = right, -1 = left

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        an = GetComponent<Animator>();
        originalScale = new Vector3(1, 1, 1); // 保存原始缩放（不含翻转）
        currentFacingDirection = transform.localScale.x > 0 ? 1 : -1;
    }

    void Update()
    {
        CheckGroundWithRaycast();
        Move();
        CheckLanding();
    }

    private void CheckGroundWithRaycast()
    {
        // 使用射线检测地面，不受scale变化影响
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        
        isGrounded = hit.collider != null;
        
        // 可选：在Scene视图中绘制调试射线
        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    private void Move()
    {
        // Horizontal movement
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)&&(!Input.GetKey(KeyCode.D)))
        {
            moveInput = -1f;
            currentFacingDirection = -1;
            ApplyFacingDirection();
            an.SetBool("isrunning", true);
        }
        else if (Input.GetKey(KeyCode.D)&&!(Input.GetKey(KeyCode.A)))
        {
            moveInput = 1f;
            currentFacingDirection = 1;
            ApplyFacingDirection();
            an.SetBool("isrunning", true);
        }
        else
        {
            moveInput = 0f;
            an.SetBool("isrunning", false);
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
        an.SetTrigger("jump");
        // 跳跃时变瘦长的动画
        DOTween.Kill(transform); // 杀死当前transform上的所有Tween，避免冲突
        Vector3 targetScale = new Vector3(jumpScale.x * currentFacingDirection, jumpScale.y, jumpScale.z);
        transform.DOScale(targetScale, jumpScaleDuration).SetEase(Ease.OutQuad);
    }

    private void CheckLanding()
    {
        // 检测从空中到地面的瞬间
        if (isGrounded && !wasGrounded)
        {
            OnLand();
        }
        wasGrounded = isGrounded;
    }

    private void OnLand()
    {
        // 落地时先变矮胖，然后恢复正常
        DOTween.Kill(transform); // 杀死当前transform上的所有Tween
        
        Vector3 landScaleWithDirection = new Vector3(landScale.x * currentFacingDirection, landScale.y, landScale.z);
        Vector3 normalScaleWithDirection = new Vector3(originalScale.x * currentFacingDirection, originalScale.y, originalScale.z);
        
        // 创建落地缩放序列
        Sequence landSequence = DOTween.Sequence();
        landSequence.Append(transform.DOScale(landScaleWithDirection, landScaleDuration).SetEase(Ease.OutQuad));
        landSequence.Append(transform.DOScale(normalScaleWithDirection, landRecoverDuration).SetEase(Ease.OutBack));
    }

    private void ApplyFacingDirection()
    {
        // 应用朝向，同时保持当前的Y和Z缩放
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * currentFacingDirection, currentScale.y, currentScale.z);
    }
}
