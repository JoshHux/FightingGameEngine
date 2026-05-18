using UnityEngine;
using UnityEngine.InputSystem;
using FightingGameEngine.Gameplay;
using System.Threading;
using System.Runtime.CompilerServices;
namespace FightingGameEngine.CharacterSelect
{
    public class CharacterSelector : MonoBehaviour
    {
        [SerializeField] private soPlayerCharacterChoice _playerChoices;
        [SerializeField] private int _playerInd;

        [SerializeField] private GameObject _preSelectedIcon;
        [SerializeField] private GameObject _selectedIcon;
        [SerializeField] private float _moveCooldown = 0.1f;
        private AudioSource _audioSource;

        private int m_currentInd;
        private bool m_chosen;


        private float m_moveTimer;

        Gamepad pad1, pad2;
        Keyboard keyboard;

        public bool Chosen { get { return this.m_chosen; } }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.m_currentInd = 0;
            if (this._playerInd == 1) { this.m_currentInd = 1; }

            this.m_chosen = false;

            this.CorrectOffset();

            this._preSelectedIcon.SetActive(true);
            this._selectedIcon.SetActive(false);


            this.m_moveTimer = 0;

            this._audioSource = this.GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            if (this.m_moveTimer > 0) { this.m_moveTimer -= Time.deltaTime; }
            ManageControllers();//It must be called before any key read attempt wether on the main update or elsewhere

            var toCheck = this._playerChoices.P1ControlType;
            if (this._playerInd == 1) { toCheck = this._playerChoices.P2ControlType; }

            switch (toCheck)
            {
                case controllers.pad1:
                    ReadInputsFromController1();
                    break;
                case controllers.pad2:
                    ReadInputsFromController2();
                    break;
                case controllers.KeyboardP1:
                    ReadInputsFromKeyboardP1();
                    break;
                case controllers.KeyboardP2:
                    ReadInputsFromKeyboardP2();
                    break;
                default:
                    break;
            }
        }

        private void ManageControllers()
        {
            InputSystem.onDeviceChange +=
            (device, change) =>
            {
                switch (change)
                {
                    case InputDeviceChange.Added:
                        Debug.Log("New device added: " + device);
                        // New Device.
                        break;
                    case InputDeviceChange.Disconnected:
                        Debug.Log("Device Disconnected: " + device);
                        // Device got unplugged.
                        break;
                    case InputDeviceChange.Reconnected:
                        Debug.Log("Device Reconnected: " + device);
                        // Plugged back in.
                        break;
                    case InputDeviceChange.Removed:
                        Debug.Log("Device removed: " + device);
                        // Remove from Input System entirely; by default, Devices stay in the system once discovered.
                        break;
                    default:
                        // See InputDeviceChange reference for other event types.
                        break;
                }
            };
            pad1 = Gamepad.all.Count >= 1 ? Gamepad.all[0] : null;
            pad2 = Gamepad.all.Count > 1 ? Gamepad.all[1] : null;
            keyboard = Keyboard.current;
        }
        private void ReadInputsFromController1()
        {
            if (pad1 == null)
            {
                return;
            }

            if (pad1.dpad.left.isPressed) { this.IncrementLeft(); }
            else if (pad1.dpad.right.isPressed) { this.IncrementRight(); }
            else if (pad1.buttonSouth.isPressed) { this.LockIn(); }
            else if (pad1.buttonWest.isPressed) { this.BackOut(); }

        }
        private void ReadInputsFromController2()
        {
            if (pad2 == null)
            {
                return;
            }
            if (pad2.dpad.left.isPressed) { this.IncrementLeft(); }
            else if (pad2.dpad.right.isPressed) { this.IncrementRight(); }
            else if (pad2.buttonSouth.isPressed) { this.LockIn(); }
            else if (pad2.buttonWest.isPressed) { this.BackOut(); }

        }
        private void ReadInputsFromKeyboardP1()
        {
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.aKey.isPressed) { this.IncrementLeft(); }
            else if (keyboard.dKey.isPressed) { this.IncrementRight(); }
            else if (keyboard.jKey.isPressed) { this.LockIn(); }
            else if (keyboard.kKey.isPressed) { this.BackOut(); }


        }
        private void ReadInputsFromKeyboardP2()
        {
            if (keyboard == null)
            {
                return;
            }
            if (keyboard.leftArrowKey.isPressed) { this.IncrementLeft(); }
            else if (keyboard.rightArrowKey.isPressed) { this.IncrementRight(); }
            else if (keyboard.numpad1Key.isPressed) { this.LockIn(); }
            else if (keyboard.numpad2Key.isPressed) { this.BackOut(); }
        }

        private void IncrementLeft()
        {
            if (this.m_moveTimer > 0 || this.m_chosen) { return; }
            var cardCount = CharacterSelectManager.Instance.CharacterSelectCards.Count;
            this.m_currentInd = (this.m_currentInd - 1 + cardCount) % cardCount;
            this.CorrectOffset();
            this.m_moveTimer = this._moveCooldown;

            this._audioSource.clip = CharacterSelectManager.Instance.MovingSound;
            this._audioSource.Play();
        }

        private void IncrementRight()
        {
            if (this.m_moveTimer > 0 || this.m_chosen) { return; }
            var cardCount = CharacterSelectManager.Instance.CharacterSelectCards.Count;
            this.m_currentInd = (this.m_currentInd - 1 + cardCount) % cardCount;
            this.CorrectOffset();
            this.m_moveTimer = this._moveCooldown;

            this._audioSource.clip = CharacterSelectManager.Instance.MovingSound;
            this._audioSource.Play();
        }

        private void CorrectOffset()
        {
            var offset = new Vector3(-1, 2, 0);
            if (this._playerInd == 1) { offset = new Vector3(1, -2, 0); }

            this.transform.position = CharacterSelectManager.Instance.CharacterSelectCards[this.m_currentInd].Card.transform.position + offset;


        }

        private void LockIn()
        {
            if (this.m_chosen) { return; }
            if (this._playerInd == 0)
            {
                CharacterSelectManager.Instance.PlayerChoices.P1 = CharacterSelectManager.Instance.CharacterSelectCards[this.m_currentInd].CharacterPrefab;
                CharacterSelectManager.Instance.P1ChosenCharacter = CharacterSelectManager.Instance.CharacterSelectCards[this.m_currentInd].CharacterID;
            }
            else
            {
                CharacterSelectManager.Instance.PlayerChoices.P2 = CharacterSelectManager.Instance.CharacterSelectCards[this.m_currentInd].CharacterPrefab;
                CharacterSelectManager.Instance.P2ChosenCharacter = CharacterSelectManager.Instance.CharacterSelectCards[this.m_currentInd].CharacterID;
            }

            this._preSelectedIcon.SetActive(false);
            this._selectedIcon.SetActive(true);

            this.m_chosen = true;

            this._audioSource.clip = CharacterSelectManager.Instance.SelectedSound;
            this._audioSource.Play();

        }

        private void BackOut()
        {
            if (!this.m_chosen) { return; }

            if (this._playerInd == 0)
            {
                CharacterSelectManager.Instance.PlayerChoices.P1 = null;
                CharacterSelectManager.Instance.P1ChosenCharacter = -1;
            }
            else
            {
                CharacterSelectManager.Instance.PlayerChoices.P2 = null;
                CharacterSelectManager.Instance.P2ChosenCharacter = -1;
            }

            this._preSelectedIcon.SetActive(true);
            this._selectedIcon.SetActive(false);

            this.m_chosen = false;

            this._audioSource.clip = CharacterSelectManager.Instance.UnselectedSound;
            this._audioSource.Play();

        }
    }
}