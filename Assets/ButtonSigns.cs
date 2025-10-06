using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSigns : MonoBehaviour
{
    // Start is called before the first frame update
    [Range(0.1f, 10f)] public float speed = 2f;   // cycles per second
    [Range(0f, 1f)] public float amplitude = 0.1f; // 0.1 = ±10% around base
    public bool useUnscaledTime = false;          // keep pulsing when paused?

    Vector3 _base;
    void Start()
    {
        _base = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float t = (useUnscaledTime ? Time.unscaledTime : Time.time) * speed;
        float s = 1f + Mathf.Sin(t * Mathf.PI * 2f) * amplitude; // 1±amplitude
        transform.localScale = _base * s;
    }
}
