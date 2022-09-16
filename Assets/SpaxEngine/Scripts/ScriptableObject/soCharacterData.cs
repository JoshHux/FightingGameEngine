using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Data
{

    [CreateAssetMenu(fileName = "CharacterData", menuName = "Statemachine/CharacterData", order = 1)]
    public class soCharacterData : ScriptableObject
    {

        [SerializeField] private string _charName = "AAAA";
        [SerializeField] private ResourceData _startingResources;
        [SerializeField] private ResourceData _maxResources;
        [SerializeField] private Fix64 _mass;
        [SerializeField] private Fix64 _juggleMass;
        [SerializeField] private Fix64 _friction;
        [SerializeField] private Fix64 _walkAccel;
        [SerializeField] private Fix64 _walkMaxSpd;
        [SerializeField] private Fix64 _runAccel;
        [SerializeField] private Fix64 _runMaxSpd;
        [SerializeField] private Fix64 _fallMaxSpd;
        [SerializeField] private soVFXValues _vfxValues;
        [SerializeField] private soStateData[] _stateList;
        [SerializeField] private List<TransitionData> _moveList;
        public Fix64 Mass { get { return this._mass; } }
        public Fix64 JuggleMass { get { return this._juggleMass; } }
        public Fix64 Friction { get { return this._friction; } }
        public Fix64 WalkAccel { get { return this._walkAccel; } }
        public Fix64 WalkMaxSpd { get { return this._walkMaxSpd; } }
        public Fix64 RunAccel { get { return this._runAccel; } }
        public Fix64 RunMaxSpd { get { return this._runMaxSpd; } }
        public Fix64 FallMaxSpd { get { return this._fallMaxSpd; } }
        public soVFXValues VFXValues { get { return this._vfxValues; } }
        public ResourceData StartingResources { get { return this._startingResources; } }
        public ResourceData MaxResources { get { return this._maxResources; } }
        public soStateData[] StateList { get { return this._stateList; } }
        public TransitionData[] MoveList { get { return this._moveList.ToArray(); } }


#if UNITY_EDITOR
        //convenience function to automatically set the state id's
        public void AssignStateID()
        {
            int i = 0;
            int len = this._stateList.Length;

            while (i < len)
            {
                var hold = this._stateList[i];

                hold.SetStateID(i, this._charName);

                //var name = StateID.CharString(hold.StateID);
                //var index = hold.StateID.GetIndex();

                //Debug.Log(i + " :: " + "index = " + index + " | name = " + name + " | long ID = " + hold.StateID.ID);

                i++;
            }
        }
#endif

        public TransitionData CheckMoveList(TransitionFlags curFlags, CancelConditions curCan, ResourceData curResources, InputItem[] playerInputs, int facingDir, Fix64 yVel, Fix64 yPos)
        {
            TransitionData ret = null;
            //Debug.Log("checking movelist");
            /* int i = 0;
             int len = this._moveList.Length;
             while (i < len)
             {
                 var hold = this._moveList[i];

                 bool check = hold.CheckTransition(curFlags, curCan, curResources, playerInputs);

                 //if (i == 1)
                 //{
                 //Debug.Log(i + " " + checkCancels + " " + checkFlags + " " + checkResources + " " + checkInputs);
                 //}

                 if (check)
                 {
                     ret = hold;
                     //Debug.Log("passed transition to " + hold.TargetState.name);
                     break;
                 }

                 i++;
             }*/
            ret = this._moveList.Find(hold => hold.CheckTransition(curFlags, curCan, curResources, playerInputs, facingDir, yVel, yPos));

            //if (ret != null && ret.TargetState != null && ret.TargetState.name == "Grab-Back") { Debug.Log("found it"); }

            return ret;
        }

        public soStateData GetStateFromID(StateID id)
        {
            int i = 0;
            int len = this._stateList.Length;

            while (i < len)
            {
                var hold = this._stateList[i];

                if (hold.StateID == id) { return hold; }
                i++;
            }

            return null;
        }

    }
}