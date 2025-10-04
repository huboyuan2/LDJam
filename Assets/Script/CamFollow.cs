using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Import DOTween namespace

public class CamFollow : MonoBehaviour
{
    public Transform target; // The target to follow
    public Vector3 offset = new Vector3(0, 0, -10); // For 2D camera, Z is usually -10
    private Transform initalTarget;

    void Start()
    {
        if (target != null)
        {
            // Set initial camera position (follow X and Y only, Z remains offset.z)
            Vector3 startPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);
            transform.position = startPos;
            initalTarget = target;
        }
    }

    void Update()
    {
        if (target != null)
        {
            // Follow target only on X and Y axes, Z is fixed
            Vector3 targetPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);
            transform.DOMove(targetPos, 1f).SetEase(Ease.OutQuad);
            // 2D camera does not need rotation
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestSwitchTarget();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTarget();
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void TestSwitchTarget()
    {
        // Test switching target
        Transform newTarget = GameObject.Find("NewTarget").transform;
        SetTarget(newTarget);
    }

    void ResetTarget()
    {
        if (initalTarget != null)
            SetTarget(initalTarget);
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