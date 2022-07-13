using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Gameplay
{
    public delegate void BoxActivator(object sender, FrameData frameData);

    public abstract class VulnerableObject : LivingObject
    {
        private HurtboxTrigger[] _hurtboxes;

        //delgate that is called to activate hit/hurtboxes, we pass the hitbox triggers the frame data to know whether or not to activate
        public BoxActivator OnFrameReached;

        public int Facing { get { return this.status.CurrentFacingDirection; } }

        protected override void OnStart()
        {
            base.OnStart();

            //find the parent of all hurtboxes
            GameObject hurtHolder = ObjectFinder.FindChildWithTag(this.gameObject, "HurtboxContainer");
            //stor all of the hurtboxes
            this._hurtboxes = hurtHolder.GetComponentsInChildren<HurtboxTrigger>();
            //initialize all hurtboxes
            int len = _hurtboxes.Length;
            for (int i = 0; i < len; i++)
            {
                HurtboxTrigger box = this._hurtboxes[i];
                //set the trigger index for each box
                box.SetTriggerIndex(i);
                //we don't need to hook the delegate, since the boxes do it themselves on start
            }
        }

        protected override void HurtboxQueryUpdate() { }


        //call to process the frame data
        protected override void ProcessFrameData(FrameData frame)
        {
            //call base version
            base.ProcessFrameData(frame);

            /*----- ACTIVATING HURTBOXES -----*/
            //call the delegate and pass the frame in, the boxes will take care of the rest
            //the way that we set this up, we don't need to call this from CombatObject because the HitboxTrigger objects will also have their hooks in this delegate
            this.OnFrameReached.Invoke(this, frame);
        }

        //called by the hurtbox to add the hitbox to process in HurtBoxQueryUpdate
        public HitIndicator AddHitboxToQuery(HitboxData boxData)
        {
            HitIndicator ret = 0;
            return ret;
        }


    }
}