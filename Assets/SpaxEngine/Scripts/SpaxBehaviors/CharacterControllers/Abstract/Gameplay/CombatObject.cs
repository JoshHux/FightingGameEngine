using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Gameplay
{
    public abstract class CombatObject : VulnerableObject
    {
        private HitboxTrigger[] _hitboxes;
        public override void ResetStatus()
        {
            base.ResetStatus();
            //reset damage scaling
            this.status.CurrentDamageScaling = 1;
            this.status.StoredDamageScaling = 1;
        }

        protected override void OnStart()
        {
            base.OnStart();

            GameObject hitHolder = ObjectFinder.FindChildWithTag(this.gameObject, "HitboxContainer");
            this._hitboxes = hitHolder.GetComponentsInChildren<HitboxTrigger>();

            //initialize our boxes
            int len = this._hitboxes.Length;
            for (int i = 0; i < len; i++)
            {
                HitboxTrigger box = this._hitboxes[i];
                //set the trigger index for each box
                box.SetTriggerIndex(i);
                box.SetTriggerAllegiance(this.status.Allegiance);
                //we don't need to hook the delegate, since the boxes do it themselves on start
            }
        }

        protected override void HitboxQueryUpdate()
        {

            //list of hit info to process
            var hitInfoList = new List<HitInfo>();

            int i = 0;
            int len = this._hitboxes.Length;

            while (i < len)
            {
                var hold = this._hitboxes[i];

                bool isActive = hold.IsActive();
                if (isActive)
                {
                    var results = hold.QueryHitbox();

                    int resultsLen = results.Length;

                    if (resultsLen > 0)
                    {
                        hitInfoList.AddRange(results);
                    }
                }

                i++;
            }

            int infoLen = hitInfoList.Count;

            if (infoLen > 0)
            {
                var toProcess = hitInfoList.ToArray();
                this.ProcessHitInfo(toProcess);
            }
        }


        //process the hits we landed
        private void ProcessHitInfo(in HitInfo[] hitInfo)
        {
            // Debug.Log("processing HitInfo");

            //we only want to process hitboxes with the highest priority
            //      FOREACH unique object
            var processList = new List<HitInfo>();

            int i = 0;
            int len = hitInfo.Length;
            while (i < len)
            {
                var hold = hitInfo[i];
                var hitTarget = hold.HurtCharacter;

                //only process if HitInfo has a non-whiff indicator
                bool notWhiff = hold.Indicator > 0;

                if (notWhiff)
                {

                    //TODO: check if we got a blocked or clean hit

                    var existsTarget = processList.Find(o => o.HurtCharacter == hitTarget);

                    if (existsTarget == null)
                    {
                        //Debug.Log("add to list");

                        processList.Add(hold);

                    }
                    else
                    {
                        //Debug.Log("checking the hit priority");
                        bool newHasHigherPriority = existsTarget.HitboxData.Priority > hold.HitboxData.Priority;

                        if (newHasHigherPriority)
                        {
                            processList.Remove(existsTarget);
                            processList.Add(hold);
                        }
                    }


                }

                i++;
            }


            //process the things we made contact with
            i = 0;
            len = processList.Count;
            while (i < len)
            {
                //holds the current HitInfo object
                var hold = processList[i];

                var hitIndicator = hold.Indicator;

                //did we block the hit?
                int blocked = (int)EnumHelper.isNotZero((uint)(hitIndicator & HitIndicator.BLOCKED));
                // did we land a grab?
                int grabbed = (int)EnumHelper.isNotZero((uint)(hitIndicator & HitIndicator.GRABBED));
                //is it a combo hit?
                int comboed = (int)EnumHelper.isNotZero((uint)(hitIndicator & HitIndicator.COMBO));
                //unblocked hit
                int rawHit = blocked ^ 1;
                //we landed a strike
                this.landedStrikeThisFrame = grabbed == 0 && rawHit > 0;

                if (comboed == 0)
                {
                    this.status.CurrentDamageScaling = 1;
                }

                //multiply hitbox's scaling with stored scaling for every box that isn't blocked
                if (blocked == 0)
                {
                    this.status.StoredDamageScaling *= hold.HitboxData.ForcedProration;
                }


                //set hitstop
                int potenHitstop = hold.HitboxData.Hitstop * rawHit + hold.HitboxData.BlockStop * blocked;
                this.SetStopTimer(potenHitstop);

                this.AddCurrentResources(hold.HitboxData.ResourceChange);
                this.status.CancelFlags |= (hold.HitboxData.OnHitCancel);

                if (EnumHelper.HasEnum((uint)hold.Indicator, (uint)HitIndicator.COUNTER_HIT))
                {
                    this.status.CancelFlags |= (hold.HitboxData.OnCounterHitCancel);
                }
                else if (EnumHelper.HasEnum((uint)hold.Indicator, (uint)HitIndicator.BLOCKED))
                {
                    this.status.CancelFlags = (hold.HitboxData.OnBlockedHitCancel);
                }

                this.status.TransitionFlags |= (TransitionFlags)((int)TransitionFlags.LANDED_HIT * rawHit);

                i++;
            }

        }

        //fires when state is changed
        protected override void OnStateSet()
        {
            base.OnStateSet();
            this.status.CurrentDamageScaling *= this.status.StoredDamageScaling;
            this.status.StoredDamageScaling = 1;
        }

        //gets the first hurtbox in a list, helpful for hitbox stuff
        public HitboxTrigger GetHitbox(int i) { return this._hitboxes[i]; }

        public override CharStateInfo GetCharacterInfo()
        {
            return new CharStateInfo(this.status, this._hitboxes, this.hurtboxes);
        }

    }
}