using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTrap : MonoBehaviour
{
    public int index;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TrapModule tm = CollectableMgr.Instance.trapSO.trapModules[index];
            //CharacterModule.Instance.SkillRewind += cm.skillRewind;
            //CharacterModule.Instance.SkillDash += cm.skillDash;
            //CharacterModule.Instance.SkillStop += cm.skillStop;
            //CharacterModule.Instance.Health += 1;
            CharacterModule.Instance.TimeLeft += tm.time;
            if (tm.lethality)
            {
                CharacterCtrl.Instance.CallRebirth();
                //CharacterModule.Instance.Health -= 1;
                //if (CharacterModule.Instance.Health <= 0)
                //{
                //    // Trigger death sequence
                //    CharacterModule.Instance.Die();
                //}
            }
            if (tm.isProjectile)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
