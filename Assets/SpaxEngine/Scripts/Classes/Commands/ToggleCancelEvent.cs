using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class ToggleCancelEvent : ICommand
    {
        private CancelConditions _cancel;
        public ToggleCancelEvent(CancelConditions c)
        {
            this._cancel = c;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            lobj.Status.CancelFlags ^= this._cancel;
        }
    }
}