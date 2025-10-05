using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ShowHint(true);
            //GameMgr.Instance.NextLevel();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Input.GetKeyDown(KeyCode.DownArrow))
        {
            //GameMgr.Instance.NextLevel();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ShowHint(false);
            //GameMgr.Instance.NextLevel();
        }
    }
    void ShowHint(bool show)
    {
        Debug.Log("ShowHint: " + show);
        // Implement hint display logic here
    }
}
