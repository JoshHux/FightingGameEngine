using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using FightingGameEngine.Enum;
using FightingGameEngine.Commands;

namespace FightingGameEngine.Data
{

    [CreateAssetMenu(fileName = "StateData", menuName = "Statemachine/StateData", order = 1)]

    public class soStateData : ScriptableObject
    {
        [SerializeField] private StateID _id;
        [SerializeField] private soStateData _parentState;

        [SerializeField] private bool inheritTrans_ = true;
        [SerializeField] private bool inheritCond_ = true;
        [SerializeField] private bool inheritCancel_ = true;

        [SerializeField] private int _duration;
        [SerializeField] private bool loop_ = false;
        [SerializeField] private StateConditions _stateConditions;
        [SerializeField] private StateConditions _toggleState;
        [SerializeField] private CancelConditions _cancels;
        [SerializeField] private CancelConditions _toggleCancel;
        [SerializeField] private TransitionEvents _enterEvents;
        [SerializeField] private TransitionEvents _exitEvents;
        [SerializeField] private List<TransitionData> _transitions;
        [SerializeField] private List<HitboxHolder> _hitboxSets;
        [SerializeField] private List<HurtboxHolder> _hurtboxSets;

        //we only want this to be here during development
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [TextArea(3, 10)]
        [SerializeField] private string _program;
#endif


        [SerializeField] private List<FrameData> _frames;
        [SerializeField] private List<AnimationFrameData> _animation;

        public StateID StateID { get { return this._id; } }
        public TransitionEvents EnterEvents { get { return this._enterEvents; } }
        public TransitionEvents ExitEvents { get { return this._exitEvents; } }
        public soStateData ParentState { get { return this._parentState; } }
        public int Duration { get { return this._duration; } }

        public List<TransitionData> Transitions
        {
            get
            {
                var ret = new List<TransitionData>(this._transitions);

                var getParent = this._parentState != null && this.inheritTrans_;
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
                bool canLookAtParent = this.inheritCond_;
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
                bool canLookAtParent = this.inheritCancel_;
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

        public bool GetLoop()
        {

            return this.loop_;
        }

#if UNITY_EDITOR
        public void SetStateID(int index, string charName)
        {

            this._id = new StateID();

            this._id.ID = StateID.FromString(charName);
            //Debug.Log(StateID.FromString(charName));
            this._id.SetIndex((long)index);
            if (charName == "UNIV") { this._id.SetAsUniversal(); }
            //Debug.Log(this._id.ID);

        }
#endif

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


        public HitboxHolder GetHitboxSet(int i)
        {
            var ret = this._hitboxSets[i];
            return ret;
        }

        public HurtboxHolder GetHurtboxSet(int i)
        {
            var ret = this._hurtboxSets[i];
            return ret;
        }

        public int get_hit_set_len() { return this._hitboxSets.Count; }
        public int get_hurt_set_len() { return this._hurtboxSets.Count; }


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Compile()
        {
            JackScriptCompiler.CompileString(this._program, this);
        }
#endif
    }
}