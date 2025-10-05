using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floatitem : MonoBehaviour
{
    public float floatAmplitude = 0.2f;   
    public float floatFrequency = 1f;     

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = startPos + new Vector3(0, offsetY, 0);
    }
}
