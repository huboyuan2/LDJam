using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CountDown : MonoBehaviour
{
    // Start is called before the first frame update
    //float leftTime = 100.0f;
    string timeText = "1:40";
    public TimeState timeState = TimeState.Running;
    private BarScaler bar;
    public static event System.Action<string> OnTimeChanged;
    void Awake()
    {
        GameLogic.Instance.OnTimeStateChanged += HandleTimeStateChange;
        bar = FindFirstObjectByType<BarScaler>();
    }
    void HandleTimeStateChange(int newState)
    {
        timeState = (TimeState)newState;
    }
    // Update is called once per frame
    void Update()
    {

        //if (timeState == TimeState.Running)
        CharacterModule.Instance.TimeLeft -= Time.deltaTime;
        //else if (timeState == TimeState.Rewinding)
        //CharacterModule.Instance.TimeLeft += Time.deltaTime;
        if (bar)
        {
            bar.InterpolateToScale(CharacterModule.Instance.TimeLeft / 100, 0.1f);
        }



        //string newTimeText = GetTimeText(CharacterModule.Instance.TimeLeft);
        //if (newTimeText != timeText)
        //{
        //    timeText = newTimeText;
        //    InvokeTimeChange(timeText);
        //    //OnTimeStringChanged();
        //}
    }
    public static void InvokeTimeChange(string newTimeText)
    {
        OnTimeChanged?.Invoke(newTimeText);
    }   
    public static string GetTimeText(float timeLeft)
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60F);
        int seconds = Mathf.FloorToInt(timeLeft - minutes * 60);
        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }
    public enum TimeState
    {
        
        Rewinding,
        Running,
        Stopped
    }   
    //private void OnTimeStringChanged()
    //{
    //   OnTimeChanged?.Invoke(timeText);
    //}
}
