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
    public Vector3 jumpScale = new Vector3(0.8f, 1.2f, 1f); // ��Ծʱ�����ţ��ݳ�
    public Vector3 landScale = new Vector3(1.2f, 0.8f, 1f); // ���ʱ�����ţ�����
    public float jumpScaleDuration = 0.2f; // ��Ծ���Ŷ���ʱ��
    public float landScaleDuration = 0.15f; // ������Ŷ���ʱ��
    public float landRecoverDuration = 0.5f; // ��غ�ָ�������ʱ��

    [Header("Raycast Settings")]
    public Vector2 groundCheckOffset = Vector2.zero; // ���߼������ƫ����

    private Rigidbody2D rb;
    private Animator an;
    private bool isGrounded = false;
    private bool wasGrounded = false; // ���ڼ�����˲��
    private Vector3 originalScale = Vector3.one; // �洢ԭʼ����
    private int currentFacingDirection = 1; // 1 = right, -1 = left

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        an = GetComponent<Animator>();
        originalScale = new Vector3(1, 1, 1); // ����ԭʼ���ţ�������ת��
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
        // ʹ�����߼����棬����scale�仯Ӱ��
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        
        isGrounded = hit.collider != null;
        
        // ��ѡ����Scene��ͼ�л��Ƶ�������
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
        // ��Ծʱ���ݳ��Ķ���
        DOTween.Kill(transform); // ɱ����ǰtransform�ϵ�����Tween�������ͻ
        Vector3 targetScale = new Vector3(jumpScale.x * currentFacingDirection, jumpScale.y, jumpScale.z);
        transform.DOScale(targetScale, jumpScaleDuration).SetEase(Ease.OutQuad);
    }

    private void CheckLanding()
    {
        // ���ӿ��е������˲��
        if (isGrounded && !wasGrounded)
        {
            OnLand();
        }
        wasGrounded = isGrounded;
    }

    private void OnLand()
    {
        // ���ʱ�ȱ䰫�֣�Ȼ��ָ�����
        DOTween.Kill(transform); // ɱ����ǰtransform�ϵ�����Tween
        
        Vector3 landScaleWithDirection = new Vector3(landScale.x * currentFacingDirection, landScale.y, landScale.z);
        Vector3 normalScaleWithDirection = new Vector3(originalScale.x * currentFacingDirection, originalScale.y, originalScale.z);
        
        // ���������������
        Sequence landSequence = DOTween.Sequence();
        landSequence.Append(transform.DOScale(landScaleWithDirection, landScaleDuration).SetEase(Ease.OutQuad));
        landSequence.Append(transform.DOScale(normalScaleWithDirection, landRecoverDuration).SetEase(Ease.OutBack));
    }

    private void ApplyFacingDirection()
    {
        // Ӧ�ó���ͬʱ���ֵ�ǰ��Y��Z����
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * currentFacingDirection, currentScale.y, currentScale.z);
    }
}
