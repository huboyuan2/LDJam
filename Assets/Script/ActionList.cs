using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionList : MonoBehaviour
{
    public List<BaseAction> list;
    public BitArray Groups;
    public bool SpeedUp;
    public bool SpeedDown;
    public bool Paused = false;
    public float Slow = 0.5f; //how slow should the slowdown mode be
    public float FastForwardSpeed = 5f; //how fast should the speed up mode should be
    float SpeedModifier;
    public int NumberOfGroups = 5;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }
    public void Initialize()
    {
        if (list == null)
        {
            list = new List<BaseAction>();
        }
        if (Groups == null)
        {
            Groups = new BitArray(NumberOfGroups, false); ;
        }
    }
    public bool IsEmpty()
    {
        return (list.Count == 0);
    }
    public void FastFoward()
    {
        SpeedDown = false;
        SpeedUp = !SpeedUp;
    }
    //marks the list so it knows to slow down
    public void SlowDown()
    {
        SpeedUp = false;
        SpeedDown = !SpeedDown;
    }
    //reset the all action's speed to normal
    public void ResetSpeed()
    {
        SpeedUp = false;
        SpeedDown = false;
    }

    public void AddTranslateAction(Vector3 Goal, GameObject target, float TimeToComplete = 1, int id = 0, float delay = 0, bool block = false, BaseAction.EaseType ease = BaseAction.EaseType.Linear)
    {
        AddAction(new TranslateAction(Goal, target, id, TimeToComplete, delay, block, ease));
    }

    public void AddRotateAction(Vector3 Goal, GameObject target, float TimeToComplete = 1, int id = 0, float delay = 0, bool block = false, BaseAction.EaseType ease = BaseAction.EaseType.Linear)
    {
        AddAction(new RotateAction(Goal, target, id, TimeToComplete, delay, block, ease));
    }

    public void AddScaleAction(Vector3 Goal, GameObject target, float TimeToComplete = 1, int id = 0, float delay = 0, bool block = false, BaseAction.EaseType ease = BaseAction.EaseType.Linear)
    {
        AddAction(new RotateAction(Goal, target, id, TimeToComplete, delay, block, ease));
    }

    void AddAction(BaseAction action)
    {
        list.Add(action);
    }
    // Update is called once per frame
    void Update()
    {
        if (SpeedDown)
        {
            SpeedModifier = Slow;
        }
        else if (SpeedUp)
        {
            SpeedModifier = FastForwardSpeed;
        }
        else
        {
            SpeedModifier = 1;
        }
        if (!Paused)//nothing gets run if it's in pause
        {
            if (!IsEmpty())//run if list not empty
            {
                bool block = false;
                for (int i = 0; i < list.Count; ++i)
                {

                    if (block == true) //if there was a previous action that's blocking
                    {
                        if (Groups[list[i].groupID] == true) //check if this action belongs to the blocking action's group
                        {
                            continue;   //don't run if it does
                        }
                    }
                    if (list[i].Update(Time.deltaTime * SpeedModifier) == ActionResult.Success || list[i].Update(Time.deltaTime * SpeedModifier) == ActionResult.Failed)//remove the action if it's done or failed
                    {
                        if (list[i].Block) //unblock all the other actions in the same group
                        {
                            block = false;
                            Groups[list[i].groupID] = false;
                        }


                        list.Remove(list[i]); //decrement i
                        --i;
                    }
                    else
                    {
                        if (list[i].delay <= 0) //if it is not in delay
                        {
                            list[i].IncrementTime(Time.deltaTime * SpeedModifier);
                        }
                        if (list[i].Block == true)//if this action is blocking, then just return so the rest dont run the rest of actions
                        {
                            block = true;
                            Groups[list[i].groupID] = true;
                        }
                    }
                }
                Groups.SetAll(false); ; //reset the groups
            }
        }
    }
}

public class ScaleAction : BaseAction
{
    Vector3 StartScale;
    Vector3 EndScale;

    // Start is called before the first frame update
    public ScaleAction(Vector3 Goal, GameObject target, int id, float lifeSpan, float delay, bool block, EaseType ease)
    {
        EndScale = Goal;
        obj = target;
        LifeSpan = lifeSpan;
        Block = block;
        Easing = ease;
        groupID = id;
    }
    public override ActionResult Update(float dt)
    {
        if (!obj)
        {
            return ActionResult.Failed;
        }
        if (delay <= 0)
        {
            if (firstTime)
            {

                StartScale = obj.transform.localScale;


                firstTime = false;
            }
            Vector3 scale = StartScale + (EndScale - StartScale) * Percent;

            obj.transform.localScale = scale;
            if (Percent >= 1)
            {
                return ActionResult.Success;
            }
            else
            {
                return ActionResult.OnGoing;
            }
        }
        else
        {
            delay -= dt;
            return ActionResult.OnGoing;
        }
    }
}

public class RotateAction : BaseAction
{
    Vector3 StartAngle;
    Vector3 EndAngle;

    // Start is called before the first frame update
    public RotateAction(Vector3 Goal, GameObject target, int id, float lifeSpan, float delay, bool block, EaseType ease)
    {
        EndAngle = Goal;
        obj = target;
        LifeSpan = lifeSpan;
        Block = block;
        Easing = ease;
        groupID = id;
    }
    public override ActionResult Update(float dt)
    {
        if (!obj)
        {
            return ActionResult.Failed;
        }
        if (delay <= 0)
        {
            if (firstTime)
            {

                StartAngle = obj.transform.eulerAngles;


                firstTime = false;
            }
            Vector3 angle = StartAngle + (EndAngle - StartAngle) * Percent;

            obj.transform.rotation = Quaternion.Euler(angle);
            if (Percent >= 1)
            {
                return ActionResult.Success;
            }
            else
            {
                return ActionResult.OnGoing;
            }
        }
        else
        {
            delay -= dt;
            return ActionResult.OnGoing;
        }
    }
}

public class TranslateAction : BaseAction
{
    Vector3 StartPosition;
    Vector3 EndPosition;

    // Start is called before the first frame update
    public TranslateAction(Vector3 Goal, GameObject target, int id, float lifeSpan, float delay, bool block, EaseType ease)
    {
        EndPosition = Goal;
        obj = target;
        LifeSpan = lifeSpan;
        Block = block;
        Easing = ease;
        groupID = id;
    }
    public override ActionResult Update(float dt)
    {
        if (!obj)
        {
            return ActionResult.Failed;
        }
        if (delay <= 0)
        {
            if (firstTime)
            {

                StartPosition = obj.transform.position;


                firstTime = false;
            }
            Vector3 pos = StartPosition + (EndPosition - StartPosition) * Percent;

            obj.transform.position = pos;
            if (Percent >= 1)
            {
                return ActionResult.Success;
            }
            else
            {
                return ActionResult.OnGoing;
            }
        }
        else
        {
            delay -= dt;
            return ActionResult.OnGoing;
        }
    }
}

public enum ActionResult
{
    Invalid = -1,
    OnGoing,
    Blocking,
    Success,
    Failed
}

public abstract class BaseAction
{
    float Age; //how much time has passed
    public float LifeSpan; //how long this thing should live for
    public float Percent = 0;
    public float delay = 0;
    public bool Block = false; //if this is true, the action will block all subsequent actions on the list from running until it's done
    public GameObject obj;
    public EaseType Easing = EaseType.Linear;
    public int groupID;// the group this action belongs to, used for blocking
    public float OPA; //organic procedural adjustment
    protected bool firstTime = true;
    public string name;
    public enum EaseType
    {
        Linear,
        EaseIn,
        EaseOut,
        FastIn,
        FastOut,
        InAndOut,
        Bounce
    }
    public float Ease(float Percent, EaseType type) //adjust percent based on ease type
    {
        if (Percent <= 0)
        {
            return 0;
        }
        if (Percent >= 1)
        {
            return 1;
        }
        switch (type)
        {
            case EaseType.Linear: return Percent;
            case EaseType.EaseIn: return Mathf.Sqrt(Percent);
            case EaseType.EaseOut: return Mathf.Pow(Percent, 2.0f);
            case EaseType.FastIn: return Mathf.Sqrt(Mathf.Sqrt(Percent));
            case EaseType.FastOut: return Mathf.Pow(Percent, 4.0f);
            case EaseType.InAndOut:
                if (Percent < 0.5f)
                {
                    return Mathf.Pow(Percent * 2.0f, 2.0f) * 0.5f;
                }
                else
                {
                    return Mathf.Sqrt((Percent - 0.5f) * 2.0f) * 0.5f + 0.5f;
                }
            case EaseType.Bounce:
                float n = 7.5625f;
                float d = 2.75f;
                if (Percent < 1.0f / d)
                {
                    return n * Percent * Percent;
                }
                else if (Percent < 2.0f / d)
                {
                    return n * (Percent -= 1.5f / d) * Percent + 0.75f;
                }
                else if (Percent < 2.5f / d)
                {
                    return n * (Percent -= 2.25f / d) * Percent + 0.9375f;
                }
                else
                {
                    return n * (Percent -= 2.625f / d) * Percent + 0.984375f;
                }
        }
        return Percent;
    }
    public void IncrementTime(float dt)
    {
        Age += dt;
        if (Age >= LifeSpan) //if pass 1.f, set to 1
        {
            Age = LifeSpan;
            Percent = 1f;
        }
        else
        {
            Percent = Age / LifeSpan;
        }
        Percent = Ease(Percent, Easing);
    }
    // Update is called once per frame
    public abstract ActionResult Update(float dt);

    public void SetLifeSpan(float life)
    {
        LifeSpan = life;
        Percent = Age / LifeSpan;
    }

    //returns progress 
    public float PercentDone()
    {
        return Age / LifeSpan;
    }
    //how much time is left
    public float TimeLeft()
    {
        return LifeSpan - Age;
    }
}
