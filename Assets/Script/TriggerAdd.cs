using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            //CharacterModule.Instance.Health += 1;
            gameObject.SetActive(false);
        }
    }
}
