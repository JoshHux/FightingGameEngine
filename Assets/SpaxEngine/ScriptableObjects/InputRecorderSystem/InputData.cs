using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Input Data", menuName = "Input Recorder")]
public class InputData : ScriptableObject
{
    public List<string> frameCommands;
}
