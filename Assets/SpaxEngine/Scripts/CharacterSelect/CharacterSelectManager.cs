using System.Collections.Generic;
using FightingGameEngine.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace FightingGameEngine.CharacterSelect
{
    public class CharacterSelectManager : MonoBehaviour
    {

        public static CharacterSelectManager Instance;

        private int m_p1ChosenCharacter;
        private int m_p2ChosenCharacter;

        [SerializeField] private soPlayerCharacterChoice _playerChoices;
        [SerializeField] private List<CharacterSelectCard> _characterSelectCards;
        [SerializeField] private AudioClip _movingSound;
        [SerializeField] private AudioClip _selectedSound;
        [SerializeField] private AudioClip _unselectedSound;


        public int P1ChosenCharacter { get { return this.m_p1ChosenCharacter; } set { this.m_p1ChosenCharacter = value; } }
        public int P2ChosenCharacter { get { return this.m_p2ChosenCharacter; } set { this.m_p2ChosenCharacter = value; } }

        public List<CharacterSelectCard> CharacterSelectCards { get { return this._characterSelectCards; } }
        public soPlayerCharacterChoice PlayerChoices { get { return this._playerChoices; } }
        public AudioClip MovingSound { get { return this._movingSound; } }
        public AudioClip SelectedSound { get { return this._selectedSound; } }
        public AudioClip UnselectedSound { get { return this._unselectedSound; } }



        void Awake()
        {
            CharacterSelectManager.Instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.m_p1ChosenCharacter = -1;
            this.m_p2ChosenCharacter = -1;

        }

        // Update is called once per frame
        void Update()
        {
            if (this.m_p1ChosenCharacter != -1 && this.m_p2ChosenCharacter != -1) { this.BothPlayersChoseCharacter(); }

        }

        public void SetPlayer1Controls(int c)
        {
            var set = controllers.KeyboardP1;
            switch (c)
            {
                case 1:
                    set = controllers.KeyboardP2;
                    break;
                case 2:
                    set = controllers.pad1;
                    break;
                case 3:
                    set = controllers.pad2;
                    break;
            }
            this._playerChoices.P1ControlType = set;
        }
        public void SetPlayer2Controls(int c)
        {
            var set = controllers.KeyboardP1;
            switch (c)
            {
                case 1:
                    set = controllers.KeyboardP2;
                    break;
                case 2:
                    set = controllers.pad1;
                    break;
                case 3:
                    set = controllers.pad2;
                    break;
            }
            this._playerChoices.P2ControlType = set;
        }

        private void BothPlayersChoseCharacter()
        {
            SceneManager.LoadScene("BattleScene");
        }

        private void PlayerChose(int player, int character)
        {
            if (player == 0)
            {
                this.m_p1ChosenCharacter = character;

            }
            else
            {
                this.m_p2ChosenCharacter = character;

            }

            if (this.m_p1ChosenCharacter != -1 && this.m_p2ChosenCharacter != -1) { this.BothPlayersChoseCharacter(); }

        }
    }

    [System.Serializable]
    public class CharacterSelectCard
    {
        [SerializeField] private int _characterID;
        [SerializeField] private GameObject _card;
        [SerializeField] private GameObject _characterPrefab;

        public int CharacterID { get { return this._characterID; } }
        public GameObject Card { get { return this._card; } }
        public GameObject CharacterPrefab { get { return this._characterPrefab; } }
    }
}