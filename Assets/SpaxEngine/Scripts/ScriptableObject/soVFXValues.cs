using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Data
{

    [CreateAssetMenu(fileName = "VFXValues", menuName = "VFXValues", order = 1)]
    public class soVFXValues : ScriptableObject
    {
        [SerializeField] private List<GameObject> _vfxList;
        public  List<GameObject> VFXList { get { return this._vfxList; } }

    }
}