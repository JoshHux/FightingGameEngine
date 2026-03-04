using FixMath.NET;
using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class ProjectileSpawnEvent : ICommand
    {
        [UnityEngine.SerializeField] int _p;
        [UnityEngine.SerializeField] FVector2 _pos;
        [UnityEngine.SerializeField] Fix64 _rot;

        public ProjectileSpawnEvent(int p, FVector2 pos, Fix64 rot)
        {
            this._p = p;
            this._pos = pos;
            this._rot = rot;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            lobj.SpawnProjectile(this._p, this._pos, this._rot);
        }
    }
}