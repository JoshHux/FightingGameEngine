using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using FlatPhysics;
using FlatPhysics.Unity;
using FlatPhysics.Filter;
using FightingGameEngine;


namespace Spax
{
    public class SpaxManager : MonoBehaviour
    {
        //public FixedAnimationController test;
        public static SpaxManager Instance;

        public delegate void InputUpdateEventHandler();
        public delegate void StateUpdateEventHandler();
        public delegate void StateCleanUpdateEventHandler();
        public delegate void PreUpdateEventHandler();
        public delegate void SpaxUpdateEventHandler();
        public delegate void PostPhysUpdateEventHandler();
        public delegate void HitQueryUpdateEventHandler();
        public delegate void HurtQueryUpdateEventHandler();
        public delegate void PostUpdateEventHandler();
        public delegate void RenderUpdateEventHandler();
        public delegate void RenderPrepEventHandler();
        public delegate void PreRenderEventHandler();
        public event InputUpdateEventHandler InputUpdate;
        public event StateUpdateEventHandler StateUpdate;
        public event StateCleanUpdateEventHandler StateCleanUpdate;
        public event PreUpdateEventHandler PreUpdate;
        public event SpaxUpdateEventHandler SpaxUpdate;
        public event PostUpdateEventHandler PostPhysUpdate;
        public event HitQueryUpdateEventHandler HitQueryUpdate;

        public event HurtQueryUpdateEventHandler HurtQueryUpdate;
        public event PostUpdateEventHandler PostUpdate;
        public event RenderUpdateEventHandler RenderUpdate;
        public event RenderPrepEventHandler PrepRender;
        public event RenderPrepEventHandler PreRender;

        //static values for stuff like input buffer and leniency
        [SerializeField] private soStaticValues _staticValues;

        private FlatWorld _world;
        private Fix64 _timeStep;
        private CollisionLayer[] _collisionMatrix;

        public soStaticValues StaticValues { get { return this._staticValues; } }

        //for initializing the physics and filtering collisions
        //private CollisionGroup[] groups;
        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            //test.Initialize();
            //initialize physics world stuff
            //collision layer stuff
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
            //asssign world
            this._world = new FlatWorld();
            this._timeStep = (Fix64)1 / (Fix64)60;

        }

        void Start()
        {
            var s = FindObjectsOfType<SpaxBehavior>();
            int len = s.Length;
            for (int i = 0; i < len; i++)
            {
                var hold = s[i];
                hold.SpaxAwake();
            }
            for (int i = 0; i < len; i++)
            {
                var hold = s[i];
                hold.SpaxStart();
            }
        }

        void FixedUpdate()
        {
            GameplayUpdate();
            RendererUpdate();

        }

        private void GameplayUpdate()
        {
            InputUpdate?.Invoke();
            StateUpdate?.Invoke();
            StateCleanUpdate?.Invoke();
            PreUpdate?.Invoke();
            SpaxUpdate?.Invoke();
            //m_space.Update();
            //UpdatePhysics();
            //world.PhysUpdate();
            this._world.Step(this._timeStep, 128);
            PostPhysUpdate?.Invoke();
            HitQueryUpdate?.Invoke();
            HurtQueryUpdate?.Invoke();
            PostUpdate?.Invoke();
            PrepRender?.Invoke();
        }
        private void RendererUpdate()
        {
            PreRender?.Invoke();
            RenderUpdate?.Invoke();
        }



        public void AddBody(FRigidbody rb)
        {
            this._world.AddBody(rb.Body);
        }


        public void RemoveBody(FRigidbody rb)
        {
            //Debug.Log("adding " + rb.name + " " + (rb.Body == null));
            this._world.RemoveBody(rb.Body);
        }

        public CollisionLayer GetCollisions(int layer) { return this._collisionMatrix[layer]; }

    }
}