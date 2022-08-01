using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FightingGameEngine.Enum;
using FightingGameEngine.Data;

namespace FightingGameEngine.Gameplay
{
    public abstract class ControllableObject : CombatObject
    {
        //Unity's input mapping, replace when given the opportunity
        [SerializeField] private InputActionAsset actions;
        [SerializeField] private bool _canControl = true;
        protected override void OnAwake()
        {
            base.OnAwake();

            //init the status object, only really for the input recorder to get ready
            this.status.Initialize();

            if (this._canControl)
            {
                //using the input system to do a little mapping, replace as soon as possible
                //pressed events
                actions["Direction"].performed += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, false, true);
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

                actions.Enable();
            }
        }

        protected override void InputUpdate()
        {
            //Debug.Log("input updating");
            this.BufferInput();
        }

        private void ApplyInput(UnityEngine.Vector2 dir, InputEnum input, bool released, bool direction)
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

        private void BufferInput()
        {
            //whether to add extra buffer leniency to the input to buffer aerials curing prejump
            bool bufferLeniency = EnumHelper.HasEnum((uint)this.status.TotalStateConditions, (uint)StateConditions.BUFFER_INPUT);
            //adds a new input, check if state can transition
            //buffers the current controller state, saved in the recorder itself
            this.status.BufferInput(bufferLeniency);
        }

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