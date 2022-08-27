using FightingGameEngine.Data;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Gameplay
{
    public class HurtboxTrigger : BoxTrigger
    {

        private HurtboxData m_data;

        private void ActivateBox(HurtboxData newData)
        {
            this.m_data = newData;

            var newPos = this.m_data.Position;
            var newDim = this.m_data.Dimensions;

            this.CommonActivateBox(newPos, newDim);
        }


        protected override void CheckDataFromFrame(object sender, FrameData data)
        {
            //get the data for quick 
            if (data == null) { this.DeactivateBox(); }
            var boxdata = data.GetHurtbox(this.triggerIndex);
            //valid or invalid boxdata, checks if dimensions are valid
            bool isValid = boxdata.Dimensions.sqrMagnitude > 0;

            //if invalid data, don't
            if (!isValid)
            {
                return;
            }
            this.ActivateBox(boxdata);

        }

        public override void DeactivateBox()
        {
            this.CommonDeactivateBox();
        }

        //called by hitboxes interacting with this to hit the owner
        public HitIndicator HurtThisBox(HitInfo boxData)
        {
            var ret = this.Owner.AddHitboxToQuery(boxData);
            return ret;
        }
    }
}