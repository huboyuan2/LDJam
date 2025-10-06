using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TriggerAdd : MonoBehaviour
{
    public int index;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectableModule cm = CollectableMgr.Instance.collectableSO.collectableModules[index];
            CharacterModule.Instance.SkillRewind += cm.skillRewind;
            CharacterModule.Instance.SkillDash += cm.skillDash;
            CharacterModule.Instance.SkillStop += cm.skillStop;
            CharacterModule.Instance.TimeLeft += cm.time;
            //CharacterModule.Instance.Health += 1;
            if (cm.isRenewable)
            {
                // Handle renewable collectable
                MoveAway();
                Invoke("MoveBack", cm.coolDown);
            }
            else
                gameObject.SetActive(false);
        }
    }
    void MoveAway()
    {
        //Debug.LogError("MoveAway");
        gameObject.SetActive(false);
        //transform.Translate(new Vector3(100, 100, 0));
    }
    void MoveBack()
    {
        //Debug.LogError("MoveBack");
        gameObject.SetActive(true);
        //transform.Translate(new Vector3(-100, -100, 0));
    }
}
