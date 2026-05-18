using FixMath.NET;
using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class SetPushboxDimensions : ICommand
    {
        [UnityEngine.SerializeField] private FVector2 _dim;
        public SetPushboxDimensions(FVector2 a)
        {
            this._dim = a;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            lobj.SetPushboxDimensions(this._dim);
        }
    }
}