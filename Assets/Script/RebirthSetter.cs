using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebirthSetter : MonoBehaviour
{
    public Transform rebirthPoint;
    // Start is called before the first frame update

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CharacterCtrl.Instance.CurRebirthPlace = rebirthPoint.position;
            //gameObject.SetActive(false);
        }
    }
}
