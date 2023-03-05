using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class ToggleConditionEvent : ICommand
    {
        private StateConditions _cond;
        public ToggleConditionEvent(StateConditions c)
        {
            this._cond = c;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            lobj.Status.CurrentStateConditions ^= this._cond;
        }
    }
}