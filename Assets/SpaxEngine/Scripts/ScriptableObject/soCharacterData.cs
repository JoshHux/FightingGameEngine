using UnityEngine;
using FixMath.NET;
namespace FightingGameEngine.Data
{

    [CreateAssetMenu(fileName = "CharacterData", menuName = "Statemachine/CharacterData", order = 1)]
    public class soCharacterData : ScriptableObject
    {

        [SerializeField] private int _maxHp;
        [SerializeField] private Fix64 _mass;
        [SerializeField] private Fix64 _friction;
        [SerializeField] private Fix64 _walkAccel;
        [SerializeField] private Fix64 _walkMaxSpd;
        [SerializeField] private Fix64 _runAccel;
        [SerializeField] private Fix64 _runMaxSpd;
        [SerializeField] private Fix64 _fallMaxSpd;
        [SerializeField] private soStateData[] _stateList;
        [SerializeField] private TransitionData[] _moveList;
        public int MaxHP { get { return this._maxHp; } }
        public Fix64 Mass { get { return this._mass; } }
        public Fix64 Friction { get { return this._friction; } }
        public Fix64 WalkAccel { get { return this._walkAccel; } }
        public Fix64 WalkMaxSpd { get { return this._walkMaxSpd; } }
        public Fix64 RunAccel { get { return this._runAccel; } }
        public Fix64 RunMaxSpd { get { return this._runMaxSpd; } }
        public Fix64 FallMaxSpd { get { return this._fallMaxSpd; } }
        public soStateData[] StateList { get { return this._stateList; } }
        public TransitionData[] MoveList { get { return this._moveList; } }
    }
}