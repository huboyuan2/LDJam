using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    GameObject player;
    ActionList actionList;

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
            actionList.AddTranslateAction(player.transform.position * 2, player, 0, 1, 0, false);
            
        }
    }
}
