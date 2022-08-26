using UnityEngine;
using FixMath.NET;
using FlatPhysics.Filter;
using Spax;
namespace FlatPhysics.Unity
{
    public abstract class FRigidbody : MonoBehaviour
    {
        protected FlatBody _rb;

        protected enum col { Red, Green, Yellow, Cyan, Blue }
        [SerializeField] protected col boxColor = col.Blue;
        [SerializeField] protected bool isStatic;
        [SerializeField] protected bool isTrigger;
        [SerializeField] protected bool isPushbox;
        [SerializeField] protected Fix64 mass;
        [SerializeField, ReadOnly] private FVector2 _position;
        [SerializeField, ReadOnly] private FVector2 _velocity;
        [SerializeField, ReadOnly] private CollisionLayer _thisLayer;
        [SerializeField, ReadOnly] private CollisionLayer _collidesWith;

        //For Draw Boxes
        public GameObject drawedBox;
        private GameObject tempDrawedBox;

        public FlatBody Body
        {
            get
            {
                if (this._rb == null)
                {
                    this.StartPhys();
                }
                return this._rb;
            }
        }

        public FVector2 Velocity
        {
            get { return this._rb.LinearVelocity; }
            set { this._rb.LinearVelocity = value; }
        }
        public FVector2 Position
        {
            get { return this._rb.Position + new FVector2(0, -this._rb.Height / 2); }
            set { this._rb.Position = value + new FVector2(0, this._rb.Height / 2); }
        }

        public FVector2 LocalPosition
        {
            get { return this._rb.LocalPosition; }
            set { this._rb.LocalPosition = value; }
        }

        public bool Awake
        {
            get { return this._rb.Awake; }
            set { this._rb.Awake = value; }
        }

        void Start()
        {
            if (this._rb == null)
            {
                this.StartPhys();
            }
        }

        private void StartPhys()
        {
            //CollisionLayer layer = FlatWorldMono.instance.GetCollisions(this.gameObject.layer);
            CollisionLayer layer = SpaxManager.Instance.GetCollisions(this.gameObject.layer);
            //Debug.Log(this.gameObject.layer);
            this.InstantiateBody(layer);

            this.ResolveStart();
        }

        private void ResolveStart()
        {
            FRigidbody hold = null;
            Transform possibleParent = this.transform.parent;

            //search up to two levels up for a possible rigidbody parent
            if (possibleParent != null)
            {
                possibleParent = possibleParent.parent;
                if (possibleParent != null)
                {
                    possibleParent.TryGetComponent<FRigidbody>(out hold);
                }
            }

            //if we could find a parent, set the parent
            if (hold != null)
            {
                var toSet = hold.Body;
                this._rb.SetParent(toSet);
                var offset = this._rb.Position - hold.Body.Position;
                this._rb.LocalPosition = offset;
            }

            this._rb.Awake = true;

            SpaxManager.Instance.AddBody(this);

            this._thisLayer = this.Body.Layer;
            this._collidesWith = this.Body.CollidesWith;
        }

        private void DrawBoxesInRunTime(Vector3 pos, Vector3 dim)
        {
            if (tempDrawedBox == null)
            {
                tempDrawedBox = Instantiate(drawedBox, GameObject.FindGameObjectWithTag("DrawBoxContainer").transform);
                tempDrawedBox.name =this.gameObject.transform.root.name + "/" + this.gameObject.name + "-DrawBox";
            }
            else
            {
                tempDrawedBox.transform.position = pos;
                tempDrawedBox.transform.rotation = transform.rotation;

                tempDrawedBox.transform.GetChild(0).localPosition = new Vector3(0, 0, -0.5f);
                tempDrawedBox.transform.GetChild(1).localPosition = new Vector3(0, 0, 0.5f);
                tempDrawedBox.transform.GetChild(2).localPosition = new Vector3(-dim.x / 2, 0, 0);
                tempDrawedBox.transform.GetChild(3).localPosition = new Vector3(dim.x / 2, 0, 0);
                tempDrawedBox.transform.GetChild(4).localPosition = new Vector3(0, dim.y / 2, 0);
                tempDrawedBox.transform.GetChild(5).localPosition = new Vector3(0, -dim.y / 2, 0);

                tempDrawedBox.transform.GetChild(0).GetComponent<SpriteRenderer>().size = new Vector2(dim.x, dim.y);
                tempDrawedBox.transform.GetChild(1).GetComponent<SpriteRenderer>().size = new Vector2(dim.x, dim.y);
                tempDrawedBox.transform.GetChild(2).GetComponent<SpriteRenderer>().size = new Vector2(1, dim.y);
                tempDrawedBox.transform.GetChild(3).GetComponent<SpriteRenderer>().size = new Vector2(1, dim.y);
                tempDrawedBox.transform.GetChild(4).GetComponent<SpriteRenderer>().size = new Vector2(dim.x, 1);
                tempDrawedBox.transform.GetChild(5).GetComponent<SpriteRenderer>().size = new Vector2(dim.x, 1);

                for (int i = 0; i < tempDrawedBox.transform.childCount; i++)
                {
                    switch (boxColor)
                    {
                        case col.Red:
                            tempDrawedBox.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.red;
                            break;
                        case col.Green:
                            tempDrawedBox.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.green;
                            break;
                        case col.Yellow:
                            tempDrawedBox.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.yellow;
                            break;
                        case col.Cyan:
                            tempDrawedBox.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.cyan;
                            break;
                        case col.Blue:
                            tempDrawedBox.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.blue;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        void Update()
        {
            if (this._rb != null)
            {
                this.transform.position = new Vector3((float)this._rb.Position.x, (float)this._rb.Position.y, 0f);
                this._velocity = this.Body.LinearVelocity;
                this._position = this.Body.Position;

                DrawBoxesInRunTime(new Vector3((float)this._rb.Position.x, (float)this._rb.Position.y, 0f), new Vector3((float)this._rb.Width, (float)this._rb.Height, 1f));
            }
        }

        void OnDestroy()
        {
            Spax.SpaxManager.Instance.RemoveBody(this);
        }

        protected abstract void InstantiateBody(CollisionLayer collidesWith);
        public virtual void SetDimensions(FVector2 newDim) { }
    }
}