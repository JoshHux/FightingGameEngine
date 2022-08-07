using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using FlatPhysics;
using FlatPhysics.Unity;
using FlatPhysics.Filter;
using FightingGameEngine;
using FightingGameEngine.Data;
using FightingGameEngine.Gameplay;


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
        [SerializeField] private soUniversalStateHolder _universalStates;
        private FlatWorld _world;
        private Fix64 _timeStep;
        [SerializeField] private CollisionLayer[] _collisionMatrix;

        private List<FightingCharacterController> _players;

        public soStaticValues StaticValues { get { return this._staticValues; } }
        public soUniversalStateHolder UniversalStates { get { return this._universalStates; } }

        //for initializing the physics and filtering collisions
        //private CollisionGroup[] groups;
        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;

            this._players = new List<FightingCharacterController>();
            //test.Initialize();
            //initialize physics world stuff
            //collision layer stuff
            this._collisionMatrix = new CollisionLayer[16];
            int len = 10;
            for (int i = 0; i < len; i++)
            {
                this._collisionMatrix[i] = 0;
                for (int j = 0; j < len; j++)
                {
                    bool collides = !Physics.GetIgnoreLayerCollision(i, j);

                    if (collides)
                    {
                        this._collisionMatrix[i] |= (CollisionLayer)(1 << j);
                        //Debug.Log((CollisionLayer)(1 << j) + " | " + i + ", " + j);
                    }
                }
                //Debug.Log(i + " :: " + this._collisionMatrix[i]);
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
                //hold.SpaxAwake();
            }
            for (int i = 0; i < len; i++)
            {
                var hold = s[i];
                //hold.SpaxStart();
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

            FightingCharacterController livingObject = null;

            bool got = rb.TryGetComponent<FightingCharacterController>(out livingObject);
            if (got)
            {
                this._players.Add(livingObject);
            }

        }


        public void RemoveBody(FRigidbody rb)
        {
            //Debug.Log("adding " + rb.name + " " + (rb.Body == null));
            this._world.RemoveBody(rb.Body);
        }

        public void ResolveRepositioning(FlatBody body1, FlatBody body2)
        {
            this._world.ResolveAgainstAllStatic(body1, body2);
        }

        public FightingCharacterController GetLivingObjectByID(int playerID)
        {
            var ret = this._players.Find(o => o.PlayerID == playerID);

            return ret;
        }

        public CollisionLayer GetCollisions(int layer)
        { return this._collisionMatrix[layer]; }
        public int GetNumberOfPlayers() { return this._players.Count; }

    }
}