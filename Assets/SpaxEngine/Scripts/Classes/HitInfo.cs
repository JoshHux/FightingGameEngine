using UnityEngine;
using FixMath.NET;
using FightingGameEngine.Enum;
using FightingGameEngine.Gameplay;
namespace FightingGameEngine.Data
{
    //has info to send to attacking character
    [System.Serializable]
    public class HitInfo
    {
        //data of hitbox that made contact
        private HitboxData _hitboxData;
        //indicator for what happened
        private HitIndicator _indicator;

        //location to put vfx, 0.5 lerp between hitbox and contacking box position
        private Vector3 _contactLoc;
        //parent object of box we're colliding with
        private VulnerableObject _otherOwner;


        public HitboxData HitboxData { get { return this._hitboxData; } set { this._hitboxData = value; } }
        public HitIndicator Indicator { get { return this._indicator; } set { this._indicator = value; } }
        public Vector3 ContactLoc { get { return this._contactLoc; } set { this._contactLoc = value; } }
        public VulnerableObject OtherOwner { get { return this._otherOwner; } set { this._otherOwner = value; } }

        public HitInfo(HitboxData hd, HitIndicator hi, Vector3 cl, VulnerableObject vu)
        {
            this._hitboxData = hd;
            this._indicator = hi;
            this._contactLoc = cl;
            this._otherOwner = vu;
        }
    }
}