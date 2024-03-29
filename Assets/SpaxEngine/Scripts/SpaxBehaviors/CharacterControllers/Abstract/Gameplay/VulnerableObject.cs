using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;
using FixMath.NET;

namespace FightingGameEngine.Gameplay
{
    public delegate void HurtBoxActivator(object sender, in HurtboxHolder evnt);
    public delegate void StateAssigner(object sender, in GameplayState state);

    public abstract class VulnerableObject : LivingObject
    {
        protected HurtboxTrigger[] hurtboxes;

        //delgate that is called to activate hit/hurtboxes, we pass the hitbox triggers the frame data to know whether or not to activate
        public HurtBoxActivator OnHurtFrameReached;
        //delegate that is called to assign hit/hurtbox states, we pass the gameplay state to the triggers to get them prepped with the right data
        public StateAssigner OnGameStateSet;

        public int Facing { get { return this.status.CurrentFacingDirection; } }

        protected override void OnStart()
        {
            base.OnStart();

            //find the parent of all hurtboxes
            GameObject hurtHolder = ObjectFinder.FindChildWithTag(this.gameObject, "HurtboxContainer");
            //stor all of the hurtboxes
            this.hurtboxes = hurtHolder.GetComponentsInChildren<HurtboxTrigger>();
            //initialize all hurtboxes
            int len = this.hurtboxes.Length;
            for (int i = 0; i < len; i++)
            {
                HurtboxTrigger box = this.hurtboxes[i];
                //set the trigger index for each box
                box.SetTriggerIndex(i);
                box.SetTriggerAllegiance(this.status.Allegiance);
                //we don't need to hook the delegate, since the boxes do it themselves on start
            }

            //init the list of hitinfo objects to process when hit
            this.m_hurtList = new List<HitInfo>();
            //init indicator of whether or not we landed a strike
            this.landedStrikeThisFrame = false;
        }

        //ONLY HERE TO MAKE SURE THAT STRIKE-TRADES END THE STRIKE'S FAVOR
        protected bool landedStrikeThisFrame = false;

        protected override void HurtboxQueryUpdate()
        {
            int i = 0;
            int len = this.m_hurtList.Count;

            //iindex of universal target state we want to go to
            int univTargetState = -1;
            int totalStun = 0;

            //are we airborne?
            int airborne = this.IsAirborne();

            while (i < len)
            {
                var hold = this.m_hurtList[i];
                //applied knockback
                var kb = new FVector2();

                //were we grabbed?
                bool isGrabbed = EnumHelper.HasEnum((uint)hold.Indicator, (uint)HitIndicator.GRABBED);
                int grabbed = EnumHelper.HasEnumInt((uint)hold.Indicator, (uint)HitIndicator.GRABBED);

                //is this a non-whiff?
                bool notWhiff = hold.Indicator > 0;
                bool tradedWithGrab = (isGrabbed && landedStrikeThisFrame);
                if (notWhiff && !tradedWithGrab)
                {
                    //quick access to the hitbox data
                    var boxData = hold.HitboxData;

                    //did we block the hit?
                    int blocked = (int)EnumHelper.isNotZero((uint)(hold.Indicator & HitIndicator.BLOCKED));
                    //did we get counter hit?
                    int counter = (int)EnumHelper.isNotZero((uint)(hold.Indicator & HitIndicator.COUNTER_HIT));
                    //unblocked hit
                    int rawHit = blocked ^ 1;

                    //Debug.Log("processing hit");

                    //set universal target state
                    //  we want the original hit cause if it's grab BECAUSE the state grabs cause should ignore counterhits
                    univTargetState = boxData.UniversalStateCause * (counter ^ 1) + boxData.CounterStateCause * counter * (grabbed ^ 1);

                    //hitstun for grounded, untech time for airborne, also add stun mod if it's a coutnerhit
                    totalStun = (boxData.Hitstun * (airborne ^ 1) + boxData.UntechTime * airborne) * rawHit + boxData.BlockStun * blocked + counter * (boxData.Hitstun * (airborne ^ 1) + boxData.UntechTime * airborne);

                    //if (blocked > 0) { Debug.Log("blocked hit"); }
                    //add transition flag to let the status know we got hit
                    this.status.TransitionFlags = this.status.TransitionFlags | TransitionFlags.GOT_HIT | (TransitionFlags)((int)TransitionFlags.BLOCKED_HIT * blocked) | (TransitionFlags)((int)TransitionFlags.UNBLOCKED_HIT * (blocked ^ 1));

                    //Debug.Log("blocked? " + ((this.status.TransitionFlags & TransitionFlags.UNBLOCKED_HIT) > 0));

                    //knockback/postion offset value (for grabs)
                    var groundedPhysVal = boxData.GroundedKnockback;
                    groundedPhysVal.x = groundedPhysVal.x * hold.HitCharacter.Facing;
                    var airbornePhysVal = boxData.AirborneKnockback;
                    airbornePhysVal.x = airbornePhysVal.x * hold.HitCharacter.Facing;

                    //remove the accumulated velocity so far
                    this.status.CurrentVelocity = new FVector2(0, 0);


                    if (isGrabbed)
                    {
                        //we got grabbed, reset armor
                        this.status.SetTransitionInfoVal(2, 0);

                        //position of grabber
                        var grabberPosition = hold.HitCharacter.FlatPosition;

                        var newPosition = grabberPosition + groundedPhysVal;

                        this.SetPosition(newPosition);

                        //set proper facing direction
                        //what is the difference between our x position and their x position?
                        var diffPos = hold.HitCharacter.FlatPosition.x - this.status.CurrentPosition.x;

                        //if our facing direction and the difference in position are different, then we should turn
                        bool shouldTurn = (diffPos * this.status.CurrentFacingDirection) < 0;

                        if (shouldTurn)
                        {
                            var newFacing = Fix64.Sign(diffPos);
                            this.status.CurrentFacingDirection = newFacing;
                        }
                        //Debug.Log("universal transition state - " + univTargetState);
                        Spax.SpaxManager.Instance.ResolveRepositioning(hold.HitCharacter.Body, this.Body);
                    }
                    else
                    {
                        int grounded = airborne ^ 1;
                        //Velocity in hitstop will be restored based on CurrentVelocity
                        kb = (groundedPhysVal * grounded) + (airbornePhysVal * airborne);
                        //Debug.Log("grounded - " + grounded + " | airborne - " + airborne + " | knockback - " + ((groundedPhysVal * grounded) + (airbornePhysVal * airborne)).x + " , " + ((groundedPhysVal * grounded) + (airbornePhysVal * airborne)).y);

                        //COALATE ALL THIS DATA AND APPLY IT AFTER THE LOOP
                        //set bounce data
                        this.status.GroundBounces = boxData.GroundBounces;
                        this.status.GroundBounceScaling = boxData.GroundBounceMultiplier;
                        this.status.WallBounces = boxData.WallBounces;
                        this.status.WallBounceScaling = boxData.WallBounceMultiplier;

                        //do we have armor?
                        int armorHits = this.status.TransitionInfo.GetValue(2);
                        //if we are armored
                        if (armorHits > 0)
                        {
                            //cancel bouncing
                            this.status.GroundBounces = 0;
                            this.status.GroundBounceScaling = 0;
                            this.status.WallBounces = 0;
                            this.status.WallBounceScaling = 0;

                            //reset knockback
                            kb = FVector2.zero;

                            //reset transition flags
                            this.status.TransitionFlags = this.status.TransitionFlags & ~(TransitionFlags.GOT_HIT | (TransitionFlags)((int)TransitionFlags.BLOCKED_HIT * blocked) | (TransitionFlags)((int)TransitionFlags.UNBLOCKED_HIT * (blocked ^ 1)));

                            //reset universal state, so we don't transition out of the current state
                            univTargetState = -1;
                            totalStun = 0;

                            //subtract a hit of armor
                            this.status.SetTransitionInfoVal(2, armorHits - 1);
                        }

                    }

                    //set hitstop
                    int potenHitstop = hold.HitboxData.Hitstop * rawHit + hold.HitboxData.BlockStop * blocked;
                    this.SetStopTimer(potenHitstop);
                    //set the current velocity to kb so that we only override velocity when hit
                    this.status.CurrentVelocity = kb;

                    //set hitspark
                    this.status.RendererInfo.VFXID = boxData.Hitspark;
                    this.status.RendererInfo.VFXPos = hold.ContactLoc;
                    //print(hold.ContactLoc);
                }

                // deal damage            
                this.status.CurrentHP -= (int)Fix64.Max(hold.HitboxData.Damage * hold.CurrentDamageScaling, hold.HitboxData.MinDamage);
                //this.status.CurrentHP -= boxData.HitboxData.Damage; 

                i++;
            }

            //set the transition info to hold onto into the next frame
            //first item is the target universal state
            this.status.SetTransitionInfoVal(0, univTargetState);
            //second item is the duration of stun
            this.status.SetTransitionInfoVal(1, totalStun);
            //Debug.Log("stun val :: " + this.status.TransitionInfo.GetValue(1));


            //DON'T transition if we're going into hit/blockstun with 0 totalstun
            /*
            if (univTargetState > -1 && !(univTargetState == 0 && totalStun <= 0))
            {
                this.TryTransitionUniversalState(univTargetState);
                //set stun duration
                int sttDur = (this.status.CurrentState.Duration == 0) ? totalStun : this.status.CurrentState.Duration;
                this.Status.StateTimer = new FrameTimer(sttDur);
                //Debug.Log(this.status.CurrentState + " " + totalStun + " " + sttDur);
            }
            */


            this.landedStrikeThisFrame = false;
        }

        protected override void PostUpdate()
        {
            base.PostUpdate();
            this.m_hurtList.Clear();

        }

        //call to process the state conditions of our current state
        protected override void ProcessStateConditions(in StateConditions stateConditions)
        {
            //grounded or not
            int grounded = this.IsAirborne() ^ 1;
            //walled or not
            int walled = this.IsWalled();

            //bounce data
            //  we multiply each ground bounce count by the respective environment check to make it so that we only apply bounce if each count is nonzero
            //wall bounce count
            int groundBounces = this.status.GroundBounces * grounded;
            //ground bounce count
            int wallBounces = this.status.WallBounces * walled;


            base.ProcessStateConditions(stateConditions);
            //get the current physics velocity for bouncing
            //new velocity that will be reassigned to the current velocity
            var newVel = this.status.CurrentVelocity;

            //TODO: see if we can make this branchless
            //  MAYBE: check to make sure we only wall bounce when airborne

            //we only bounce with x or y velocity since we ony want the character to either pop up or away from the wall
            //  we don't check the direction of the velocity but generally, we will only really collide with the wall or ground 
            //  in the same direction of their velocity
            //      IF BOUNCES ARE NOT WORKING PROPERLY: CHANGE THE CHECK OF VELOCITY TO EXPLICITLY LAUNCH AWAY FROM THE WALL
            if (groundBounces > 0)
            {
                var gBounceScaling = this.status.GroundBounceScaling;

                //multiply by the negative scaling so that we bounce back up
                newVel.y = newVel.y * gBounceScaling * -1;

                //deincrement respective bounce count
                this.status.GroundBounces -= 1;
            }
            if (wallBounces > 0)
            {
                var wBounceScaling = this.status.WallBounceScaling;

                //multiply by the negative scaling so that we bounce back up
                newVel.x = newVel.x * wBounceScaling * -1;
                //deincrement respective bounce count
                this.status.WallBounces -= 1;
            }

            //reassign new velocity
            this.status.CurrentVelocity = newVel;

        }
        //call to process transition event enums
        protected override void ProcessTransitionEvent(in TransitionEvents te)
        {
            base.ProcessTransitionEvent(te);

            /*----- PROCESSING TECHING EVENT -----*/
            bool grabTech = EnumHelper.HasEnum((uint)te, (uint)TransitionEvents.GRAB_TECH);

            if (grabTech)
            {
                int otherID = (this.status.PlayerID == 0) ? 1 : 0;
                var grabber = Spax.SpaxManager.Instance.GetLivingObjectByID(otherID);

                grabber.TryTransitionUniversalState(1);
            }

            if (EnumHelper.HasEnum((uint)te, (uint)TransitionEvents.EXIT_STUN))
            {
                this.status.CurrentDamageScaling = 1;
                this.status.StoredDamageScaling = 1;
            }
        }

        //fires when state is changed
        protected override void OnStateSet()
        {
            base.OnStateSet();
            //this.OnHurtFrameReached?.Invoke(this, new HurtboxHolder());
            //reset number of armored hits
            this.status.SetTransitionInfoVal(2, 0);
        }


        //list of hitboxes to process, reset every frame
        private List<HitInfo> m_hurtList;
        //called by the hurtbox to add the hitbox to process in HurtBoxQueryUpdate
        public HitIndicator AddHitboxToQuery(in HitInfo boxData)
        {
            HitIndicator ret = HitIndicator.WHIFF;

            var hasGrab = this.m_hurtList.Find(hold => hold.HitboxData.Type == HitboxType.GRAB);
            if (hasGrab != null)
            {
                return ret;
            }

            /*--- our state condition info ---*/

            //current state conditions we process
            var curSttCond = this.status.TotalStateConditions;
            //are we in a stunstate?
            var inStun = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.STUN_STATE));

            //what we are invulnerable against
            var strikeInvuln = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.INVULNERABLE_STRIKE));
            var grabInvuln = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.INVULNERABLE_GRAB));
            var projInvuln = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.INVULNERABLE_PROJECTILE));
            var onTheGround = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.ON_THE_GROUND));

            //what we have guard point against
            var midGuard = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.GUARD_POINT_MID));
            var lowGuard = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.GUARD_POINT_LOW));
            var highGuard = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.GUARD_POINT_HIGH));
            var guardEnum = (uint)((uint)curSttCond >> 23);

            //are we grounded or airborne?
            var airOrGround = (uint)(this.status.TotalStateConditions & (StateConditions.GROUNDED | StateConditions.AIRBORNE));

            /*--- attacker hitbox properties ---*/

            //the total hitbox characteristics
            var hitboxType = boxData.HitboxData.Type;

            //strike properties of the hitbox
            var strikeType = hitboxType & HitboxType.STRIKE;
            //strike properties of the hitbox
            uint unblockableType = ((uint)(hitboxType & HitboxType.TRUE_UNBLOCKABLE) >> 7);
            //grab properties of the hitbox
            uint grabType = ((uint)(hitboxType & HitboxType.GRAB) >> 3);

            //hitbox type check
            var isStrike = (int)EnumHelper.isNotZero((uint)(strikeType));
            var isGrab = (int)EnumHelper.isNotZero((uint)(grabType));
            var isProj = (int)EnumHelper.isNotZero((uint)(hitboxType & HitboxType.PROJECTILE));
            var isNotOtg = ((int)EnumHelper.isNotZero((uint)(hitboxType & HitboxType.OFF_THE_GROUND)) ^ 1);
            //is the hit unblockable?
            var isBlockable = ((int)EnumHelper.HasEnumInt((uint)airOrGround, (uint)unblockableType) ^ 1);
            var isStrikeGrab = isGrab & isStrike;

            /*--- COMPARING THE HITBOX INFO VS OUR STATE CONDITIONS ---*/

            //-- IMPORTANT NOTE --//
            //We are only checking if we're crossed up manually because we're using a block button and we want to at least make crossups work
            //TODO: make block button ignore crossups a toggleable feature
            //are we in a blockstun state?
            int inBlockstun = (int)(EnumHelper.isNotZero((uint)(curSttCond & StateConditions.GUARD_POINT)) * EnumHelper.isNotZero((uint)(curSttCond & StateConditions.STUN_STATE)));

            //if we're blocking, check the x-position of the attacker and us to check for crossups
            //  only 1 if we got crossed up, 0 if we didn't
            int crossup = (int)(((uint)(Fix64.Sign(boxData.HitCharacter.FlatPosition.x - this.status.CurrentPosition.x) * this.status.CurrentFacingDirection)) >> 31);

            /* SIDE TANGENT */
            /*
            I just had a revelation.
            Edd has told me that in GGXrd, the blocking state will force the character to automatically turn to face whatever just hit them.
            And that crossups only hit if they're not in blockstun.
            This makes crossups in true blockstrings do nothing.
            I thought this was weird until I realized something.
            First off, this is a form of crossup protection, which is nice, but here's the most important thing:
                BLOCKING IN XRD DOESN'T CARE ABOUT WHAT SIDE YOU'RE ON
                I think, I actually don't know it's a theory, but I'm pretty sure this is the case for all ArkSys games. 
            Let me explain.
            GBFVS and DNF Duel are ArkSys games with a block buttons and it gives complete crossup protection. I didn't think much of it until 
            Edd told me the second thing, and I realized that the only reason that would happen is because THE BLOCKING PLAYER ISN'T HOLDING BACKWARDS!
            But then I thought about FD and Barrier in GG and BB respectively, they have ways to manually enter block states, they don't give 
            you crossup protection!
            But that's because you still have to hold backwards to enter that state! as soon as your character changes what direction they face, they'll
            stop blocking!
            ArkSys blockstates always protect against crossups! They just look like they don't because you have to hold backward to enter them!
            */


            //strike box
            //we add inBlockstun to make it so that we give crossup protection in blockstun
            var weBlocked = EnumHelper.HasEnumInt(guardEnum, (uint)strikeType) * isBlockable * ((crossup ^ 1) | inBlockstun);
            var weGotHit = weBlocked ^ 1;


            //provide data to the ret
            ret = (HitIndicator)((weBlocked * (int)HitIndicator.BLOCKED) | (weGotHit * (int)HitIndicator.HIT));

            //invuln check
            //  each multiplier vairable can ONLY be zero IF they are that hitbox type AND we have the corresponding invuln condition 
            //  we want the stuff afte 1^ to be 0 for a hit, 1 for a whiff

            //projectile invuln multiplier
            //  only considers a whiff if this is a projectile first
            var projMult = 1 ^ (isProj * projInvuln);
            var otgMult = 1 ^ (isNotOtg * onTheGround);

            ret = (HitIndicator)((int)ret * projMult * otgMult);

            //strike invuln multiplier
            //  mult is 1 if it isn't a strike box
            var strikeIndicator = (isStrike | isStrikeGrab);
            var strikeMult = 1 ^ ((strikeIndicator * strikeInvuln) | (strikeIndicator ^ 1));

            ret = (HitIndicator)((int)ret * strikeMult);

            //grab invuln multiplier
            var grabLocMatch = (int)EnumHelper.isNotZero((uint)(grabType & airOrGround));
            //  ((isStrikeGrab^isGrab)*isStrikeGrab) overrides isGrab to be 0 if the hit is a strike-grab
            //      it's 1 if we have a regular grab
            var rawGrab = ((isStrikeGrab ^ isGrab) * (isStrikeGrab ^ 1));
            var grabMult = 1 ^ ((rawGrab & (grabInvuln | inStun | (grabLocMatch ^ 1))) | (isStrike * (1 ^ isStrikeGrab)));

            ret |= (HitIndicator)((int)HitIndicator.GRABBED * grabMult);

            //add combo flag
            ret |= (HitIndicator)((int)HitIndicator.COMBO * inStun);



            //make copy of object just in case of some shallow memory access shenanigans
            //last parameter is the object that hit us (to be used later)
            var toAdd = new HitInfo(boxData.HitboxData, ret, boxData.ContactLoc, boxData.HurtCharacter, boxData.HitCharacter);


            //make sure we only are hit by the hitbox WITH THE HIGHEST priority if hit by multiple hitboxes from the same object
            var existsTarget = this.m_hurtList.Find(o => o.HitCharacter == toAdd.HitCharacter);
            if (existsTarget == null)
            {
                //Debug.Log("add to list");


                //if hitbox is grab, clear the other objects to query so that character doesn't get hit by both grab and strike
                var existsGrab = this.m_hurtList.Find(hold => hold.HitboxData.Type == HitboxType.GRAB);
                if (existsGrab != null)
                {
                    this.m_hurtList.Insert(0, existsGrab);
                    this.m_hurtList.RemoveRange(1, this.m_hurtList.Count - 1);
                    this.m_hurtList.Add(toAdd);
                }
                else
                {
                    //add to list of hitboxes to process
                    this.m_hurtList.Add(toAdd);
                }

            }
            else
            {
                //Debug.Log("checking the hit priority");
                bool newHasHigherPriority = existsTarget.HitboxData.Priority > toAdd.HitboxData.Priority;

                if (newHasHigherPriority)
                {
                    this.m_hurtList.Remove(existsTarget);
                    //add to list of hitboxes to process
                    this.m_hurtList.Add(toAdd);
                }
            }




            return ret;
        }

        //gets the first hurtbox in a list, helpful for hitbox stuff
        public HurtboxTrigger GetHurtbox(int i) { return this.hurtboxes[i]; }

        //call to activate a set of hurtboxes
        public void ActivateHurtboxes(HurtboxHolder boxes) { this.OnHurtFrameReached?.Invoke(this, boxes); }

        public void set_armor_hits(int a)
        {
            this.status.SetTransitionInfoVal(2, a);
        }

        public override CharStateInfo GetCharacterInfo()
        {
            return new CharStateInfo(this.status, new HitboxTrigger[8], this.hurtboxes);
        }

        protected override void OnApplyGameState(in GameplayState state)
        {
            this.OnGameStateSet?.Invoke(this, state);
        }

    }
}