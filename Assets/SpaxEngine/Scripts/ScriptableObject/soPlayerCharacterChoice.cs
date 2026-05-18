using UnityEngine;

namespace FightingGameEngine.Gameplay
{
    [CreateAssetMenu(fileName = "soPlayerCharacterChoice", menuName = "Scriptable Objects/soPlayerCharacterChoice")]
    public class soPlayerCharacterChoice : ScriptableObject
    {
        [SerializeField] private GameObject _p1;
        [SerializeField] private GameObject _p2;

        [SerializeField] private controllers _p1ControlType;
        [SerializeField] private controllers _p2ControlType;
        public GameObject P1 { get { return this._p1; } set { this._p1 = value; } }
        public GameObject P2 { get { return this._p2; } set { this._p2 = value; } }
        public controllers P1ControlType { get { return this._p1ControlType; } set { this._p1ControlType = value; } }
        public controllers P2ControlType { get { return this._p2ControlType; } set { this._p2ControlType = value; } }
    }
}