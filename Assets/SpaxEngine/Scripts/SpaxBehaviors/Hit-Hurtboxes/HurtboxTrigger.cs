using FightingGameEngine.Data;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Gameplay
{
    public class HurtboxTrigger : BoxTrigger
    {

        private HurtboxData m_data;

        protected override void OnStart()
        {
            base.OnStart();

            //add our own activator event when we reached a frame and may activate this box trigger
            this.Owner.OnHurtFrameReached += CheckDataFromFrame;
        }



        void OnDestroy()
        {
            this.Owner.OnHurtFrameReached -= CheckDataFromFrame;
        }

        private void ActivateBox(in HurtboxData newData)
        {
            this.m_data = newData;

            var newPos = this.m_data.Position;
            var newDim = this.m_data.Dimensions;

            this.CommonActivateBox(newPos, newDim);

            if (newData.Dimensions.x == 0 || newData.Dimensions.y == 0)
            {
                this.CommonDeactivateBox();
            }
        }


        private void CheckDataFromFrame(object sender, in HurtboxHolder data)
        {
            //if (data == null) { this.DeactivateBox(); return; }
            var boxData = data.GetHurtbox(this.triggerIndex);
            this.SetData(boxData);
        }

        protected override void ApplyGameState(object sender, in GameplayState state)
        {
            this.SetData(state.HurtboxStates.GetValue(this.triggerIndex));
        }


        public override void DeactivateBox(object sender)
        {
            this.CommonDeactivateBox();
        }

        private void SetData(in HurtboxData boxData)
        {
            //valid or invalid boxdata, checks if dimensions are valid
            bool isValid = boxData.Dimensions.sqrMagnitude > 0;

            //if invalid data, don't
            if (!isValid)
            {
                return;
            }
            this.ActivateBox(boxData);
        }

        //called by hitboxes interacting with this to hit the owner
        public HitIndicator HurtThisBox(in HitInfo boxData)
        {
            var ret = this.Owner.AddHitboxToQuery(boxData);
            return ret;
        }

        public HurtboxData GetHurtboxData() { return this.m_data; }


    }
}