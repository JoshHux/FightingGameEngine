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

        public soCharacterData Data { get { return this.data; } }
        public soCharacterStatus Status { get { return this.status; } }
        public FlatPhysics.FlatBody Body { get { return this._rb.Body; } }
        public FVector2 FlatPosition { get { return this._rb.Position; } }

        //call to reset this character's status
        public virtual void ResetStatus()
        {
            //this.gameObject.name += this.gameObject.transform.position.x;
            //initialize the timers
            this.status.StateTimer = new FrameTimer();
            this.status.StopTimer = new FrameTimer();
            this.status.SuperFlashTimer = new FrameTimer();
            this.status.InstallTimer = new FrameTimer();
            this.status.ConditionTimer = new ConditionTimer();

            //set default data for the status
            this.status.CalcVelocity = new FVector2();
            this.status.CurrentVelocity = new FVector2();


            //assign default values from character's data
            this.status.CurrentHP = this.data.MaxResources.Health;
            this.status.CurrentGravity = this.data.Mass;
            this.status.GravityScaling = 1;
            this.status.CurrentFacingDirection = -1;

            //default values for transition flags
            this.status.TransitionFlags = 0;


            //default values for the extra transition info
            this.status.SetTransitionInfoVal(0, -1);
            this.status.SetTransitionInfoVal(1, -1);
            this.status.SetTransitionInfoVal(2, -1);
            this.status.SetTransitionInfoVal(3, -1);
            this.status.SetTransitionInfoVal(4, -1);
            this.status.SetTransitionInfoVal(5, -1);
            this.status.SetTransitionInfoVal(6, -1);
            this.status.SetTransitionInfoVal(7, -1);

            //get the default state
            var defaultState = this.data.StateList[0];
            //set the default state
            this.SetStateRaw(defaultState);

            Debug.Log(this.gameObject.name + " " + this.status.StopTimer.EndTime);

            //set starting resources
            this.status.CurrentResources = this.data.StartingResources;
            //print(this.status.PlayerID);

        }


        protected override void OnAwake()
        {
            base.OnAwake();
            this.ResetStatus();
        }

        protected override void OnStart()
        {
            base.OnStart();
            //get the rigidbody of the character
            this._rb = this.GetComponent<FBox>();

            //get the default state
            var defaultState = this.data.StateList[0];
            //set the default state
            this.SetStateRaw(defaultState);

            //set starting resources
            this.status.CurrentResources = this.data.StartingResources;

        }

        protected override void StateUpdate()
        {
            //Debug.Log("state updating - living object");




            //tick superflash timer
            var foo = !this.status.SuperFlashTimer.IsDone() && this.TickSuperFlash();

            //tick the stop timer
            bool inHitstop = this.status.StopTimer.TickTimer();
            //if we're in hitstop, don't tick timer
            if (inHitstop)
            {
                //if we're in hitstop, let's see if we need to transition to something like a hitstun state
                //try transitioning to a universal state
                var univTargetState = this.status.TransitionInfo.GetValue(0);
                var totalStun = this.status.TransitionInfo.GetValue(1);
                //Debug.Log(this.gameObject.name + " second check :: " + ((this.status.TransitionFlags & TransitionFlags.UNBLOCKED_HIT) > 0) + " " + univTargetState + " " + totalStun);
                if (univTargetState > -1 && !(univTargetState == 0 && totalStun <= 0))
                {
                    this.TryTransitionUniversalState(univTargetState);
                    //set stun duration
                    int sttDur = (this.status.CurrentState.Duration == 0) ? totalStun : this.status.CurrentState.Duration;
                    this.Status.StateTimer = new FrameTimer(sttDur);
                    //Debug.Log(this.status.CurrentState + " " + totalStun + " " + sttDur);
                }
                return;
            }
            //try to transition to a new state
            this.TryTransitionState();


            //tick the state timer after possible new state assignment anything else
            bool stateComplete = !this.status.StateTimer.TickTimer();
            //tick the condition timer, the persistent condition will possibly reset on the frame we reach
            this.status.ConditionTimer.TickTimer();

            //if (this.status.CurrentState.name == "Jab") { Debug.Log("state timer in StateUpdate is - " + stateComplete + " - " + this.status.StateTimer.TimeElapsed + "/" + this.status.StateTimer.EndTime); }

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
            StateConditions curCond = this.status.TotalStateConditions;
            this.ProcessStateConditions(curCond);

            //correct the direction of CalcVelocity
            //we do this before assigning velocity to the rigidbody so we have more control over
            //  which velocity(ies) get their direction changed
            this.CorrectVelocityDirection();
            //Debug.Log("spaxupdate - " +this.status.TotalVelocity.x+" | calc - "+this.status.CalcVelocity.x+" | cur - "+this.status.CurrentVelocity.x);

            //apply the total velocity to the rigidbody
            this._rb.Velocity = this.status.TotalVelocity;
            this._rb.Position = this.status.CurrentPosition;


            //set CalcVelocity to 0 to prevent any extra changes to velocity on the next frame
            this.status.CalcVelocity = new FVector2();
            //reset the walled flag so that it only appears on frames where we touch the wall
            this.status.TransitionFlags &= (~TransitionFlags.WALLED);

            //if (this.name == "TestPlayer" && this.status.TotalVelocity.magnitude > 0) Debug.Log("SpaxUpdate, total velocity :: (" + this.status.TotalVelocity.x + ", " + this.status.TotalVelocity.y + ")");

            //i++;
            //Debug.Log("SpaxUpdate, _rb velocity :: (" + this._rb.Velocity.x + ", " + this._rb.Velocity.y + ")");
            //Debug.Log("spaxupdate - " + this.name + " - " + i);
        }
        protected override void PostPhysUpdate()
        {  //if we're in hitstop, we con't want to assign velocity
            if (!this.status.InHitstop)
            {//record the current frame's velocity, to be used for the next frame
                this.status.CurrentVelocity = this._rb.Body.LinearVelocity;
                //return;
            }


            //if (this.name == "TestPlayer" && this.status.TotalVelocity.magnitude > 0) Debug.Log("PostUpdate, total velocity :: (" + this.status.TotalVelocity.x + ", " + this.status.TotalVelocity.y + ")");
            this.status.CurrentPosition = this._rb.Position;

        }

        /**
            Absolute LAST thing to process int the gameplay loop
        **/
        protected override void PostUpdate()
        {
            //record the current frame's position, to be used for the next frame



            //Debug.Log("-PostUpdate, _rb velocity :: (" + this._rb.Velocity.x + ", " + this._rb.Velocity.y + ")");


            //Debug.Log("postupdate - " + this.name + " - " + i);
            //if (this.status.CurrentState.name == "Jab") { Debug.Log("state timer in PostUpdate is - " + this.status.StateTimer.TimeElapsed + "/" + this.status.StateTimer.EndTime); }

        }

        //call to process transition data 
        protected virtual void ProcessTransitionData(in TransitionData trans)
        {
            //state to transition to
            var targetState = trans.TargetState;

            //if it's null, go to default state
            if (targetState == null)
            {
                //we want to automatically go to the default airborne or grounded state
                //  IF we either have a valid index OR there is not valid Universal state index
                if (trans.TargetStateIndex > -1 || trans.TargetUniversalStateIndex < 0)
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
                //check transition to universal state
                else
                {
                    targetState = Spax.SpaxManager.Instance.UniversalStates.GetStateData(trans.TargetUniversalStateIndex);
                }
            }
            //set the new state
            this.SetState(targetState);
            //process any transition events
            this.ProcessTransitionEvent(trans.TransitionEvents);

            //we process this transition event outside of the standard function
            //  this is because we want to access the transition's resources
            var changeInResources = trans.RequiredResources * (int)EnumHelper.isNotZero((uint)(trans.TransitionEvents & TransitionEvents.ADD_RESOURCES));
            this.AddCurrentResources(changeInResources);

            //check if it's a nodestate, if it is, try to transition out of it
            if (targetState.Duration < 0)
            {
                this.TryTransitionState();
            }
            //IF the duration is 0, AND there's an extra transition info that isn't 0, then we assign that as out new duration
            //this is to check for a scenario where we want to have a "stun state" without going through the universal states (IE. Blockstun)
            else if (targetState.Duration == 0 && (this.status.TransitionInfo.GetValue(1) > 0))
            {

                this.Status.StateTimer = new FrameTimer(this.status.TransitionInfo.GetValue(1));
            }

        }

        //call to process transition event enums
        protected virtual void ProcessTransitionEvent(in TransitionEvents te)
        {
            //int result for killing x velocity, 1 for has enum, 0 for doesn't have
            int killX = EnumHelper.HasEnumInt((uint)te, (uint)TransitionEvents.KILL_X_VEL);
            //int result for killing y velocity, 1 for has enum, 0 for doesn't have
            int killY = EnumHelper.HasEnumInt((uint)te, (uint)TransitionEvents.KILL_Y_VEL);
            //int result for flipping facing direction
            int flipDir = EnumHelper.HasEnumInt((uint)te, (uint)TransitionEvents.FLIP_FACING);


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

            //reassign the facing direction
            this.status.CurrentFacingDirection = this.status.CurrentFacingDirection * ((flipDir * -1) + ((flipDir ^ 1) * 1));

            //this.status.ResetLeniency();

        }

        //call to process the frame data
        protected virtual void ProcessFrameData(in FrameData frame)
        {
            /*----- PROCESSING STATE CONDITIONS -----*/
            this.status.CurrentStateConditions ^= frame.ToggleConditions;
            this.status.CancelFlags ^= frame.ToggleCancels;

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
            //add current resources
            this.AddCurrentResources(rChange);


            /*----- PROCESSING TIMER EVENT -----*/
            var timerEvent = frame.TimerEvent;

            //do we have a nonzero duration for this timer?
            var isNonzero = (int)EnumHelper.isNotZero((uint)timerEvent.Duration);

            if (isNonzero > 0)
            {
                //parameters we put into the timer
                var endTime = timerEvent.Duration * isNonzero;
                var addedStateConditions = (StateConditions)((int)timerEvent.StateConditions * isNonzero);

                //get new timer
                var newCondTimer = new ConditionTimer(endTime, addedStateConditions);

                //set the new timer
                this.status.ConditionTimer = newCondTimer;
            }

            /*----- PROCESSING SUPERFLASH -----*/
            this.StartSuperFlash(frame.SuperFlashDuration);

            /*----- PROCESSING PROJECTILE SPAWNING -----*/
            int projLen = this.data.Projectiles.Length;
            if (frame.HasProjectile() && projLen > -1)
            {
                var projectiles = frame.Projectiles;
                int facing = this.status.CurrentFacingDirection;
                int len = projectiles.Length;
                int i = 0;
                while (i < len)
                {
                    var projectileData = projectiles[i];
                    if (projectileData.ProjectileInd < projLen && projectileData.ProjectileInd > -1)
                    {
                        var obj = this.data.Projectiles[projectileData.ProjectileInd];
                        var rot = projectileData.SpawnRotation;
                        var pos = new FVector2(projectileData.RelativePos.x * facing, projectileData.RelativePos.y);

                        //actually instantiate the object
                        var go = (GameObject)Instantiate(obj, this.transform.position, this.transform.rotation);

                        if(go==null){Debug.Log("go is null");}
                        var goRb = go.GetComponent<FBox>();
                        //if(go==null){Debug.Log("goRb is null");}

                        goRb.Body.Position = pos + this._rb.Position;
                    }
                    i++;
                }
            }

        }

        //call to process the state conditions of our current state
        protected virtual void ProcessStateConditions(in StateConditions stateConditions)
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

                //whether or not we are in hitstun
                int inStun = EnumHelper.HasEnumInt((uint)stateConditions, (uint)StateConditions.STUN_STATE);
                int notStun = inStun ^ 1;

                //grab the current gravity for easy reference
                var grav = (this.status.CurrentGravity * notStun + this.data.JuggleMass * inStun) * this.status.GravityScaling;

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
            bool applyFriction = xVelCheck && ((!wantToMove) || fasterThanMax);// && EnumHelper.HasEnum((uint)this.status.TransitionFlags, (uint)StateConditions.GROUNDED);

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

                //if (this.status.CurrentState.name == "c_HBK_OnTheGround") Debug.Log("applying friction - " + appliedFrict + " - " + this.status.CalcVelocity.x + " - " + this.status.TotalVelocity.x);

            }

        }

        protected virtual void OnStateSet()
        {
            //restore gravity
            this.status.CurrentGravity = this.data.Mass;
        }

        //call to process try to transition the state
        protected void TryTransitionState()
        {

            //if (this.status.CurrentState.name == "c_HBK_StunAir") Debug.Log("checking airborne " + (this.status.CurrentPosition.y) + " " + (this.status.CurrentVelocity.y) + "  |  " + (this.status.CurrentPosition.y - (this._rb.Height / 2) > 0) + " " + (this.status.CurrentVelocity.y > 0));

            //simple check if we're airborne
            if ((this.status.CurrentPosition.y > 0) || (this.status.CurrentVelocity.y > 0))
            {
                //make status think it's airborne
                //Debug.Log("becoming airborne " + (this.status.CurrentPosition.y  > 0) ) + " " + (this.status.CurrentVelocity.y > 0));
                this.status.TransitionFlags = (~TransitionFlags.GROUNDED) & (this.status.TransitionFlags | TransitionFlags.AIRBORNE);
            }
            else
            {
                //make character think it's grounded
                //Debug.Log("becoming grounded ");
                this.status.TransitionFlags = (~TransitionFlags.AIRBORNE) & (this.status.TransitionFlags | TransitionFlags.GROUNDED);
                //this.status.CurrentVelocity = new FVector2(this.status.TotalVelocity.x, 0);
            }


            var trnFlags = this.status.TransitionFlags;
            var curCan = this.status.CancelFlags;
            var curRsrc = this.status.CurrentResources;
            var curInpt = this.status.Inputs;
            var curFacing = this.status.CurrentFacingDirection;
            var trans = this.status.CurrentState.CheckTransitions(trnFlags, curCan, curRsrc, curInpt, curFacing, this.status.TotalVelocity.y, this.status.CurrentPosition.y);
            //flags that the transition we want to use SHOULD have
            var reqFlags = trnFlags & TransitionFlags.REQ_FLAGS;

            //Debug.Log(this.gameObject.name + " zero check :: " + ((reqFlags & TransitionFlags.REQ_FLAGS) > 0));


            //don't check the state for transitions anymore
            //this.status.CheckState = false;
            //if it's null, no transition to process
            //also, check to make sure that this transition we found has the flags we want to see
            if (trans == null || ((!this.status.InHitstop) && (reqFlags != (trans.RequiredTransitionFlags & TransitionFlags.REQ_FLAGS))))
            {
                //check the move list in the data instead
                trans = this.data.CheckMoveList(trnFlags, curCan, curRsrc, curInpt, curFacing, this.status.TotalVelocity.y, this.status.CurrentPosition.y);
                //Debug.Log(this.gameObject.name + " first check :: " + this.status.TransitionInfo.GetValue(0) + " " + this.status.TransitionInfo.GetValue(1));

                //if (trans == null)
                //{
                //Debug.Log(curInpt[1].Input + " " + curInpt[1].Flags + " " + (curInpt[1].HoldDuration < Spax.SpaxManager.Instance.StaticValues.InputLeniency));
                //}

                //if trans is still null, we end the check
                //we check the required flags, but only if we're NOT in hitstop, since we are already checking that in StateUpdate
                if (trans == null || (reqFlags != (trans.RequiredTransitionFlags & TransitionFlags.REQ_FLAGS)))
                {
                    if (this.status.InHitstop) { return; }
                    //try transitioning to a universal state
                    var univTargetState = this.status.TransitionInfo.GetValue(0);
                    var totalStun = this.status.TransitionInfo.GetValue(1);
                    //Debug.Log(this.gameObject.name + " second check :: " + ((this.status.TransitionFlags & TransitionFlags.UNBLOCKED_HIT) > 0) + " " + univTargetState + " " + totalStun);
                    if (univTargetState > -1 && !(univTargetState == 0 && totalStun <= 0))
                    {
                        this.TryTransitionUniversalState(univTargetState);
                        //set stun duration
                        int sttDur = (this.status.CurrentState.Duration == 0) ? totalStun : this.status.CurrentState.Duration;
                        this.Status.StateTimer = new FrameTimer(sttDur);
                        //Debug.Log(this.status.CurrentState + " " + totalStun + " " + sttDur);
                        return;
                    }
                    //all else fails, we loop the state
                    else if (EnumHelper.HasEnum((uint)this.status.TransitionFlags, (uint)TransitionFlags.STATE_END) && this.status.CurrentState.GetLoop())
                    {
                        trans = new TransitionData(this.status.CurrentState);
                    }
                    //put the return in the else statement to so we can possibly send a transition to the looping state
                    else
                    {
                        return;
                    }
                }

                //Debug.Log("found transition in movelist - " + trans.TargetState.name);
            }
            //if (trans.TargetState != null) { Debug.Log("found transition to - " + trans.TargetState.name); }
            //if (this.status.CurrentState.name == "GroundedThrowHit") { Debug.Log("transition flags is - " + this.status.TransitionFlags); }


            this.ProcessTransitionData(trans);

        }

        protected void TryTransitionUniversalState(int universalStateInd = -1)
        {
            //simple check if we're airborne
            if ((this.status.CurrentPosition.y > 0) || (this.status.CurrentVelocity.y > 0))
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
                //this.status.CurrentVelocity = new FVector2(this.status.TotalVelocity.x, 0);
            }

            var trnFlags = this.status.TransitionFlags;
            var curCan = this.status.CancelFlags;
            var curRsrc = this.status.CurrentResources;
            var curInpt = this.status.Inputs;
            var curFacing = this.status.CurrentFacingDirection;

            var universalStateList = Spax.SpaxManager.Instance.UniversalStates;

            var trans = universalStateList.CheckTransition(universalStateInd, trnFlags, curCan, curRsrc, curInpt, curFacing, this.status.TotalVelocity.y, this.status.CurrentPosition.y);

            //if (universalStateInd == 2) { Debug.Log(trans.TargetState.name); }

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

        //how superflash will work is that all other players are set to be in hitstop for the duration except us
        //  renderer will handle all the flashy stuff

        //TODO: notify renderer that superflash has occured
        //TODO: 
        private void StartSuperFlash(int duration)
        {
            //will not fire superflash if no duration is given
            //keeps superflash even if state is changed
            if (duration <= 0) { return; }

            //set the other players to "hitstop"
            //manager instance
            var managerInstance = Spax.SpaxManager.Instance;
            int end = managerInstance.GetNumberOfPlayers();
            int i = 0;
            while (i < end)
            {
                if (i != this.status.PlayerID)
                {
                    var other = managerInstance.GetLivingObjectByID(i);
                    if (other != null)
                    {
                        //start our superflash by stopping others movement
                        other.StartStopTimer(duration);
                    }
                }
            }
            i++;

            //everything is prepped, start our superflash
            this.status.SuperFlashTimer = new FrameTimer(duration);
        }



        //call when you want to tick the SuperFlashTimer
        //  returns true when you tick superflash
        private bool TickSuperFlash()
        {
            //do we need to tick superflash?
            bool tickingSuperflash = !this.status.SuperFlashTimer.IsDone();
            //do we need to start superflash?
            bool startSf = tickingSuperflash && (this.status.SuperFlashTimer.TimeElapsed == 0);

            bool otherInSuperflash = false;

            //we have yet to start superflash, check to start superflash
            if (startSf)
            {
                //are the other characters with lower playerID in super flash?
                //manager instance
                var managerInstance = Spax.SpaxManager.Instance;
                int end = this.status.PlayerID;
                int i = 0;
                while (i < end)
                {
                    var other = managerInstance.GetLivingObjectByID(i);
                    otherInSuperflash = otherInSuperflash || other.IsInSuperFlash();
                    i++;
                }

                //no others are in superflash right now, start superflash
                if (!otherInSuperflash) { this.StartSuperFlash(this.status.SuperFlashTimer.EndTime); }
            }

            //if a character with a higher index is in superflash, we don't tick superflash
            bool ret = !otherInSuperflash && this.status.SuperFlashTimer.TickTimer();

            return ret;

        }
        protected void SetState(in soStateData newState)
        {
            //if (newState.name == "GroundedThrowHit") { Debug.Log("state duration is - " + newState.Duration); }

            //does our current state have a negative duration (is it a node state)?
            //  only 1 if the duration is positive
            var isNonNodeState = (((uint)newState.Duration) >> 31) ^ 1;
            // we only transfer state duration if
            //     a) we're EXITING in a stun state AND we're ENTERING a stun state AND the entering state's duration is 0
            /*var wasStunState = (int)EnumHelper.isNotZero((uint)(this.status.TotalStateConditions & StateConditions.STUN_STATE));
            var enterStunState = (int)EnumHelper.isNotZero((uint)(newState.StateConditions & StateConditions.STUN_STATE));
            var enterZeroDur = (int)(EnumHelper.isNotZero((uint)(newState.Duration)) ^ 1);
            var transferDuration = wasStunState * enterStunState * enterZeroDur;
            var replaceDuration = transferDuration ^ 1;
            var oldStateDuration = this.status.StateTimer.EndTime;
*/
            ///process current state
            this.ProcessTransitionEvent(this.status.CurrentState.ExitEvents);
            //set the new current state
            this.status.CurrentState = newState;
            //start the new state timer
            int stateDuration = (this.status.CurrentState.Duration);// * replaceDuration) + (oldStateDuration * transferDuration);
            this.status.StateTimer = new FrameTimer(stateDuration);
            //assign the current state conditions
            this.status.CurrentStateConditions = newState.StateConditions;
            //assign the current cancel conditions
            this.status.CancelFlags = newState.CancelConditions;
            //remove the state end transition flag
            this.status.TransitionFlags = this.status.TransitionFlags & ((TransitionFlags)~((int)(TransitionFlags.STATE_END | TransitionFlags.LANDED_HIT | TransitionFlags.GOT_HIT | TransitionFlags.BLOCKED_HIT | TransitionFlags.UNBLOCKED_HIT) * isNonNodeState));

            this.ProcessTransitionEvent(this.status.CurrentState.EnterEvents);

            //if (newState.name == "Stun-Grounded") { Debug.Log("TF in setstate is - " + this.status.TransitionFlags); }
            //if (newState.name == "Hitstun-Air") { Debug.Log("TF in setstate is - " + this.status.TransitionFlags); }
            //if (newState.name == "GroundedThrowHit") { Debug.Log("state timer in setstate is - " + this.status.StateTimer.TimeElapsed + "/" + this.status.StateTimer.EndTime); }

        }

        //call to set the state without any consideration about transition events or transfering stun
        protected void SetStateRaw(in soStateData newState)
        {
            //if (newState.name == "GroundedThrowHit") { Debug.Log("state duration is - " + newState.Duration); }

            //set the new current state
            this.status.CurrentState = newState;
            ///process current state
            //this.ProcessTransitionEvent(this.status.CurrentState.ExitEvents);
            //start the new state timer
            int stateDuration = (this.status.CurrentState.Duration);// * replaceDuration) + (oldStateDuration * transferDuration);
            this.status.StateTimer = new FrameTimer(stateDuration);
            //assign the current state conditions
            this.status.CurrentStateConditions = newState.StateConditions;
            //assign the current cancel conditions
            this.status.CancelFlags = newState.CancelConditions;
            //remove the state end transition flag
            this.status.TransitionFlags = (TransitionFlags)((int)this.status.TransitionFlags & ~((int)(TransitionFlags.STATE_END | TransitionFlags.LANDED_HIT | TransitionFlags.GOT_HIT | TransitionFlags.BLOCKED_HIT | TransitionFlags.UNBLOCKED_HIT)));

            this.OnStateSet();
            //this.ProcessTransitionEvent(this.status.CurrentState.EnterEvents);
            //Debug.Log(this.gameObject.name + " Raw transition to " + newState.name + " | Current state is " + this.status.CurrentState.name);

            //if (newState.name == "Stun-Grounded") { Debug.Log("TF in setstate is - " + this.status.TransitionFlags); }
            //if (newState.name == "Hitstun-Air") { Debug.Log("TF in setstate is - " + this.status.TransitionFlags); }
            //if (newState.name == "GroundedThrowHit") { Debug.Log("state timer in setstate is - " + this.status.StateTimer.TimeElapsed + "/" + this.status.StateTimer.EndTime); }

        }


        public void SetPosition(FVector2 newPos) { this.status.CurrentPosition = newPos; }

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


                //record the current frame's velocity, to be used for the next frame
                this.status.CurrentVelocity = this._rb.Body.LinearVelocity;

                //should freeze the rigidbody's velocity
                this._rb.Body.LinearVelocity = new FVector2();
            }

        }

        protected void SetCurrentResources(in ResourceData newResources)
        {
            this.status.CurrentResources = newResources.SetMin(this.data.MaxResources);
            //this.status.CurrentResources.SetMin(this.data.MaxResources);
        }

        protected void AddCurrentResources(in ResourceData newResources)
        {
            var potenResources = newResources + this.status.CurrentResources;
            //Debug.Log("adding resources");
            this.status.CurrentResources = potenResources.SetMin(this.data.MaxResources);
            //Debug.Log("current resources :: " + this.status.CurrentResources.Resource1 + " | " + this.data.MaxResources.Resource1);
        }

        public FVector2 get_position()
        {
            //we add the width in the opposite direction so that we prevent the opponent change direction
            //if WE'RE in the corner
            var ret = this.status.CurrentPosition - (new FVector2(0, this.status.CurrentFacingDirection * this._rb.Width / 2) * this.IsWalled());

            return ret;
        }

        public int IsAirborne()
        {
            var ret = (int)EnumHelper.isNotZero((uint)(this.status.TotalStateConditions & StateConditions.AIRBORNE));

            return ret;
        }

        public int IsWalled()
        {
            var ret = (int)EnumHelper.isNotZero((uint)(this.status.TransitionFlags & TransitionFlags.WALLED));
            //Debug.Log(this.gameObject.name + " checking walled :: " + ret + " | " + (this.status.TransitionFlags & TransitionFlags.WALLED));
            return ret;
        }

        public bool IsInSuperFlash()
        {
            bool ret = !this.status.SuperFlashTimer.IsDone();
            return ret;
        }


        public void SetWalled(int done)
        {
            this.status.TransitionFlags |= (TransitionFlags)((int)TransitionFlags.WALLED * done);
        }

        public void StartStopTimer(int dur)
        {
            this.SetStopTimer(dur);
        }

        public void ApplyGameplayState(in GameplayState state)
        {
            var newCharState = this.data.GetStateFromID(state.CurrentStateID);
            this.SetStateRaw(newCharState);

            this.status.ApplyGameplayState(state);

            this.OnApplyGameState(state);
        }

        //functions to be called by the projectile 
        public virtual void ProjDestroyed() { }
        public virtual void ProjRegisterHit(HitInfo info) { }

        //extra things that need to be called when we set the gameplay state
        protected virtual void OnApplyGameState(in GameplayState state) { }

        public virtual CharStateInfo GetCharacterInfo()
        {
            return new CharStateInfo(this.status, new HitboxTrigger[8], new HurtboxTrigger[8]);
        }

    }
}