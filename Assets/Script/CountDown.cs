using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CountDown : MonoBehaviour
{
    // Start is called before the first frame update
    float leftTime = 100.0f;
    string timeText = "1:40";
    public TimeState timeState = TimeState.Running;

    public static event System.Action<string> OnTimeChanged;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timeState == TimeState.Running)
            leftTime -= Time.deltaTime;
        else if (timeState == TimeState.Rewinding)
            leftTime += Time.deltaTime;

        string newTimeText = GetTime();
        if (newTimeText != timeText)
        {
            timeText = newTimeText;
            OnTimeChanged?.Invoke(timeText);
            //OnTimeStringChanged();
        }
    }
    //
    string GetTime()
    {
        int minutes = Mathf.FloorToInt(leftTime / 60F);
        int seconds = Mathf.FloorToInt(leftTime - minutes * 60);
        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }
    public enum TimeState
    {
        Running,
        Rewinding,
        Stopped
    }   
    //private void OnTimeStringChanged()
    //{
    //   OnTimeChanged?.Invoke(timeText);
    //}
}
