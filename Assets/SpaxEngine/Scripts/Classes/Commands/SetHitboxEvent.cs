using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;


namespace FightingGameEngine.Commands
{
    public class SetHitboxEvent : ICommand
    {
        private HitboxHolder _data;
        public SetHitboxEvent(HitboxHolder data)
        {
            this._data = data;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            (lobj as CombatObject).ActivateHitboxes(this._data);
        }
    }
}