using FixMath.NET;
using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    public class SetVelEvent : ICommand
    {
        private FVector2 _vel;
        public SetVelEvent(FVector2 vel)
        {
            this._vel = vel;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            status.CurrentVelocity = FVector2.zero;
            status.CalcVelocity = this._vel;
        }
    }
}
