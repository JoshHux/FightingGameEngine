using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using FlatPhysics.Unity;
using FightingGameEngine.Enum;
using FightingGameEngine.Data;
namespace FightingGameEngine.Gameplay
{
    public class FightingCharacterController : ControllableObject
    {
        [SerializeField] private LivingObject _other;
        public LivingObject Other { set { this._other = value; } }
        public int PlayerID { get { return this.status.PlayerID; } }


        protected override void InputUpdate()
        {
            base.InputUpdate();

            if (this.status.CurrentControllerState.X() * this.status.WalledDirection < 0) { this.status.TransitionFlags |= TransitionFlags.WALL_JUMPING; }
        }

        protected override void StateCleanUpdate() { }
        protected override void PreUpdate() { }

        protected override void SpaxUpdate()
        {
            base.SpaxUpdate();
            this.status.TransitionFlags &= (TransitionFlags)(~((uint)1 << 8));
            this.status.WalledDirection = 0;

        }


        //call to process transition event enums
        protected override void ProcessTransitionEvent(in TransitionEvents te, in ResourceData rd)
        {
            base.ProcessTransitionEvent(te, rd);
            /*----- PROCESSING AUTOMATIC TURNING -----*/

            //can we rotate?
            bool canRotate = EnumHelper.HasEnum((uint)te, (uint)TransitionEvents.FACE_OPPONENT);

            //TODO: When transporting this to 3d, replace this calculation with a 3d math

            //what is the difference between our x position and their x position?
            var diffPos = this._other.get_position().x - this.status.CurrentPosition.x;

            //if our facing direction and the difference in position are different, then we should turn
            bool shouldTurn = canRotate && ((diffPos * this.status.CurrentFacingDirection) < 0) && !EnumHelper.HasEnum((uint)this.status.TransitionFlags, (uint)TransitionFlags.WALLED) && this.IsAirborne() == 0;

            if (shouldTurn)
            {
                var newFacing = Fix64.Sign(diffPos);
                this.status.CurrentFacingDirection = newFacing;
            }

        }

        //call to process the state conditions of our current state
        protected override void ProcessStateConditions(in StateConditions stateConditions)
        {
            /*----- PROCESSING AUTOMATIC TURNING -----*/

            //can we rotate?
            bool canRotate = EnumHelper.HasEnum((uint)stateConditions, (uint)StateConditions.AUTO_TURN);

            //TODO: When transporting this to 3d, replace this calculation with a 3d math

            //what is the difference between our x position and their x position?
            var diffPos = this._other.get_position().x - this.status.CurrentPosition.x;

            //if our facing direction and the difference in position are different, then we should turn
            bool shouldTurn = canRotate && ((diffPos * this.status.CurrentFacingDirection) <= 0) && !EnumHelper.HasEnum((uint)this.status.TransitionFlags, (uint)TransitionFlags.WALLED) && this.IsAirborne() == 0;

            if (shouldTurn)
            {
                var newFacing = Fix64.Sign(diffPos);
                this.status.CurrentFacingDirection = newFacing;
            }


            base.ProcessStateConditions(stateConditions);

            /*----- PROCESSING IF PLAYER MOVES -----*/
            bool canMove = EnumHelper.HasEnum((uint)stateConditions, (uint)StateConditions.CAN_MOVE);
            bool canRun = EnumHelper.HasEnum((uint)stateConditions, (uint)StateConditions.CAN_RUN, true);

            //current controller state
            var curConStt = this.status.CurrentControllerState;
            //x-axis input
            var xInput = curConStt.X();
            //max movement speed
            var maxMoveSpeed = this.data.WalkMaxSpd;
            if (canRun) { maxMoveSpeed = this.data.RunMaxSpd; }

            var acceleration = this.data.WalkAccel;
            if (canRun) { acceleration = this.data.RunAccel; }

            //current x speed
            var curXvel = this.status.TotalVelocity.x;
            //difference between current x-velocity and max velocity in the direction we want to move
            var velDiff = (maxMoveSpeed * xInput) - curXvel;

            //can we move?
            //so we want to move?
            bool wantToMove = canMove && (xInput != 0);
            //can we even apply any acceleration? 
            //  if we're not moving as fast as max speed OR they're pointing in the opposite directions
            bool shouldMove = wantToMove && (velDiff != 0) && ((Fix64.FastAbs(curXvel) < maxMoveSpeed) || ((curXvel * maxMoveSpeed * xInput) <= 0));

            if (shouldMove)
            {
                //we only reach here if we need to apply acceleration
                //acceleration is the min between the abs of the difference between max speed and current x speed AND applies movement velocity
                var absDiff = Fix64.FastAbs(velDiff);
                var appliedAccel = Fix64.Min(acceleration, absDiff) * xInput;
                //Debug.Log("x input - " + xInput + " | appliedAccel " + appliedAccel + " | absDiff - " + absDiff + " | mms*x - " + (maxMoveSpeed * xInput) + " | cur xvel " + (curXvel));

                //we multiply by -1 so that we negate the facing direction, we're not factoring in facing direction in this application
                this.status.CalcVelocity += new FVector2(appliedAccel, 0) * this.status.CurrentFacingDirection;
                //Debug.Log("calc - " + this.status.CalcVelocity.x);
            }



            /*----- PROCESSING ACTIVE PUSHBOX -----*/
            this.Body.ActivePushbox = !EnumHelper.HasEnum((uint)stateConditions, (uint)StateConditions.INACTIVE_PUSHBOX);
        }

        //fires when state is changed
        protected override void OnStateSet()
        {
            base.OnStateSet();
        }


        public LivingObject get_other()
        {
            return this._other;
        }
    }
}