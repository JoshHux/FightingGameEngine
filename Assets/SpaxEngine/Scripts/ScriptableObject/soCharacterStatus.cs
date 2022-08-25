using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Data
{

    [CreateAssetMenu(fileName = "CharacterStatus", menuName = "Statemachine/CharacterStatus", order = 1)]


    public class soCharacterStatus : ScriptableObject
    {
        [SerializeField] private int _allegiance;
        [SerializeField] private int _playerId;
        [SerializeField] private ResourceData m_currentResources;
        [SerializeField] private FrameTimer m_stateTimer;
        [SerializeField] private FrameTimer m_stopTimer;
        [SerializeField] private FrameTimer m_superFlashTimer;
        [SerializeField] private ConditionTimer m_conditionTimer;
        [SerializeField] private soStateData m_currentState;
        [SerializeField] private StateConditions m_currentConditions;
        [SerializeField] private TransitionFlags m_transitionFlags;
        [SerializeField] private CancelConditions m_cancelFlags;
        [SerializeField] private int m_currentFacing;
        [SerializeField] private FVector2 m_currentPosition;
        //rigidbody velocity, listed seperately from calcvelocity to conserve built up momentum
        [SerializeField] private FVector2 m_currentVelocity;
        //velocity that is to be applied to the current velocity
        //druing SpaxUpdate, body velocity = currentvelocity + calcvelocity
        [SerializeField] private FVector2 m_calcVelocity;
        [SerializeField] private Fix64 m_currentGravity;
        [SerializeField] private Fix64 m_currentProration;
        [SerializeField] private int m_comboCount;

        //bounce data for ground and wall bounces
        [SerializeField] private int m_groundBounces;
        [SerializeField] private Fix64 m_groundBounceScaling;
        [SerializeField] private int m_wallBounces;
        [SerializeField] private Fix64 m_wallBounceScaling;

        [SerializeField] private InputRecorder _inputRecorder;
        [SerializeField] private FlatPhysics.FlatBody _positionAnchor;
        [SerializeField] private FVector2 _positionOffset;
        //whether or not we should check for a transition to a new state
        [SerializeField] private bool m_checkState;

        [SerializeField] private RendererInfo m_rendererInfo;

        public int Allegiance { get { return this._allegiance; } set { this._allegiance = value; } }
        public int PlayerID { get { return this._playerId; } }
        public int CurrentHP { get { return this.m_currentResources.Health; } set { this.m_currentResources.Health = value; } }
        public int CurrentFacingDirection { get { return this.m_currentFacing; } set { this.m_currentFacing = value; } }
        public FrameTimer StateTimer { get { return this.m_stateTimer; } set { this.m_stateTimer = value; } }
        public FrameTimer StopTimer { get { return this.m_stopTimer; } set { this.m_stopTimer = value; } }
        public FrameTimer SuperFlashTimer { get { return this.m_superFlashTimer; } set { this.m_superFlashTimer = value; } }
        public ConditionTimer ConditionTimer { get { return this.m_conditionTimer; } set { this.m_conditionTimer = value; } }
        public soStateData CurrentState { get { return this.m_currentState; } set { this.m_currentState = value; } }
        //the conditions we have, based on our current state
        public StateConditions CurrentStateConditions { get { return this.m_currentConditions; } set { this.m_currentConditions = value; } }
        //the conditions we process
        public StateConditions TotalStateConditions { get { return this.m_currentConditions | this.m_conditionTimer.StateConditions; } }
        public TransitionFlags TransitionFlags
        {
            get { return this.m_transitionFlags; }
            set
            {
                this.m_checkState = true;
                this.m_transitionFlags = value;
            }
        }
        public CancelConditions CancelFlags
        {
            get { return this.m_cancelFlags; }
            set
            {
                this.m_checkState = true;
                this.m_cancelFlags = value;
            }
        }
        public ResourceData CurrentResources
        {
            get { return this.m_currentResources; }
            set
            {
                this.m_checkState = true;
                this.m_currentResources = value;
            }
        }
        public FVector2 CurrentPosition { get { return this.m_currentPosition; } set { this.m_currentPosition = value; } }
        public FVector2 CurrentVelocity { get { return this.m_currentVelocity; } set { this.m_currentVelocity = value; } }
        public FVector2 CalcVelocity { get { return this.m_calcVelocity; } set { this.m_calcVelocity = value; } }
        public Fix64 CurrentGravity { get { return this.m_currentGravity; } set { this.m_currentGravity = value; } }
        public Fix64 CurrentProration { get { return this.m_currentProration; } set { this.m_currentProration = value; } }
        public int CurrentComboCount { get { return this.m_comboCount; } set { this.m_comboCount = value; } }

        //public access to bounce info
        public int GroundBounces { get { return this.m_groundBounces; } set { this.m_groundBounces = value; } }
        public Fix64 GroundBounceScaling { get { return this.m_groundBounceScaling; } set { this.m_groundBounceScaling = value; } }
        public int WallBounces { get { return this.m_wallBounces; } set { this.m_wallBounces = value; } }
        public Fix64 WallBounceScaling { get { return this.m_wallBounceScaling; } set { this.m_wallBounceScaling = value; } }


        //we determine whether we check the state here
        public bool CheckState { get { return this.m_checkState; } set { this.m_checkState = value; } }

        //anchor for our position when we are grabbed
        public FlatPhysics.FlatBody PositionAnchor { get { return this._positionAnchor; } }
        public FVector2 PositionOffset { get { return this._positionOffset; } }

        //for easy access from the InputRecorder object
        public InputItem CurrentControllerState { get { return this._inputRecorder.CurrentControllerState; } set { this._inputRecorder.CurrentControllerState = value; } }
        public InputItem[] Inputs { get { return this._inputRecorder.GetInputs(); } }

        public RendererInfo RendererInfo { get { return this.m_rendererInfo; } set { this.m_rendererInfo = value; } } 

        //gets the total velocity that will be assigned to the rigidbody
        public FVector2 TotalVelocity
        {
            get
            {
                var ret = this.m_currentVelocity + this.m_calcVelocity;
                return ret;
            }
        }

        //only really here to initialize the InputRecorder, at least for now
        public void Initialize() { this._inputRecorder = new InputRecorder(); }
        public void ResetLeniency() { this._inputRecorder.ResetLeniency(); }

        public void BufferInput(bool bufferLeniency)
        {
            this.m_checkState = this._inputRecorder.BufferInput(bufferLeniency);
        }
        public void SetAnchor(FlatPhysics.FlatBody newAnchor) { this._positionAnchor = newAnchor; }
        public void SetOffset(FVector2 newOffset) { this._positionOffset = newOffset; }
        public bool InHitstop { get { return !this.m_stopTimer.IsDone(); } }
        public bool IsAnchored() { return this._positionAnchor != null; }
    }
}