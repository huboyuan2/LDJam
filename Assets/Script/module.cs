using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollectableModule
{
    public string moduleName;
    public int moduleID;
    public float time;
    public int skillRewind;
    public int skillDash;
    public int skillStop;
    public bool isRenewable = false;
    public float coolDown = 3f;
}
[System.Serializable]
public class  TrapModule
{
    public string name;
    public float time;
    public bool lethality;
    public bool isProjectile;
}
[System.Serializable]
public class PortalModule
{
    public string name;
    public float time;
    public int destinationID;
}