using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // or HighDefinition

public class GameLogic : MonoBehaviour
{
    private static GameLogic _instance;

    public static GameLogic Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameLogic>();

            }
            return _instance;
        }
    }
    GameObject winUI;
     public GameObject player;
    ActionList actionList;

    public enum TimeState
    {
        Reversing,
        Advancing,
        Paused
    }

    public TimeState _current = TimeState.Reversing;

    // Event: invoked when time state changes, passes new state as int
    public Action<int> OnTimeStateChanged;

    public TimeState current
    {
        get => _current;
        set
        {
            if (_current != value)
            {
                _current = value;
                OnTimeStateChanged?.Invoke((int)value);
            }
        }
    }

    TimeState saved;

    private void Awake()
    {
        current = TimeState.Advancing;
        //DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        actionList = GetComponent<ActionList>();
    }

    public void pauseTime()
    {
        saved = current;
        current = TimeState.Paused;

        Volume volume = FindFirstObjectByType<Volume>();
        if (volume)
        {
            CamVignetteAction(this, volume, 0, 0.35f, 0.5f, 4);
        }
    }

    public void returnTimeState()
    {
        current = saved;
    }

    public void TriggerTimeState()
    {
        if (current == TimeState.Paused)
        {
            return;
        }
        if (current == TimeState.Reversing)
        {
            current = TimeState.Advancing;
        }
        else if (current == TimeState.Advancing)
        {
            current = TimeState.Reversing;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Win()
    {

        if (winUI)
        {
            winUI.SetActive(true);
        }
        pauseTime();
    }
    public void MoveObj(Vector3 Goal, GameObject target, float TimeToComplete = 1, int id = 0, float delay = 0, bool block = false, BaseAction.EaseType ease = BaseAction.EaseType.Linear)
    {
        actionList.AddTranslateAction(Goal, target, TimeToComplete, id, delay, block, ease);
    }

    public void RotateObj(Vector3 Goal, GameObject target, float TimeToComplete = 1, int id = 0, float delay = 0, bool block = false, BaseAction.EaseType ease = BaseAction.EaseType.Linear)
    {
        actionList.AddRotateAction(Goal, target, TimeToComplete, id, delay, block, ease);
    }

    public void ScaleObj(Vector3 Goal, GameObject target, float TimeToComplete = 1, int id = 0, float delay = 0, bool block = false, BaseAction.EaseType ease = BaseAction.EaseType.Linear)
    {
        actionList.AddScaleAction(Goal, target, TimeToComplete, id, delay, block, ease);
    }

    public void ShakeObj(GameObject target, float magnitude, float TimeToComplete = 1, int id = 0, float delay = 0, float opa = 0, bool block = false, BaseAction.EaseType ease = BaseAction.EaseType.Linear)
    {
        actionList.AddShakeAction(target, magnitude, TimeToComplete, id, delay, opa, block, ease);
    }

    public void CamVignetteAction(GameLogic mgr, Volume volume, float Duration, float vig, float ca, int id, float _delay = 0, bool block = false)
    {
        actionList.AddCamVigAction(mgr, volume, Duration, vig, ca, id, _delay, block);
    }

}
