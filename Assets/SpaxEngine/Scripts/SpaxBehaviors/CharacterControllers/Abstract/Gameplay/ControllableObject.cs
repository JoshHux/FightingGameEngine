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
        protected override void OnAwake()
        {
            base.OnAwake();

            //init the status object, only really for the input recorder to get ready
            this.status.Initialize();

            //using the input system to do a little mapping, replace as soon as possible
            //pressed events
            actions["Direction"].performed += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, false);
            actions["Punch"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000001000000, false);
            actions["Kick"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000010000000, false);
            actions["Slash"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000100000000, false);
            actions["Dust"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000001000000000, false);
            actions["Jump"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000010000000000, false);
            actions["Block"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000100000000000, false);
            //released events
            actions["Direction"].canceled += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, true);
            actions["Punch"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000001000000, true);
            actions["Kick"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000010000000, true);
            actions["Slash"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000100000000, true);
            actions["Dust"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000001000000000, true);
            actions["Jump"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000010000000000, true);
            actions["Block"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000100000000000, true);

            actions["Direction"].Enable();
            actions["Punch"].Enable();
            actions["Kick"].Enable();
            actions["Slash"].Enable();
            actions["Dust"].Enable();
            actions["Jump"].Enable();
            actions["Block"].Enable();

            actions.Enable();
        }

        protected override void InputUpdate()
        {
            //Debug.Log("input updating");
            this.BufferInput();
        }

        private void ApplyInput(UnityEngine.Vector2 dir, short input, bool released)
        {
            //check the direction
            //"current" controller state, for easy reference
            var curInput = this.status.CurrentControllerState;
            //getting the buttons only for later refrence
            var curInputBtn = curInput.Input & InputEnum.BUTTONS;


            //get the x-direction from dir, X_ZERO is the default so we only need to check negative or positive values
            var xDir = InputEnum.X_ZERO;
            if (dir.x > 0) { xDir = InputEnum.X_POSITIVE; }
            else if (dir.x < 0) { xDir = InputEnum.X_NEGATIVE; }

            //get the y-direction from dir, Y_ZERO is the default so we only need to check negative or positive values
            var yDir = InputEnum.Y_ZERO;
            if (dir.y > 0) { yDir = InputEnum.Y_POSITIVE; }
            else if (dir.y < 0) { yDir = InputEnum.Y_NEGATIVE; }


            //AND the xDir and yDir so that we get the final direction to be assigned to the CurrentControllerState in the InputRecorder object
            var finalDir = xDir & yDir;

            //final new InputEnum
            var finalInput = finalDir | 0;

            //new InputItem we assign as the CurrentControllerState
            var newCurState = new InputItem(finalInput);

            //assign new CurrentControllerState
            this.status.CurrentControllerState = newCurState;

        }

        private void BufferInput()
        {
            //whether to add extra buffer leniency to the input to buffer aerials curing prejump
            bool bufferLeniency = false;
            //adds a new input, check if state can transition
            //buffers the current controller state, saved in the recorder itself
            this.status.BufferInput(bufferLeniency);
        }
    }
}