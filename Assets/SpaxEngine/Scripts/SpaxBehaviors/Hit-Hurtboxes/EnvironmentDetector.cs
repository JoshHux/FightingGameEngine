using FixMath.NET;
using FlatPhysics.Unity;
using FlatPhysics.Contact;
using FightingGameEngine.Data;
using Spax;

namespace FightingGameEngine.Gameplay
{
    public class EnvironmentDetector : SpaxBehavior
    {
        private LivingObject _owner;
        private FBox _trigger;
        protected override void OnStart()
        {
            base.OnStart();

            this._trigger = this.GetComponent<FBox>();
            this._owner = this.transform.parent.GetComponentInParent<LivingObject>();
            this._trigger.Body.OnOverlap += (ctx) => this.OnFlatOverlap(ctx);
        }

        void OnDestroy()
        {
            this._trigger.Body.OnOverlap -= (ctx) => this.OnFlatOverlap(ctx);

        }



        //delegate to add the object this object is overlapping with
        //if we are not overlapping with an object, nothing is added to curColliding
        private void OnFlatOverlap(ContactData c)
        {
            //UnityEngine.Debug.Log("walled");
            this._owner.SetWalled(1);
        }


    }
}