using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBlock : MonoBehaviour
{
    public enum Type
    {
        Freeform, //move according to keyframe
        Rotate //constantly spins around
    }
    public Type type = Type.Freeform;
    public List<TransformKeyframe> States;
    [Tooltip("Only used if type is rotate")]
    public Vector3 axis = new Vector3(0f, 1f, 0f); // Y axis
    [Tooltip("Only used if type is rotate")]
    public float degreesPerSecond = 90f;           // 90°/sec
    [Tooltip("Only used if type is rotate")]
    public Space space = Space.Self;               // Self or World

    [Serializable]
    public struct TransformKeyframe
    {
        public Vector3 position;
        public Vector3 scale;

        // Store Euler in degrees if you want nice XYZ fields in the Inspector
        public Vector3 eulerDegrees;

        //How long to get to this frame
        public float TimeToThisFrame;

        public BaseAction.EaseType easeRotate;
        public BaseAction.EaseType easeTranslate;
        public BaseAction.EaseType easeScale;

        // Convenience getter for runtime use
        public Quaternion Rotation => Quaternion.Euler(eulerDegrees);

    }
    int index = 0;
    ActionList list;
    GameLogic reference;
    // Start is called before the first frame update
    void Start()
    {
        list = GetComponent<ActionList>();
        reference = FindObjectOfType<GameLogic>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (reference.current == GameLogic.TimeState.Paused)
        {
            list.Paused = true;
        }
        else
        {
            list.Paused = false;
        }
        if (type == Type.Freeform && (!list.IsEmpty() || States.Count == 0 || reference.current == GameLogic.TimeState.Paused))
        {
            return;
        }
        if (reference.current != GameLogic.TimeState.Paused && type == Type.Rotate)
        {
            transform.Rotate(degreesPerSecond * Time.deltaTime * axis, space);
        }
        else
        {
            if (reference.current == GameLogic.TimeState.Advancing)
            {

                if (index < States.Count - 1)
                {
                    ++index;
                    list.AddTranslateAction(States[index].position, this.gameObject, States[index].TimeToThisFrame, 0, 0, false, States[index].easeTranslate);
                    list.AddRotateAction(States[index].eulerDegrees, this.gameObject, States[index].TimeToThisFrame, 0, 0, false, States[index].easeRotate);
                    list.AddScaleAction(States[index].scale, this.gameObject, States[index].TimeToThisFrame, 0, 0, false, States[index].easeScale);

                }

            }
            else if (reference.current == GameLogic.TimeState.Reversing)
            {

                if (index > 0)
                {
                    --index;
                    list.AddTranslateAction(States[index].position, this.gameObject, States[index].TimeToThisFrame, 0, 0, false, States[index].easeTranslate);
                    list.AddRotateAction(States[index].eulerDegrees, this.gameObject, States[index].TimeToThisFrame, 0, 0, false, States[index].easeRotate);
                    list.AddScaleAction(States[index].scale, this.gameObject, States[index].TimeToThisFrame, 0, 0, false, States[index].easeScale);
                }
            }
        }
    }
}
