using FixMath.NET;
using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class ApplyVelEvent : ICommand
    {
        private FVector2 _vel;

        public ApplyVelEvent(FVector2 vel)
        {
            this._vel = vel;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            status.CalcVelocity += this._vel;
            UnityEngine.Debug.Log(this._vel.x);
        }
    }
}