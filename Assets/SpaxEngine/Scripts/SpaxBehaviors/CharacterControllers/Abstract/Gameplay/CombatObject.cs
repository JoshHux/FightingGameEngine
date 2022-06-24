using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
                //we don't need to hook the delegate, since the boxes do it themselves on start
            }
        }
    }
}