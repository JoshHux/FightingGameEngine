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
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using PleaseResync;


namespace Spax
{
    public class SpaxManager : PleaseResyncManager
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

        [SerializeField] private Material _p2Material;

        //static values for stuff like input buffer and leniency
        [SerializeField] private soStaticValues _staticValues;
        [SerializeField] private soUniversalStateHolder _universalStates;
        //just anobject to know which characters the players have chosen
        [SerializeField] private soPlayerCharacterChoice _playerChoices;
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
        //how many rounds a player needs to win before they win the game
        [SerializeField] private int _roundsToWin = 5;
        //# of frames elapsed in a match; differs from currentFrame, which continually increments
        [SerializeField] private int matchFrames = 0;

        [Space(20)]
        [SerializeField] private UIPlayerCanvas P1UI;
        [SerializeField] private UIPlayerCanvas P2UI;
        [SerializeField] private soCharacterStatus P1Status;
        [SerializeField] private soCharacterStatus P2Status;
        [SerializeField] private soCharacterData P1Data;
        [SerializeField] private soCharacterData P2Data;

        [SerializeField] private WinUiHandler _winUI;

        [Space(30)]
        public int currentFrame;
        public Text currentFrameText;
        public bool paused = false;

        [Space(30)]
        [SerializeField] private soWorldRecorder _worldStates;
        [SerializeField] private bool _recordWorld;
        [SerializeField] private int _replayWorldStartFrame;
        [SerializeField] private int _recWorldFrame;

        private bool m_gameStillGoing;

        private bool m_acceptInputs;
        private int m_preroundTime;

        public bool AcceptInputs { get { return this.m_acceptInputs; } }

        public List<FightingCharacterController> Players { get { return this._players; } }

        public bool IsTraining = false;


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

            this.m_gameStillGoing = true;

            this.m_acceptInputs = false;
            this.m_preroundTime = 30;

        }

        void Start()
        {
            //instantiate the players
            var p1 = Instantiate(this._playerChoices.P1).GetComponent<FightingCharacterController>();
            var p2 = Instantiate(this._playerChoices.P2).GetComponent<FightingCharacterController>();

            Debug.Log(p1);
            Debug.Log(p2);

            p1.controllerToUse = this._playerChoices.P1ControlType;
            p2.controllerToUse = this._playerChoices.P2ControlType;

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

            p1.Other = p2;
            p2.Other = p1;
            p1.Status = this.P1Status;
            p2.Status = this.P2Status;

            var p2Anim = p2.GetComponent<AnimatingObject>().PlayerAnimator;

            p1.GetComponent<AnimatingObject>().Status = P1Status;
            p2.GetComponent<AnimatingObject>().Status = P2Status;
            P1UI.CharacterData = p1.Data;
            P1UI.CharacterStatus = p1.Status;

            P2UI.CharacterData = p2.Data;
            P2UI.CharacterStatus = p2.Status;

            //if they're the same character, recolor p2
            if (p1.Data == p2.Data)
            {
                var rend = p2Anim.GetComponentsInChildren<SkinnedMeshRenderer>();
                //Debug.Log(rend.Length);
                len = rend.Length;
                for (int i = 0; i < len; i++)
                {
                    var renHold = rend[i];
                    renHold.material = p1.Data.P2Mat;
                }
            }

            //add the players to the list
            this._players.Add(p1);
            this._players.Add(p2);
            //set them to the correct positions

            this._players[0].SetPosition(new FVector2(-5, 0));
            this._players[1].SetPosition(new FVector2(5, 0));

        }

        void FixedUpdate()
        {
            if (this.m_preroundTime > 0) { this.m_preroundTime -= 1; } else { this.m_acceptInputs = true; }
            if (this.m_gameStillGoing && this._players.Count > 1 && this._players[0].Ready && this._players[1].Ready)
            {
                if (matchFrames == 0)
                {
                    this.ResetRound();
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
                        if (this.matchFrames > -1) { currentFrame++; matchFrames++; }

                    }
                }
                else
                {
                    //if (Input.GetKeyDown("p"))
                    //{
                    //    paused = true;
                    //}
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


                    if (this.matchFrames > -1) { currentFrame++; matchFrames++; }

                }

                //TODO :: make a child script of this for training mode manager
                //we want to reset in training mode
                if (this.matchFrames < 0 && Keyboard.current.enterKey.isPressed) { this.ResetRound(); }

                currentFrameText.text = currentFrame.ToString();

                _roundTimer.text = ((int)(maxTime - ((Fix64)matchFrames / secondFrames))).ToString();
                if (this.paused) { _roundTimer.text += "\nFRAME BY FRAME MODE"; }

                if ((Fix64)matchFrames / secondFrames >= maxTime)
                {
                    TimeOut();
                }
            }
            else if (!this.m_gameStillGoing)
            {
                //rematch
                if (Keyboard.current.spaceKey.isPressed) { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
                if (Keyboard.current.backspaceKey.isPressed) { SceneManager.LoadScene("CharacterSelectScreen"); }

            }
        }

        private int m_cooldown = 0;
        private void GameplayUpdate()
        {

            if (this.m_cooldown <= 0 && Keyboard.current.vKey.isPressed)
            {
                this.RecordWorldState();
                this.m_cooldown = 15;

            }
            else if (Keyboard.current.bKey.isPressed)
            {
                this.ApplyWorldState(this._worldStates.WorlStateCount - 1);
            }

            this.m_cooldown -= 1;


            if (this._recordWorld)
            {

                //we want to replay the world
                if (this._replayWorldStartFrame > 0 && this.matchFrames >= this._replayWorldStartFrame)
                {
                    this.ApplyWorldState(this._recWorldFrame);
                    this._recWorldFrame++;
                }
                //we want to record the world
                else
                {
                    this.RecordWorldState();
                }
                this._recordWorld = false;
            }
            InputUpdate?.Invoke();
            StateUpdate?.Invoke();
            StateCleanUpdate?.Invoke();
            PreUpdate?.Invoke();
            SpaxUpdate?.Invoke();
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
            //we already added the players in a specific order, let's not mess around more
            //FightingCharacterController livingObject = null;
            //
            //bool got = rb.TryGetComponent<FightingCharacterController>(out livingObject);
            //if (got)
            //{
            //    this._players.Add(livingObject);
            //
            //}

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
            if (this.IsTraining) { return; }


            //P1Status.CurrentHP = P1Data.MaxResources.Health;
            //P2Status.CurrentHP = P2Data.MaxResources.Health;

            //0 = P1, 1 = P2 to match player IDs and also lol programming counting
            if (lostPlayer == 0)
            {
                P2UI.WinRound();
                if (P2UI.RoundsWon >= this._roundsToWin) { this.EndRound(1); }
            }
            else if (lostPlayer == 1)
            {
                P1UI.WinRound();
                if (P1UI.RoundsWon >= this._roundsToWin) { this.EndRound(0); }
            }
            else
            {
                //for starting rounds without round increments?
            }

            this.m_preroundTime = 30;
            this.m_acceptInputs = false;
            ResetRound();

            //            StartCoroutine(PauseBeforeNextRound());

        }

        private void TimeOut()
        {
            //matchFrames = 0;
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

        private void ResetRound()
        {

            //P1Status.CurrentHP = P1Data.MaxResources.Health;
            //P2Status.CurrentHP = P2Data.MaxResources.Health;

            //this._players[0].SetPosition(new FVector2(-5, 0));
            //this._players[1].SetPosition(new FVector2(5, 0));

            //this._players[0].ResetObject();
            //this._players[1].ResetObject();
            Debug.Log("reached");
            this.ApplyWorldState(0);
            if (this.matchFrames > -1) { matchFrames = 0; }
        }
        private void StartRound() { }

        private void EndRound(int winningPlayer)
        {
            this.m_gameStillGoing = false;
            //this._winUI.gameObject.SetActive(true);
            this._winUI.TriggerWin(winningPlayer);
        }

        private void ApplyWorldState(int worldInd)
        {
            if (worldInd >= this._worldStates.GetWorldStateCount()) { return; }
            this._players[0].ApplyGameplayState(this._worldStates.GetWorldState(worldInd).Player1, this._worldStates.P1Inputs);
            this._players[1].ApplyGameplayState(this._worldStates.GetWorldState(worldInd).Player2, this._worldStates.P2Inputs);

        }

        private void RecordWorldState()
        {

            //Debug.Log("reached");
            CharStateInfo[] hold = new CharStateInfo[] { this._players[0].GetCharacterInfo(), this._players[1].GetCharacterInfo() };

            this._worldStates.AddWorldState(new WorldState(hold));

            //if the input states are null, update them
            if (this._worldStates.P1Inputs == null)
            {
                this._worldStates.P1Inputs = new List<InputSnapshot>(this._players[0].Status.RecordedInputs);
                this._worldStates.P2Inputs = new List<InputSnapshot>(this._players[0].Status.RecordedInputs);
            }
        }

        IEnumerator PauseBeforeNextRound()
        {
            yield return new WaitForSeconds(0.3f);
            ResetRound();

        }


        public override void OnlineGame(bool spectate, uint players, uint spectators, uint ID)
        {
            CharStateInfo[] hold = new CharStateInfo[] { this._players[0].GetCharacterInfo(), this._players[1].GetCharacterInfo() };
            StartOnlineGame(new WorldState(hold), spectate, players, spectators, ID);
        }

        public override void LocalGame(uint players)
        {
            CharStateInfo[] hold = new CharStateInfo[] { this._players[0].GetCharacterInfo(), this._players[1].GetCharacterInfo() };

            StartOfflineGame(new WorldState(hold), players);
        }

        public override void ReplayMode(uint players)
        {
            CharStateInfo[] hold = new CharStateInfo[] { this._players[0].GetCharacterInfo(), this._players[1].GetCharacterInfo() };

            StartReplay(new WorldState(hold), players);
        }
    }
}