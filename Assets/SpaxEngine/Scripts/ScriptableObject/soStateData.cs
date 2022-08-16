using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Data
{

    [CreateAssetMenu(fileName = "StateData", menuName = "Statemachine/StateData", order = 1)]

    public class soStateData : ScriptableObject
    {
        [SerializeField] private soStateData _parentState;
        [SerializeField] private int _duration;
        [SerializeField] private StateConditions _stateConditions;
        [SerializeField] private StateConditions _toggleState;
        [SerializeField] private CancelConditions _cancels;
        [SerializeField] private CancelConditions _toggleCancel;
        [SerializeField] private TransitionEvents _enterEvents;
        [SerializeField] private TransitionEvents _exitEvents;
        [SerializeField] private List<TransitionData> _transitions;
        [SerializeField] private List<FrameData> _frames;
        [SerializeField] private List<AnimationFrameData> _animation;

        public TransitionEvents EnterEvents { get { return this._enterEvents; } }
        public TransitionEvents ExitEvents { get { return this._exitEvents; } }
        public soStateData ParentState { get { return this._parentState; } }
        public int Duration { get { return this._duration; } }

        public List<TransitionData> Transitions
        {
            get
            {
                var ret = new List<TransitionData>(this._transitions);

                var getParent = this._parentState != null && !EnumHelper.HasEnum((uint)this.StateConditions, (uint)StateConditions.NO_PARENT_TRANS);
                if (getParent)
                {
                    ret.AddRange(this._parentState.Transitions);
                }

                return ret;
            }
        }
        public StateConditions StateConditions
        {
            get
            {
                var ret = this._stateConditions;
                //do we look at parent conditions?
                bool canLookAtParent = !EnumHelper.HasEnum((uint)ret, (uint)StateConditions.NO_PARENT_COND);
                //if we do, do we have a parent?
                bool getParentCond = canLookAtParent && (this._parentState != null);

                if (getParentCond)
                {
                    //get the parent conditions to add
                    var additions = this._parentState.StateConditions;
                    ret |= additions;
                }

                return ret;
            }
        }

        public CancelConditions CancelConditions
        {
            get
            {
                var ret = this._cancels;
                //do we look at parent conditions?
                bool canLookAtParent = !EnumHelper.HasEnum((uint)ret, (uint)StateConditions.NO_PARENT_CANCEL);
                //if we do, do we have a parent?
                bool getParentCond = canLookAtParent && (this._parentState != null);

                if (getParentCond)
                {
                    //get the parent conditions to add
                    var additions = this._parentState.CancelConditions;
                    ret |= additions;
                }

                return ret;
            }
        }

        //we have a parent state only if we aren't our parent's state
        public bool HasParent()
        {
            bool ret = this._parentState != null;

            return ret;
        }

        public FrameData GetFrameAt(int f)
        {
            var ret = this._frames.Find(v => v.AtFrame == f);

            return ret;
        }

        public AnimationFrameData GetAnimationAt(int f)
        {
            var ret = this._animation.Find(v => v.AtFrame == f);

            return ret;
        }

        public TransitionData CheckTransitions(TransitionFlags curFlags, CancelConditions curCan, ResourceData curResources, InputItem[] playerInputs, int facingDir, Fix64 yVel, Fix64 yPos)
        {
            TransitionData ret = null;
            /*
                        int i = 0;
                        int len = this._transitions.Count;
                        while (i < len)
                        {
                            var hold = this._transitions[i];
                            bool check = hold.CheckTransition(curFlags, curCan, curResources, playerInputs, facingDir);


                            //if (EnumHelper.HasEnum((uint)transFlags, (uint)TransitionFlags.GROUNDED, true))
                            //    Debug.Log(i + " " + checkCancels + " " + checkFlags + " " + checkResources + " " + checkInputs);

                            if (check)
                            {
                                ret = hold;
                                return ret;

                            }

                            i++;
                        }
            */
            ret = this.Transitions.Find(hold => hold.CheckTransition(curFlags, curCan, curResources, playerInputs, facingDir, yVel, yPos));
            return ret;
        }


    }
}