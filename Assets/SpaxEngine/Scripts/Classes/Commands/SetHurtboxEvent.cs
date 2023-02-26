using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;


namespace FightingGameEngine.Commands
{
    public class SetHurtboxEvent : ICommand
    {
        private HurtboxHolder _data;
        public SetHurtboxEvent(HurtboxHolder data)
        {
            this._data = data;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            (lobj as VulnerableObject).ActivateHurtboxes(this._data);
        }
    }
}