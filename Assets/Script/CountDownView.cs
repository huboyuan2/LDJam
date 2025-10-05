using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDownView : MonoBehaviour
{
    public TMP_Text timeText;
    private void Awake()
    {
        CountDown.OnTimeChanged += UpdateView;

        
    }
    // Start is called before the first frame update
    void Start()
    {
        if (timeText == null)
        {
            //Debug.LogError("Time Text is not assigned in the inspector.");
            timeText = GetComponent<TMP_Text>();
        }
    }

    // Update is called once per frame
    //void Update()
    //{
    //}

    private void UpdateView(string newTimeText)
    {
        // Update your UI elements here with the new time text
        timeText.text = newTimeText;

    }
}
