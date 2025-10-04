using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBlock : MonoBehaviour
{
    public List<TransformKeyframe> States;
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
        if (!list.IsEmpty() || States.Count == 0)
        {
            return;
        }
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
