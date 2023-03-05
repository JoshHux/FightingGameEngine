using FixMath.NET;
using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class SetGravityEvent : ICommand
    {
        private Fix64 _g;
        public SetGravityEvent(Fix64 g)
        {
            this._g = g;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            lobj.Status.CurrentGravity = this._g;
        }
    }
}