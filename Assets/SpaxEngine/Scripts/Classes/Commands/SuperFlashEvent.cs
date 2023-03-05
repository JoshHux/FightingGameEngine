using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class SuperFlashEvent : ICommand
    {
        private int _dur;
        public SuperFlashEvent(int d)
        {
            this._dur = d;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            lobj.StartSuperFlash(this._dur);
        }
    }
}