using UnityEngine;
using FixMath.NET;
using FlatPhysics.Filter;
namespace FlatPhysics.Unity
{
    public class FlatWorldMono : MonoBehaviour
    {

        public static FlatWorldMono instance;
        private FlatWorld _world;
        private Fix64 _timeStep;
        private CollisionLayer[] _collisionMatrix;
        void Awake()
        {
            this._collisionMatrix = new CollisionLayer[16];
            int len = 16;
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    bool collides = !Physics.GetIgnoreLayerCollision(i, j);

                    if (collides)
                    {
                        this._collisionMatrix[i] |= (CollisionLayer)(1 << j);
                    }
                }
            }

            instance = this;
            this._world = new FlatWorld();
            this._timeStep = (Fix64)1 / (Fix64)60;
        }

        void Update()
        {
            this._world.Step(this._timeStep, 128);
        }

        public void AddBody(FRigidbody rb)
        {
            this._world.AddBody(rb.Body);
        }

        public CollisionLayer GetCollisions(int layer) { return this._collisionMatrix[layer]; }
    }
}
