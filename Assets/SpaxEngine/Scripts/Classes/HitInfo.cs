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
        //parent object of box hurt
        private VulnerableObject _hurtCharacter;
        //parent object of box hit
        private CombatObject _hitCharacter;

        private Fix64 _currentDamageScaling;


        public HitboxData HitboxData { get { return this._hitboxData; } set { this._hitboxData = value; } }
        public HitIndicator Indicator { get { return this._indicator; } set { this._indicator = value; } }
        public Vector3 ContactLoc { get { return this._contactLoc; } set { this._contactLoc = value; } }
        public VulnerableObject HurtCharacter { get { return this._hurtCharacter; } set { this._hurtCharacter = value; } }
        public CombatObject HitCharacter { get { return this._hitCharacter; } set { this._hitCharacter = value; } }
        public Fix64 CurrentDamageScaling { get { return this._currentDamageScaling; } set { this._currentDamageScaling = value; } }

        public HitInfo(HitboxData hd, HitIndicator hi, Vector3 cl, VulnerableObject huo, CombatObject hio)
        {
            this._hitboxData = hd;
            this._indicator = hi;
            this._contactLoc = cl;
            this._hurtCharacter = huo;
            this._hitCharacter = hio;
            this._currentDamageScaling = 0;
        }
        public HitInfo(HitboxData hd, HitIndicator hi, Vector3 cl, VulnerableObject vu, VulnerableObject huo, CombatObject hio, Fix64 cds)
        {
            this._hitboxData = hd;
            this._indicator = hi;
            this._contactLoc = cl;
            this._currentDamageScaling = cds;
        }
    }
}