using FixMath.NET;
using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class PushboxSetActiveEvent : ICommand
    {
        [UnityEngine.SerializeField] private bool _active;
        public PushboxSetActiveEvent(bool a)
        {
            this._active = a;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            lobj.SetActivePushbox(this._active);
        }
    }
}