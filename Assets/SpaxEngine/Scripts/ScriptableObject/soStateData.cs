using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Data
{

    [CreateAssetMenu(fileName = "StateData", menuName = "Statemachine/StateData", order = 1)]

    public class soStateData : ScriptableObject
    {
        [SerializeField] private StateType _type;
        [SerializeField] private soStateData _parentState;
        [SerializeField] private int _duration;
        [SerializeField] private StateConditions _stateConditions;
        [SerializeField] private StateType _cancels;
        [SerializeField] private TransitionEvents _enterEvents;
        [SerializeField] private TransitionEvents _exitEvents;
        [SerializeField] private List<TransitionData> _transitions;
        [SerializeField] private List<FrameData> _frames;
        [SerializeField] private List<AnimationFrameData> _animation;

        public StateType Type { get { return this._type; } }
        public TransitionEvents EnterEvents { get { return this._enterEvents; } }
        public TransitionEvents ExitEvents { get { return this._exitEvents; } }
        public soStateData ParentState { get { return this._parentState; } }
        public int Duration { get { return this._duration; } }
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

        public StateType CancelConditions
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

        public TransitionData CheckTransitions(TransitionFlags curFlags, StateType curCan, ResourceData curResources, InputItem[] playerInputs)
        {
            TransitionData ret = null;

            int i = 0;
            int len = this._transitions.Count;
            while (i < len)
            {
                var hold = this._transitions[i];
                var transCancels = hold.RequiredCancels;
                var transFlags = hold.RequiredTransitionFlags;
                var transRsrc = hold.RequiredResources;

                bool checkCancels = EnumHelper.HasEnum((uint)curCan, (uint)transCancels, true);
                bool checkFlags = checkCancels && EnumHelper.HasEnum((uint)curFlags, (uint)transFlags, true);
                bool checkResources = checkFlags && transRsrc.Check(curResources);
                bool checkInputs = checkResources && hold.CheckInputs(playerInputs);

                bool check = checkInputs;

                if (check)
                {
                    ret = hold;
                }

                i++;
            }

            return ret;
        }


    }
}