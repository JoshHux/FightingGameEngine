using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Data;

namespace FightingGameEngine.Gameplay
{
    public abstract class CombatObject : VulnerableObject
    {
        private HitboxTrigger[] _hitboxes;
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
        private void ProcessHitInfo(HitInfo[] hitInfo)
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
                var hitTarget = hold.OtherOwner;

                //only process if HitInfo has a non-whiff indicator
                bool notWhiff = hold.Indicator > 0;

                if (notWhiff)
                {

                    //TODO: check if we got a blocked or clean hit

                    var existsTarget = processList.Find(o => o.OtherOwner == hitTarget);

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

                    this.status.TransitionFlags |= Enum.TransitionFlags.LANDED_HIT;

                }

                i++;
            }


            //process the things we made contact with
            i = 0;
            len = processList.Count;
            while (i < len)
            {
                var hold = processList[i];

                var hitIndicator = hold.Indicator;

                //set hitstop
                int potenHitstop = hold.HitboxData.Hitstop;
                this.SetStopTimer(potenHitstop);

                this.status.CurrentResources += (processList[i].HitboxData.ResourceChange);
                this.status.CancelFlags |= (processList[i].HitboxData.OnHitCancel);

                i++;
            }

        }



    }
}