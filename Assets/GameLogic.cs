using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    GameObject player;
    ActionList actionList;
    public enum TimeState
    {
        Reversing,
        Advancing
    }
    public TimeState current = TimeState.Reversing;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        actionList = GetComponent<ActionList>();
    }



    // Update is called once per frame
    void Update()
    {
        //just testing
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (current == TimeState.Reversing)
            {
                ////MoveObj(player.transform.position * 2, player, 1, 0, 0, false);
                //GameObject[] platforms = GameObject.FindGameObjectsWithTag("TimeManipulatedPlatform");
                //foreach (var item in platforms)
                //{
                //    item.GetComponent<TimeBlock>().Advance();
                //}
                current = TimeState.Advancing;
            }
            else if (current == TimeState.Advancing)
            {
                //GameObject[] platforms = GameObject.FindGameObjectsWithTag("TimeManipulatedPlatform");
                //foreach (var item in platforms)
                //{
                //    item.GetComponent<TimeBlock>().Revert();
                //}
                current = TimeState.Reversing;
            }
        }
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

}
