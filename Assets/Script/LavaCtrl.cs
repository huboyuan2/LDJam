using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LavaCtrl : MonoBehaviour
{
    public float raiseSpeed = 1f;
    private Material lavaMaterial;
    private Renderer rend;
    public bool canRaise = false;
    
    private float currentRaiseDirection = 1f; // 1 = up, -1 = down, 0 = paused

    // Start is called before the first frame update
    void Start()
    {
        // Get the Renderer component
        rend = GetComponent<Renderer>();
        
        if (rend != null)
        {
            // Create a material instance to avoid modifying the shared material
            lavaMaterial = rend.material;
        }
        else
        {
            Debug.LogError("[LavaCtrl] No Renderer component found on this GameObject!");
        }

        // Subscribe to time state change event
        if (GameLogic.Instance != null)
        {
            GameLogic.Instance.OnTimeStateChanged += HandleTimeStateChange;
            
            // Initialize with current state
            HandleTimeStateChange((int)GameLogic.Instance.current);
        }
    }

    // Handle time state changes
    private void HandleTimeStateChange(int newState)
    {
        GameLogic.TimeState state = (GameLogic.TimeState)newState;

        switch (state)
        {
            case GameLogic.TimeState.Reversing:
                currentRaiseDirection = -1f;
                lavaMaterial?.SetFloat("_wavespeed", -1f);
                break;

            case GameLogic.TimeState.Advancing:
                currentRaiseDirection = 1f;
                lavaMaterial?.SetFloat("_wavespeed", 1f);
                break;

            case GameLogic.TimeState.Paused:
                currentRaiseDirection = 0f;
                lavaMaterial?.SetFloat("_wavespeed", 0f);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only update position and shader height
        if (canRaise && currentRaiseDirection != 0f)
        {
            transform.position += Vector3.up * raiseSpeed * currentRaiseDirection * Time.deltaTime;
        }

        // Update shader surface height
        if (lavaMaterial != null)
        {
            lavaMaterial.SetFloat("_surfacehight", transform.position.y);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        if (GameLogic.Instance != null)
        {
            GameLogic.Instance.OnTimeStateChanged -= HandleTimeStateChange;
        }

        // Clean up material instance
        if (lavaMaterial != null)
        {
            Destroy(lavaMaterial);
        }
    }
}
