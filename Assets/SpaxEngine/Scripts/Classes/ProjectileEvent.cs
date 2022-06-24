using UnityEngine;
using FixMath.NET;
namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class ProjectileEvent
    {
        [SerializeField] private GameObject _projectile;
        [SerializeField] private FVector2 _spawnOffset;
        [SerializeField] private Fix64 _spawnRotation;

        public GameObject Projectile { get { return this._projectile; } }
        public FVector2 SpawnOffset { get { return this._spawnOffset; } }
        public Fix64 SpawnRotation { get { return this._spawnRotation; } }



    }
}