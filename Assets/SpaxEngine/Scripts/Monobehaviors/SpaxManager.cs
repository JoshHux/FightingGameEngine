using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using FlatPhysics;
using FlatPhysics.Unity;
using FlatPhysics.Filter;
using FightingGameEngine;
using FightingGameEngine.Data;
using FightingGameEngine.Gameplay;
using UnityEngine.UI;
using System.IO;

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

        [SerializeField, ReadOnly] private List<FightingCharacterController> _players;

        public soStaticValues StaticValues { get { return this._staticValues; } }
        public soUniversalStateHolder UniversalStates { get { return this._universalStates; } }

        //for initializing the physics and filtering collisions
        //private CollisionGroup[] groups;

        [Space(20)]
        [SerializeField] private Text _roundTimer;
        //time (seconds) that a round takes
        [SerializeField] private int maxTime = 99;
        //# of frames for a second according to timer; 60 for accuracy, but can be set inaccurately for aesthetic purposes
        [SerializeField] private int secondFrames = 60;
        //# of frames elapsed in a match; differs from currentFrame, which continually increments
        private int matchFrames = 0;

        [Space(20)]
        [SerializeField] private UIPlayerCanvas P1UI;
        [SerializeField] private UIPlayerCanvas P2UI;
        [SerializeField] private soCharacterStatus P1Status;
        [SerializeField] private soCharacterStatus P2Status;
        [SerializeField] private soCharacterData P1Data;
        [SerializeField] private soCharacterData P2Data;

        [Space(30)]
        public int currentFrame;
        public Text currentFrameText;
        public bool paused = false;

        [Space(30)]
        [SerializeField] private soWorldRecorder _worldStates;
        [SerializeField] private bool _recordWorld;
        [SerializeField] private int _replayWorldStartFrame;
        [SerializeField] private int _recWorldFrame;



        public List<int> bodyids;
        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;

            this._players = new List<FightingCharacterController>();
            this._worldStates.Init();
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

            this._recWorldFrame = 0;

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
            if (matchFrames == 0)
            {

                GetLivingObjectByID(0).SetPosition(new FVector2(5, 0));
                GetLivingObjectByID(1).SetPosition(new FVector2(-5, 0));
            }
            if (paused)
            {
                if (Input.GetKeyDown("p"))
                {
                    paused = false;
                }
                if (Input.GetKeyDown("u"))
                {
                    //GameplayUpdate();
                    //GameplayUpdate();
                    //GameplayUpdate();
                    //GameplayUpdate();
                    //GameplayUpdate();
                    //GameplayUpdate();
                    //GameplayUpdate();
                    //GameplayUpdate();
                    GameplayUpdate();


                    RendererUpdate();
                    currentFrame++;
                    matchFrames++;
                }
            }
            else
            {
                if (Input.GetKeyDown("p"))
                {
                    paused = true;
                }
                //GameplayUpdate();
                //GameplayUpdate();
                //GameplayUpdate();
                //GameplayUpdate();
                //GameplayUpdate();
                //GameplayUpdate();
                //GameplayUpdate();
                //GameplayUpdate();
                GameplayUpdate();
                RendererUpdate();
                currentFrame++;
                matchFrames++;
            }
            currentFrameText.text = currentFrame.ToString();

            _roundTimer.text = ((int)(maxTime - ((Fix64)matchFrames / secondFrames))).ToString();

            if ((Fix64)matchFrames / secondFrames >= maxTime)
            {
                TimeOut();
            }
        }

        private void GameplayUpdate()
        {
            if (this._recordWorld)
            {


                if (this.matchFrames >= this._replayWorldStartFrame)
                {
                    this.ApplyWorldState(this._recWorldFrame);
                    this._recWorldFrame++;
                }
                else
                {
                    CharStateInfo[] hold = new CharStateInfo[] { this._players[0].GetCharacterInfo(), this._players[1].GetCharacterInfo() };
                    this._worldStates.AddWorldState(new WorldState(hold));
                }
            }
            InputUpdate?.Invoke();
            StateUpdate?.Invoke();
            StateCleanUpdate?.Invoke();
            PreUpdate?.Invoke();
            SpaxUpdate?.Invoke();
            //m_space.Update();
            //UpdatePhysics();
            //world.PhysUpdate();
            this._world.Step(this._timeStep);
            PostPhysUpdate?.Invoke();
            HitQueryUpdate?.Invoke();
            HurtQueryUpdate?.Invoke();
            PostUpdate?.Invoke();
            PrepRender?.Invoke();


            this.bodyids = this._world.GetBodyIds();
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

        public FlatBody FindBody(FRigidbody rb)
        {
            //Debug.Log("adding " + rb.name + " " + (rb.Body == null));
            return this._world.FindBody(rb);
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

        public void StartRound(int lostPlayer)
        {
            matchFrames = 0;

            GetLivingObjectByID(0).SetPosition(new FVector2(5, 1));
            GetLivingObjectByID(1).SetPosition(new FVector2(-5, 1));

            P1Status.CurrentHP = P1Data.MaxResources.Health;
            P2Status.CurrentHP = P2Data.MaxResources.Health;
            //0 = P1, 1 = P2 to match player IDs and also lol programming counting
            if (lostPlayer == 0)
            {
                P2UI.WinRound();
            }
            else if (lostPlayer == 1)
            {
                P1UI.WinRound();
            }
            else
            {
                //for starting rounds without round increments?
            }
        }

        private void TimeOut()
        {
            matchFrames = 0;
            if (P1Status.CurrentHP > P2Status.CurrentHP)
            {
                StartRound(1);
            }
            else if (P2Status.CurrentHP > P1Status.CurrentHP)
            {
                StartRound(0);
            }
            else
            {
                StartRound(-1);
            }
        }

        private void ApplyWorldState(int worldInd)
        {
            if (worldInd >= this._worldStates.GetWorldStateCount()) { return; }
            this._players[0].ApplyGameplayState(this._worldStates.GetWorldState(worldInd).Player2);
            this._players[1].ApplyGameplayState(this._worldStates.GetWorldState(worldInd).Player1);
        }
    }
}