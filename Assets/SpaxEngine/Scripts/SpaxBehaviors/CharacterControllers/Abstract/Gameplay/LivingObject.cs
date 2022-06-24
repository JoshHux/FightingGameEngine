using UnityEngine;
using FixMath.NET;
using FlatPhysics.Unity;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Gameplay
{
    public abstract class LivingObject : GameplayBehavior
    {

        //rigidbody of the character
        private FBox _rb;
        //data of our character
        [SerializeField] private soCharacterData _data;
        //current status of this character
        [SerializeField] protected soCharacterStatus status;


        public soCharacterStatus Status { get { return this.status; } }

        protected override void OnAwake()
        {
            base.OnAwake();
            //initialize the timers
            this.status.StateTimer = new FrameTimer();
            this.status.StopTimer = new FrameTimer();

            //assign default values from character's data
            this.status.CurrentHP = this._data.MaxHP;
            this.status.CurrentGravity = this._data.Mass;
            this.status.CurrentFacingDirection = -1;
        }

        protected override void OnStart()
        {
            base.OnStart();
            //get the rigidbody of the character
            this._rb = this.GetComponent<FBox>();

            //get the default state
            var defaultState = this._data.StateList[0];
            //set the default state
            this.SetState(defaultState);
        }

        protected override void StateUpdate()
        {
            //tick the stop timer
            bool inHitstop = this.status.StopTimer.TickTimer();
            //if we're in hitstop, don't tick timer
            if (inHitstop) { return; }
            //Debug.Log("state updating - living object");

            //tick the state timer before anything else
            bool stateComplete = !this.status.StateTimer.TickTimer();

            //get the frame at this frame count, based on the timer's ticks + 1
            //the +1 is there so that we can have both at frame 0 to indicate an invalid item and for easy nomenclature for the designer
            int framesTicked = this.status.StateTimer.TimeElapsed + 1;

            //process our current state's state conditions
            StateConditions curCond = this.status.CurrentState.StateConditions;
            this.ProcessStateConditions(curCond);

            //the frame at the number or frames ticked
            FrameData frameAt = this.status.CurrentState.GetFrameAt(framesTicked);

            //if the frame is not null
            if (frameAt != null)
            {
                //process we found
                this.ProcessFrameData(frameAt);
            }

        }

        protected override void SpaxUpdate()
        {
            //if we're in hitstop, we con't want to assign velocity
            if (this.status.InHitstop) { return; }

            //correct the direction of CalcVelocity
            this.CorrectVelocityDirection();

            //apply the total velocity to the rigidbody
            this._rb.Velocity = this.status.CurrentVelocity + this.status.CalcVelocity;
            //set CalcVelocity to 0 to prevent any extra changes to velocity on the next frame
            this.status.CalcVelocity = new FVector2();

            //Debug.Log("SpaxUpdate, _rb velcoity :: (" + this._rb.Velocity.x + ", " + this._rb.Velocity.y + ")");
        }

        protected override void PostUpdate()
        {
            //record the current frame's position, to be used for the next frame
            this.status.CurrentPosition = this._rb.Body.Position;
            //record the current frame's velocity, to be used for the next frame
            this.status.CurrentVelocity = this._rb.Body.LinearVelocity;
        }


        //call to process transition event enums
        protected virtual void ProcessTransitionEvent(TransitionEvents te)
        {
            //int result for killing x velocity, 1 for has enum, 0 for doesn't have
            int killX = EnumHelper.HasEnumInt((uint)te, (uint)TransitionEvents.KILL_X_VEL);
            //int result for killing y velocity, 1 for has enum, 0 for doesn't have
            int killY = EnumHelper.HasEnumInt((uint)te, (uint)TransitionEvents.KILL_Y_VEL);

            //int multiplier to multiply the respective velocities
            //xor with 1 so that if we do want to kill velcity we multiply that velocity with 0, and 1 when we don't
            int multX = killX ^ 1;
            int multY = killY ^ 1;

            //multiply the respective multiplier with the respective velocities
            var curXVel = this.status.CurrentVelocity.x;
            var curYVel = this.status.CurrentVelocity.y;

            //actually do the multiplication now
            var newXVel = curXVel * multX;
            var newYVel = curYVel * multY;

            //make the new fvector2 to apply to the status
            var newVel = new FVector2(newXVel, newYVel);

            //reassign the mew velocity
            this.status.CurrentVelocity = newVel;

        }

        //call to process the frame data
        protected virtual void ProcessFrameData(FrameData frame)
        {
            /*----- PROCESSING APPLIED VELOCITY -----*/
            //get the velocity we want to apply
            var appliedVel = frame.AppliedVelocity;

            //do we want to hard set the velocity instead?
            bool setVel = frame.SetVelocity;
            //we want to set the velocity instead of adding it
            if (setVel)
            {
                //set the calc and current velcoties to 0, that way when we add it below, it sums to appliedVel
                this.status.CalcVelocity = new FVector2();
                this.status.CurrentVelocity = new FVector2();
            }

            //hold the CalcVelocity here for easy reference
            var holdVel = this.status.CalcVelocity;

            //add the applied velocity to the calc velocity
            this.status.CalcVelocity = holdVel + appliedVel;


            /*----- PROCESSING APPLIED GRAVITY -----*/
            bool changeGrav = frame.SetGravity;
            //we want to change gravity
            if (changeGrav) { this.status.CurrentGravity = frame.AppliedGravity; }


            /*----- PROCESSING RESOURCE CHANGE -----*/
            //resource change on this frame
            var rChange = frame.ResourceChange;
            //get current resources for easy reference
            var curR = this.status.CurrentResources;
            //add 'em up and we're done
            this.status.CurrentResources = curR + rChange;
        }

        //call to process the state conditions of our current state
        protected virtual void ProcessStateConditions(StateConditions stateConditions)
        {

            /*----- PROCESSING GRAVITY APPLICATION -----*/
            //we want to apply gravity?
            bool wantApplyGrav = EnumHelper.HasEnum((uint)stateConditions, (uint)StateConditions.APPLY_GRAVITY);
            //only apply gravity ONLY if WE ALSO don't have STALL_GRAVITY
            bool applyGrav = wantApplyGrav && !EnumHelper.HasEnum((uint)stateConditions, (uint)StateConditions.STALL_GRAVITY);

            //we want to apply gravity
            if (applyGrav)
            {
                //UnityEngine.Debug.Log("applying gravity");
                //grab the current gravity for easy reference
                var grav = this.status.CurrentGravity;

                //apply the change in velocity straight to the CurrentVelocity, directly effects rigidbody anyway
                //get easy access to the velocity to manipulate
                var gravVel = this.status.CurrentVelocity;
                //changed velcity to set back to Current Velocity
                var applyingVel = new FVector2(gravVel.x, gravVel.y - grav);

                //set to CurrentVelocity
                this.status.CurrentVelocity = applyingVel;
            }
        }

        //call to process try to transition the state
        protected virtual void TryTransitionState()
        {

        }

        //call to correct CalcVelocity's direction
        //  IE. if a 2d game, flip the x axis, if 3d, where the character is currently facing
        private void CorrectVelocityDirection()
        {
            //get the CalcVelocity here for easy reference
            var cVel = this.status.CalcVelocity;
            //get the facing direction for easy access
            var facingDir = this.status.CurrentFacingDirection;
            //change the cVel to newCalc with corrected velocity
            var newCalc = new FVector2(cVel.x * facingDir, cVel.y);
            //assign the new velocity
            this.status.CalcVelocity = newCalc;
        }

        protected void SetState(soStateData newState)
        {
            //set the new current state
            this.status.CurrentState = newState;
            //start the new state timer
            int stateDuration = this.status.CurrentState.Duration;
            this.status.StateTimer = new FrameTimer(stateDuration);
        }
    }
}