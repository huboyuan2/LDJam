using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableMgr : MonoBehaviour
{
    public CollectableSO collectableSO;
    public TrapSO trapSO;
    public PortalSO portalSO;
    private static CollectableMgr _instance;

    public static CollectableMgr Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CollectableMgr>();
            }
            return _instance;
        } 
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
