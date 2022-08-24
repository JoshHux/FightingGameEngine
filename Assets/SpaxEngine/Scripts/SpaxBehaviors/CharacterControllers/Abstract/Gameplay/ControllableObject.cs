using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FightingGameEngine.Enum;
using FightingGameEngine.Data;
using Spax;

namespace FightingGameEngine.Gameplay
{
    public abstract class ControllableObject : CombatObject
    {
        //Unity's input mapping, replace when given the opportunity
        [SerializeField] private InputActionAsset actions;
        [SerializeField] private bool _canControl = true;

        FightingCharacterController cont;
        SpaxManager manager;
        Gamepad pad1, pad2;
        Keyboard keyboard;
        
        protected override void OnAwake()
        {
            base.OnAwake();

            manager = FindObjectOfType<SpaxManager>();
            cont = transform.root.GetComponent<FightingCharacterController>();

            if (cont.inputMode == inputMod.Record)
            {
                cont.inputDateToRecordAndPlay.frameCommands.Clear();//Clear the data asset if we start recording, to avoid having more inputs than frame wich is not good, we want 1 input per frame (60 inputs/1s)
            }
            #region Old Input Mapping
            //init the status object, only really for the input recorder to get ready
            this.status.Initialize();
            //using the input system to do a little mapping, replace as soon as possible
            //pressed events
            /*actions["Direction"].performed += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, false, true);
            actions["Punch"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.A, false, false);
            actions["Kick"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.B, false, false);
            actions["Slash"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.C, false, false);
            actions["Dust"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.D, false, false);
            actions["Jump"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.W, false, false);
            actions["Block"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.X, false, false);
            //released events
            actions["Direction"].canceled += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, true, true);
            actions["Punch"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.A, true, false);
            actions["Kick"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.B, true, false);
            actions["Slash"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.C, true, false);
            actions["Dust"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.D, true, false);
            actions["Jump"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.W, true, false);
            actions["Block"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.X, true, false);

            actions["Direction"].Enable();
            actions["Punch"].Enable();
            actions["Kick"].Enable();
            actions["Slash"].Enable();
            actions["Dust"].Enable();
            actions["Jump"].Enable();
            actions["Block"].Enable();

            actions.Enable();*/
            #endregion
        }

        protected override void InputUpdate()
        {
            //Debug.Log("input updating");

            ManageControllers();//It must be called before any key read attempt wether on the main update or elsewhere
            switch (cont.controllerToUse)
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

            SimulateRecordPlayInputs();
            this.BufferInput();
        }

        private void ApplyInput(UnityEngine.Vector2 dir, InputEnum input, bool released, bool direction)
        {
            _canControl = cont.inputMode == inputMod.Play ? false : _canControl;

            if (!this._canControl)
            {
                return;
            }
            else
            {
                //check the direction
                //"current" controller state, for easy reference
                var curInput = this.status.CurrentControllerState;
                //getting the buttons only for later refrence
                var curInputBtn = curInput.Input & InputEnum.BUTTONS;
                var finalDir = curInput.Input & InputEnum.DIRECTIONS;
                if (direction)
                {
                    //get the x-direction from dir, X_ZERO is the default so we only need to check negative or positive values
                    var xDir = InputEnum.X_ZERO;
                    if (dir.x > 0) { xDir = InputEnum.X_POSITIVE; }
                    else if (dir.x < 0) { xDir = InputEnum.X_NEGATIVE; }

                    //get the y-direction from dir, Y_ZERO is the default so we only need to check negative or positive values
                    var yDir = InputEnum.Y_ZERO;
                    if (dir.y > 0) { yDir = InputEnum.Y_POSITIVE; }
                    else if (dir.y < 0) { yDir = InputEnum.Y_NEGATIVE; }
                    finalDir = xDir & yDir;
                }
                //buttons 
                curInputBtn |= (InputEnum)input;
                if (released) { curInputBtn &= (InputEnum)(~input); }
                //AND the xDir and yDir so that we get the final direction to be assigned to the CurrentControllerState in the InputRecorder object

                //final new InputEnum
                var finalInput = finalDir | curInputBtn;

                //new InputItem we assign as the CurrentControllerState
                var newCurState = new InputItem(finalInput);

                //assign new CurrentControllerState
                this.status.CurrentControllerState = newCurState;
            }
        }

        private void BufferInput()
        {
            //whether to add extra buffer leniency to the input to buffer aerials curing prejump
            bool bufferLeniency = EnumHelper.HasEnum((uint)this.status.TotalStateConditions, (uint)StateConditions.BUFFER_INPUT);
            //adds a new input, check if state can transition
            //buffers the current controller state, saved in the recorder itself
            this.status.BufferInput(bufferLeniency);
        }

        #region Input Recorder
        private void RecordInputsEachFrame(InputEnum command) {
            if (cont.inputMode == inputMod.Record)
            {
                cont.inputDateToRecordAndPlay.frameCommands.Add(command.ToString());
            }
        }
        private void PlayInputEachFrame(InputEnum command)
        {
            if (cont.inputMode == inputMod.Play)
            {
                if (manager.currentFrame > 0)
                {
                    if (manager.currentFrame < cont.inputDateToRecordAndPlay.frameCommands.Count)
                    {
                        command = (InputEnum)System.Enum.Parse(typeof(InputEnum), cont.inputDateToRecordAndPlay.frameCommands[manager.currentFrame]);

                        //new InputItem we assign as the CurrentControllerState
                        var newCurState = new InputItem(command);

                        //assign new CurrentControllerState
                        this.status.CurrentControllerState = newCurState;
                    }
                }
            }
        }
        private void SimulateRecordPlayInputs() {
            var curInput = this.status.CurrentControllerState;
            RecordInputsEachFrame(curInput.Input);
            PlayInputEachFrame(curInput.Input);
        }
        #endregion

        #region Multi  Controllers Handling
        private void ManageControllers() {
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
        private void ReadInputsFromController1() {
            if (pad1 == null)
            {
                return;
            }
            #region Left Stick/Dpad
            ApplyInput(new Vector2
                (
                !pad1.dpad.left.isPressed && !pad1.dpad.right.isPressed ? 0 : pad1.dpad.left.isPressed && !pad1.dpad.right.isPressed ? -1 : !pad1.dpad.left.isPressed && pad1.dpad.right.isPressed ? 1 : 0,
                !pad1.dpad.down.isPressed && !pad1.dpad.up.isPressed ? 0 : pad1.dpad.down.isPressed && !pad1.dpad.up.isPressed ? -1 : !pad1.dpad.down.isPressed && pad1.dpad.up.isPressed ? 1 : 0
                ), 0b0000000000000000, false, true);
            #endregion
            #region Shoulders
            if (pad1.rightShoulder.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.W, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.W, true, false);
            }
            #endregion
            #region Action Pad
            if (pad1.buttonNorth.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.Y, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.Y, true, false);
            }
            if (pad1.buttonSouth.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.A, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.A, true, false);
            }
            if (pad1.buttonEast.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.B, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.B, true, false);
            }
            if (pad1.buttonWest.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.X, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.X, true, false);
            }
            #endregion
        }
        private void ReadInputsFromController2()
        {
            if (pad2 == null)
            {
                return;
            }
            #region Left Stick/Dpad
            ApplyInput(new Vector2
                (
                !pad2.dpad.left.isPressed && !pad2.dpad.right.isPressed ? 0 : pad2.dpad.left.isPressed && !pad2.dpad.right.isPressed ? -1 : !pad2.dpad.left.isPressed && pad2.dpad.right.isPressed ? 1 : 0,
                !pad2.dpad.down.isPressed && !pad2.dpad.up.isPressed ? 0 : pad2.dpad.down.isPressed && !pad2.dpad.up.isPressed ? -1 : !pad2.dpad.down.isPressed && pad2.dpad.up.isPressed ? 1 : 0
                ), 0b0000000000000000, false, true);
            #endregion
            #region Shoulders
            if (pad2.rightShoulder.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.W, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.W, true, false);
            }
            #endregion
            #region Action Pad
            if (pad2.buttonNorth.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.Y, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.Y, true, false);
            }
            if (pad2.buttonSouth.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.A, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.A, true, false);
            }
            if (pad2.buttonEast.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.B, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.B, true, false);
            }
            if (pad2.buttonWest.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.X, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.X, true, false);
            }
            #endregion
        }
        private void ReadInputsFromKeyboardP1() {
            if (keyboard == null)
            {
                return;
            }
            #region Left Stick/Dpad
            ApplyInput(new Vector2
                (
                !keyboard.aKey.isPressed && !keyboard.dKey.isPressed ? 0 : keyboard.aKey.isPressed && !keyboard.dKey.isPressed ? -1 : !keyboard.aKey.isPressed && keyboard.dKey.isPressed ? 1 : 0,
                !keyboard.sKey.isPressed && !keyboard.wKey.isPressed ? 0 : keyboard.sKey.isPressed && !keyboard.wKey.isPressed ? -1 : !keyboard.sKey.isPressed && keyboard.wKey.isPressed ? 1 : 0
                ), 0b0000000000000000, false, true);
            #endregion
            #region Shoulders
            if (keyboard.spaceKey.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.W, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.W, true, false);
            }
            #endregion
            #region Action Pad
            if (keyboard.gKey.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.Y, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.Y, true, false);
            }
            if (keyboard.hKey.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.A, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.A, true, false);
            }
            if (keyboard.jKey.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.B, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.B, true, false);
            }
            if (keyboard.kKey.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.X, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.X, true, false);
            }
            #endregion
        }
        private void ReadInputsFromKeyboardP2()
        {
            if (keyboard == null)
            {
                return;
            }
            #region Left Stick/Dpad
            ApplyInput(new Vector2
                (
                !keyboard.leftArrowKey.isPressed && !keyboard.rightArrowKey.isPressed ? 0 : keyboard.leftArrowKey.isPressed && !keyboard.rightArrowKey.isPressed ? -1 : !keyboard.leftArrowKey.isPressed && keyboard.rightArrowKey.isPressed ? 1 : 0,
                !keyboard.downArrowKey.isPressed && !keyboard.upArrowKey.isPressed ? 0 : keyboard.downArrowKey.isPressed && !keyboard.upArrowKey.isPressed ? -1 : !keyboard.downArrowKey.isPressed && keyboard.upArrowKey.isPressed ? 1 : 0
                ), 0b0000000000000000, false, true);
            #endregion
            #region Shoulders
            if (keyboard.numpad0Key.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.W, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.W, true, false);
            }
            #endregion
            #region Action Pad
            if (keyboard.numpad1Key.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.Y, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.Y, true, false);
            }
            if (keyboard.numpad2Key.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.A, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.A, true, false);
            }
            if (keyboard.numpad3Key.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.B, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.B, true, false);
            }
            if (keyboard.numpad4Key.isPressed)
            {
                ApplyInput(new Vector2(), InputEnum.X, false, false);
            }
            else
            {
                ApplyInput(new Vector2(), InputEnum.X, true, false);
            }
            #endregion
        }
        #endregion

        void OnDestroy()
        {
            if (this._canControl)
            {
                //using the input system to do a little mapping, replace as soon as possible
                //pressed events
                actions["Direction"].performed -= ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, false, true);
                actions["Punch"].performed -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.A, false, false);
                actions["Kick"].performed -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.B, false, false);
                actions["Slash"].performed -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.C, false, false);
                actions["Dust"].performed -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.D, false, false);
                actions["Jump"].performed -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.W, false, false);
                actions["Block"].performed -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.X, false, false);
                //released events
                actions["Direction"].canceled -= ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, true, true);
                actions["Punch"].canceled -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.A, true, false);
                actions["Kick"].canceled -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.B, true, false);
                actions["Slash"].canceled -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.C, true, false);
                actions["Dust"].canceled -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.D, true, false);
                actions["Jump"].canceled -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.W, true, false);
                actions["Block"].canceled -= ctx => ApplyInput(new UnityEngine.Vector2(), InputEnum.X, true, false);
            }
        }
    }
}
public enum inputMod { None, Record, Play }
public enum controllers {pad1, pad2, KeyboardP1, KeyboardP2 }