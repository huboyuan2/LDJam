using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "NewCollectable", menuName = "ScriptableObjects/Collectables", order = 1)]
//public class CollectableSO : ScriptableObject
//{
//public CollectableModule[] collectableModules;
//}

[CreateAssetMenu(fileName = "NewTrap", menuName = "ScriptableObjects/Traps", order = 1)]
public class TrapSO : ScriptableObject
{
    public TrapModule[] trapModules;
}
//[CreateAssetMenu(fileName = "NewPortal", menuName = "ScriptableObjects/Portals", order = 1)]
//public class PortalSO : ScriptableObject
//{
//    public PortalModule[] portalModules;
//}
