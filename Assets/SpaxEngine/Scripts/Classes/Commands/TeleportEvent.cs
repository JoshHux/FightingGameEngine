using FixMath.NET;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;
using FightingGameEngine.Gameplay;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class TeleportEvent : ICommand
    {
        private FVector2 _pos;
        private TeleportMode _mode;
        public TeleportEvent(FVector2 p, TeleportMode m)
        {
            this._pos = p;
            this._mode = m;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {

            if (this._mode > 0)
            {
                //we want to teleport, is the target position player or opponent relative?
                int pRel = (int)this._mode & 1;
                int oRel = (int)((((uint)this._mode) & 2) >> 1);

                var targetPos = pRel * (lobj.Status.CurrentPosition + this._pos) + oRel * ((lobj as FightingCharacterController).get_other().get_position() + this._pos);

                //set the new position
                lobj.Status.CurrentPosition = targetPos;
            }

        }
    }
}