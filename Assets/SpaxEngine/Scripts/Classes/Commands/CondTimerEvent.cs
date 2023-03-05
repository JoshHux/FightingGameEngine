using FightingGameEngine.Data;
using FightingGameEngine.Enum;
using FightingGameEngine.Gameplay;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class CondTimerEvent : ICommand
    {
        int _dur;
        StateConditions _cond;

        public CondTimerEvent(int d, StateConditions c)
        {
            this._dur = d;
            this._cond = c;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            lobj.StartCondTimer(this._dur, this._cond);
        }
    }
}