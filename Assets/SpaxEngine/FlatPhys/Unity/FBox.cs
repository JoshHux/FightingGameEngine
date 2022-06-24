using UnityEngine;
using FixMath.NET;
using FlatPhysics.Filter;
using Spax;
namespace FlatPhysics.Unity
{
    public class FBox : FRigidbody
    {
        [SerializeField] private Fix64 _width;
        [SerializeField] private Fix64 _height;

        public Fix64 Width
        {
            get { return this._rb.Width; }
            set
            {
                this._rb.Width = value;
            }
        }

        public Fix64 Height
        {
            get { return this._rb.Height; }
            set
            {
                this._rb.Height = value;
            }
        }

        public override void SetDimensions(FVector2 newDim)
        {
            this._rb.Width = newDim.x;
            this._rb.Height = newDim.y;
        }

        protected override void InstantiateBody(CollisionLayer collidesWith)
        {
            string thing = "";
            CollisionLayer layer = (CollisionLayer)(1 << this.gameObject.layer);
            //Debug.Log(layer);
            FVector2 pos = new FVector2((Fix64)this.transform.position.x, (Fix64)this.transform.position.y);
            Fix64 rot = (Fix64)this.transform.rotation.eulerAngles.z * FixedMath.Deg2Rad;
            FlatBody.CreateBoxBody(this._width, this._height, this.mass, pos, this.isStatic, this.isPushbox, this.isTrigger, rot, layer, collidesWith, this, out this._rb, out thing);
            //FlatWorldMono.instance.AddBody(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (Application.isPlaying && (this._rb != null))
            {
                Vector3 pos = new Vector3((float)this._rb.Position.x, (float)this._rb.Position.y, 0f);
                Vector3 dim = new Vector3((float)this._rb.Width, (float)this._rb.Height, 1f);

                Gizmos.matrix = Matrix4x4.TRS(pos, transform.rotation, Vector3.one);

                //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
                Gizmos.DrawWireCube(Vector2.zero, dim);
            }
            else
            {
                Vector3 dim = new Vector3((float)this._width, (float)this._height, 1f);

                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
                Gizmos.DrawWireCube(Vector2.zero, dim);
            }
        }
    }
}
