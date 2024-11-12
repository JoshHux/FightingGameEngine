using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;


namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class SetHitboxEvent : ICommand
    {
        private HitboxHolder _data;
        public SetHitboxEvent(HitboxHolder data)
        {
            this._data = data;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            //UnityEngine.Debug.Log("here we are!");
            //UnityEngine.Debug.Log(this._data.Hitbox0.Duration);
            (lobj as CombatObject).ActivateHitboxes(this._data);
        }
    }
}