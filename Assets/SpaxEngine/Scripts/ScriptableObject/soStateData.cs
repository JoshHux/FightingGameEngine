using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        [SerializeField] private string _script;
        private string _savedScript;
#endif


        private List<FrameData> _frames;
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

#if UNITY_EDITOR
        //cleaning up bad state frames
        void OnValidate()
        {/*
            //we're ending with the actual count of the frames becuase we want to change the length of the list, so we want to end with the length
            for (int i = 0; i < this._frames.Count; i++)
            {
                if (this._frames[i] == null)
                {
                    this._frames.RemoveAt(i);
                    //we decrement here to check the frame that replaces the one we removed
                    i--;
                }
            }*/
        }
#endif


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

            if (this._frames == null) { return null; }
            //Debug.Log(this.name + " | " + this._frames.Count);
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

        public void set_frames(List<FrameData> f) { this._frames = f; }

        public int get_hit_set_len() { return this._hitboxSets.Count; }
        public int get_hurt_set_len() { return this._hurtboxSets.Count; }


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Compile()
        {
            //check the current script against the saved one, see if there's a difference
            //only want to check against non-whitespaces
            var cur = Regex.Replace(this._script, @"\t|\n|\r|\r\n", "");
            var save = Regex.Replace(this._savedScript, @"\t|\n|\r|\r\n", "");
            //don't want to waste time recomiling the same script
            if (cur == save)
            {
                Debug.Log("nothing different!");
                //return;
            }

            //nothing to compile, clear out frames list
            if (cur == "")
            {
                this._frames = new List<FrameData>();
                return;
            }

            var frames = new List<FrameData>();

            try
            {
                frames = JackScriptCompiler.CompileString(this._script, this);
            }
            catch (Exception)
            {
                Debug.LogError(this.name + " | There was a compile error!");
                throw;
            }

            this._frames = new List<FrameData>();
            //we got the frames, now just add them to our dictionary (if it doesn't already exist)
            int i = 0;
            int len = frames.Count;

            //if we don't have any frames, clear the dictionary
            if (len == 0)
            {
                this._frames.Clear();
                return;
            }

            while (i < len)
            {
                var frame = frames[i];

                var hold = this._frames.Find(v => v.AtFrame == frame.AtFrame);

                this._frames.Add(frame);


                //Debug.Log(this._frames[this._frames.IndexOf(frame)].get_events().Count);

                i++;
            }
            //it's different, save the new one
            this._savedScript = this._script;


        }
#endif
    }
}