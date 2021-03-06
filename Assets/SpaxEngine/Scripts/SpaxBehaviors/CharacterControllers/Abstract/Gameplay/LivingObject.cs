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
        [SerializeField] protected soCharacterData data;
        //current status of this character
        [SerializeField] protected soCharacterStatus status;


        public soCharacterStatus Status { get { return this.status; } }

        protected override void OnAwake()
        {
            base.OnAwake();
            //initialize the timers
            this.status.StateTimer = new FrameTimer();
            this.status.StopTimer = new FrameTimer();

            //set default data for the status
            this.status.CalcVelocity = new FVector2();
            this.status.CurrentVelocity = new FVector2();


            //assign default values from character's data
            this.status.CurrentHP = this.data.MaxHP;
            this.status.CurrentGravity = this.data.Mass;
            this.status.CurrentFacingDirection = -1;
        }

        protected override void OnStart()
        {
            base.OnStart();
            //get the rigidbody of the character
            this._rb = this.GetComponent<FBox>();

            //get the default state
            var defaultState = this.data.StateList[0];
            //set the default state
            this.SetState(defaultState);
        }

        protected override void StateUpdate()
        {
            //Debug.Log("state updating - living object");




            //try to transition to a new state
            this.TryTransitionState();


            //tick the stop timer
            bool inHitstop = this.status.StopTimer.TickTimer();
            //if we're in hitstop, don't tick timer
            if (inHitstop) { return; }

            //tick the state timer after possible new state assignment anything else
            bool stateComplete = !this.status.StateTimer.TickTimer();

            if (stateComplete)
            {
                //if the state is done, timer wise
                //  add the state completed transition flag
                this.status.TransitionFlags = this.status.TransitionFlags | TransitionFlags.STATE_END;
            }

            //get the frame at this frame count, based on the timer's ticks
            int framesTicked = this.status.StateTimer.TimeElapsed;

            //the frame at the number or frames ticked
            FrameData frameAt = this.status.CurrentState.GetFrameAt(framesTicked);

            //if the frame is not null
            if (frameAt != null)
            {
                //process we found
                this.ProcessFrameData(frameAt);
            }

        }

        //int i = 0;
        //prep the rigidbody to be updated, make sure it's got the right velocity etc.
        protected override void SpaxUpdate()
        {
            //if we're in hitstop, we con't want to assign velocity
            if (this.status.InHitstop) { return; }

            //process our current state's state conditions
            StateConditions curCond = this.status.CurrentState.StateConditions;
            this.ProcessStateConditions(curCond);

            //correct the direction of CalcVelocity
            //we do this before assigning velocity to the rigidbody so we have more control over
            //  which velocity(ies) get their direction changed
            this.CorrectVelocityDirection();
            //Debug.Log("spaxupdate - " +this.status.TotalVelocity.x+" | calc - "+this.status.CalcVelocity.x+" | cur - "+this.status.CurrentVelocity.x);

            //apply the total velocity to the rigidbody
            this._rb.Velocity = this.status.TotalVelocity;
            //set CalcVelocity to 0 to prevent any extra changes to velocity on the next frame
            this.status.CalcVelocity = new FVector2();

            //i++;
            //Debug.Log("SpaxUpdate, _rb velocity :: (" + this._rb.Velocity.x + ", " + this._rb.Velocity.y + ")");
            //Debug.Log("spaxupdate - " + this.name + " - " + i);
        }

        protected override void PostUpdate()
        {
            //record the current frame's position, to be used for the next frame
            this.status.CurrentPosition = this._rb.Body.Position;


            //if we're in hitstop, we con't want to assign velocity
            if (this.status.InHitstop) { return; }

            //record the current frame's velocity, to be used for the next frame
            this.status.CurrentVelocity = this._rb.Body.LinearVelocity;
            //Debug.Log("-PostUpdate, _rb velocity :: (" + this._rb.Velocity.x + ", " + this._rb.Velocity.y + ")");

            //simple check for grounded or not
            if ((this.status.CurrentPosition.y >= -3) || (this.status.CurrentVelocity.y > 0))
            {
                //make status think it's airborne
                //Debug.Log("becoming airborne " + (this.status.CurrentPosition.y < -3) + " " + (this.status.CurrentVelocity.y > 0));
                this.status.TransitionFlags = (~TransitionFlags.GROUNDED) & (this.status.TransitionFlags | TransitionFlags.AIRBORNE);
            }
            else
            {
                //make character think it's grounded
                //Debug.Log("becoming grounded ");
                this.status.TransitionFlags = (~TransitionFlags.AIRBORNE) & (this.status.TransitionFlags | TransitionFlags.GROUNDED);

            }
            //Debug.Log("postupdate - " + this.name + " - " + i);

        }

        //call to process transition data 
        protected virtual void ProcessTransitionData(TransitionData trans)
        {
            //state to transition to
            var targetState = trans.TargetState;

            //if it's null, go to default state
            if (targetState == null)
            {
                //are we airborne right now?
                bool airborne = EnumHelper.HasEnum((uint)this.status.TransitionFlags, (uint)TransitionFlags.AIRBORNE);

                //index of default state 0 for grounded, 1 for airborne
                int ind = (airborne) ? 1 : 0;
                //do we have a valid index to transition to?
                bool validIndex = trans.TargetStateIndex > 1;
                if (validIndex)
                {
                    ind = trans.TargetStateIndex;
                }

                //Debug.Log(ind);

                //set the state we want to transtion to
                targetState = this.data.StateList[ind];

            }
            //set the new state
            this.SetState(targetState);
            //process any transition events
            this.ProcessTransitionEvent(trans.TransitionEvents);

            //check if it's a nodestate, if it is, try to transition out of it
            if (targetState.Duration < 0)
            {
                this.TryTransitionState();
            }

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

            this.status.ResetLeniency();

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

            //if (appliedVel.magnitude > 0) { Debug.Log("applying velocity"); }

            //hold the CalcVelocity here for easy reference
            var holdVel = this.status.CalcVelocity;

            //add the applied velocity to the calc velocity
            this.status.CalcVelocity = holdVel + appliedVel;


            /*----- PROCESSING APPLIED GRAVITY -----*/
            bool changeGrav = frame.SetGravity;
            //we want to change gravity
            if (changeGrav) { this.status.CurrentGravity = frame.AppliedGravity; }

            //if (this.status.CalcVelocity.magnitude > 0) { Debug.Log(this.status.CurrentState.name + " - " + this.status.StateTimer.TimeElapsed + " - " + frame.AtFrame + " - (" + this.status.CalcVelocity.x + ", " + this.status.CalcVelocity.y + ")"); }


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

                var applyingVel = new FVector2(0, -grav);

                //set to CurrentVelocity
                this.status.CalcVelocity += applyingVel;
            }



            /*----- PROCESSING FRICTION APPLICATION -----*/
            //current velocity values
            var totalVel = this.status.TotalVelocity;
            var xVel = totalVel.x;
            var yVel = totalVel.y;

            //we want to apply friction?
            bool wantApplyFric = EnumHelper.HasEnum((uint)stateConditions, (uint)StateConditions.APPLY_FRICTION);
            //can we move?
            bool canMove = EnumHelper.HasEnum((uint)stateConditions, (uint)StateConditions.CAN_MOVE);
            //want to move?
            bool wantToMove = canMove && this.status.CurrentControllerState.WantsToMove();
            //wanting to move will override friction application, BUT if the current velocity along the x-axis is larger
            //      than the assign max move speed, then we will still apply fricion
            //      the want to move thing applied later only is applied IF velocity along the x-axis is lower than the top move speed
            //      if the friction applied brings it below that top move speed, then the player wanting to move will do math to bring x velocity up to that max movement speed
            bool fasterThanMax = Fix64.FastAbs(this.status.TotalVelocity.x) > this.data.WalkMaxSpd;

            //is the x velocity non-zero?
            bool xVelCheck = wantApplyFric && (xVel != 0);
            //only apply gravity ONLY if WE ALSO grounded, AND we DON'T want to move
            bool applyFriction = xVelCheck && ((!wantToMove) || fasterThanMax) && EnumHelper.HasEnum((uint)this.status.TransitionFlags, (uint)StateConditions.GROUNDED);

            if (applyFriction)
            {
                var friction = this.data.Friction;


                //get the sign of the x velocity
                var absXVel = Fix64.FastAbs(xVel);
                var signXVel = xVel / absXVel;

                //amount of friction to apply
                var frictAmt = friction;

                //if we would apply too much friction, just have the amount of friction applies be equal to our velocity
                if (friction > absXVel) { frictAmt = absXVel; }

                //friction w/ direction 
                var appliedFrict = frictAmt * signXVel * this.status.CurrentFacingDirection * -1;

                var applyingFriction = new FVector2(appliedFrict, 0);

                this.status.CalcVelocity += applyingFriction;

                //Debug.Log("applying friction - " + appliedFrict + " - " + this.status.CalcVelocity.x + " - " + this.status.TotalVelocity.x);

            }
        }

        //call to process try to transition the state
        protected void TryTransitionState()
        {
            var trnFlags = this.status.TransitionFlags;
            var curCan = this.status.CancelFlags;
            var curRsrc = this.status.CurrentResources;
            var curInpt = this.status.Inputs;
            var curFacing = this.status.CurrentFacingDirection;
            var trans = this.status.CurrentState.CheckTransitions(trnFlags, curCan, curRsrc, curInpt, curFacing);


            //don't check the state for transitions anymore
            this.status.CheckState = false;
            //if it's null, no transition to process
            if (trans == null)
            {
                //check the move list in the data instead
                trans = this.data.CheckMoveList(trnFlags, curCan, curRsrc, curInpt, curFacing);

                //if (trans == null)
                //{
                //Debug.Log(curInpt[1].Input + " " + curInpt[1].Flags + " " + (curInpt[1].HoldDuration < Spax.SpaxManager.Instance.StaticValues.InputLeniency));
                //}

                //if trans is still null, we end the check
                if (trans == null) { /*Debug.Log("failed to find transition");*/ return; }

                //Debug.Log("found transition in movelist - " + trans.TargetState.name);
            }
            //Debug.Log("found transition to - " + trans.TargetState.name);

            this.ProcessTransitionData(trans);

        }

        protected void TryTransitionUniversalState(int universalStateInd = -1)
        {

            var trnFlags = this.status.TransitionFlags;
            var curCan = this.status.CancelFlags;
            var curRsrc = this.status.CurrentResources;
            var curInpt = this.status.Inputs;
            var curFacing = this.status.CurrentFacingDirection;

            var universalStateList = Spax.SpaxManager.Instance.UniversalStates;

            var trans = universalStateList.CheckTransition(universalStateInd, trnFlags, curCan, curRsrc, curInpt, curFacing);

            if (trans == null) { return; }

            this.ProcessTransitionData(trans);
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
            //assign the current cancel conditions
            this.status.CancelFlags = newState.CancelConditions;
            //remove the state end transition flag
            this.status.TransitionFlags = this.status.TransitionFlags & ((TransitionFlags)~TransitionFlags.STATE_END);
        }

        protected void SetStopTimer(int newDur)
        {

            int potenHitstop = newDur;

            //is the hitstop timer currently ticking?
            bool hitstopTicking = !this.status.StopTimer.IsDone();
            //if the timer is still ticking, go with the higher hitstop value
            bool newHitstopIsLarger = hitstopTicking && (potenHitstop > this.status.StopTimer.EndTime);
            //restart the timer with new hitstop
            bool restartStopTimer = (!hitstopTicking) || newHitstopIsLarger;

            if (restartStopTimer)
            {
                //Debug.Log("starting stop timer in - " + this.name + " for " + potenHitstop + " frames long");
                this.status.StopTimer = new FrameTimer(potenHitstop);

                //should freeze the rigidbody's velocity
                this._rb.Body.LinearVelocity = new FVector2();
            }


        }
    }
}