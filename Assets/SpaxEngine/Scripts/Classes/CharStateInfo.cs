using FightingGameEngine.Gameplay;
namespace FightingGameEngine.Data
{
    public class CharStateInfo
    {
        private soCharacterStatus _charStatus;
        private HitboxTrigger[] _hitboxes;
        private HurtboxTrigger[] _hurtboxes;

        public CharStateInfo(in soCharacterStatus status, in HitboxTrigger[] hitboxes, in HurtboxTrigger[] hurtboxes)
        {
            this._charStatus = status;
            this._hitboxes = hitboxes;
            this._hurtboxes = hurtboxes;
        }

        public soCharacterStatus GetStatus() { return this._charStatus; }
        public HitboxTrigger[] GetHitboxes() { return this._hitboxes; }
        public HurtboxTrigger[] GetHurtboxes() { return this._hurtboxes; }
    }
}