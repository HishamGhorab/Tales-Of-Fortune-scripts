using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New StateTime SO", menuName = "ServerScriptableObjects/CreateStateTime", order = 1)]
public class StateTimesScriptableObject : ScriptableObject
{
    public float planningPhase = 30;
    public float playingPhase = 12; //3 *4 
}
