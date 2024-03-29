﻿using System;
using UnityEngine;
using FixMath.NET;
using FlatPhysics.Shapes;
using FlatPhysics.Filter;
using FlatPhysics.Contact;
using FlatPhysics.Unity;
using FightingGameEngine.Gameplay;
namespace FlatPhysics
{
    public enum ShapeType
    {
        Circle = 0,
        Box = 1
    }

    public delegate void OverlapEventHandler(in ContactData c);
    public sealed class FlatBody
    {
        private bool _awake;
        private bool _concave;

        private int _bodyID;
        private FVector2 _position;
        private FVector2 _localPosition;
        private FVector2 _linearVelocity;
        //velocity applied to rexolve collisions, set to 0 after use
        public FVector2 Impulse;
        private Fix64 _rotation;
        private Fix64 _rotationalVelocity;

        private FVector2 force;
        private FlatBody _parent;

        public readonly Fix64 Mass;
        public Fix64 InvMass
        {
            get
            {
                if (this.IsStatic || (false && this.livingObject != null && this.livingObject.IsWalled() > 0)) { return 0; }
                return 1 / this.Mass;
            }
        }
        public readonly Fix64 Restitution;

        public readonly bool IsStatic;
        public readonly bool IsTrigger;

        public Fix64 Radius
        {
            get { return (this._shape as Circle).Radius; }
            set
            {
                (this._shape as Circle).Radius = value;
                //this.transformUpdateRequired = true;
                //this.aabbUpdateRequired = true;
            }
        }

        public Fix64 Width
        {
            get { return (this._shape as Rectangle).Width; }
            set
            {
                //automatically preps the AABB dimensions
                (this._shape as Rectangle).Width = value;
                this.CreateRectVertices();
            }
        }
        public Fix64 Height
        {
            get { return (this._shape as Rectangle).Height; }
            set
            {
                //automatically preps the AABB dimensions
                (this._shape as Rectangle).Height = value;
                this.CreateRectVertices();
            }
        }

        private FVector2[] vertices;
        public readonly int[] Triangles;
        private FVector2[] transformedVertices;
        private FlatAABB aabb;

        //private bool transformUpdateRequired;
        //private bool aabbUpdateRequired;

        public readonly ShapeType ShapeType;

        //what layer this body is on
        public readonly CollisionLayer Layer;
        //what layer this body can collide with
        public readonly CollisionLayer CollidesWith;
        //gameobject this body corresponds to
        public readonly FRigidbody GameObject;
        public readonly LivingObject livingObject;


        public readonly bool IsPushbox;

        private Shape _shape;

        private OverlapEventHandler _onOverlap;

        public bool Awake
        {
            get { return this._awake; }
            set { this._awake = value; }
        }

        public int BodyID
        {
            get { return this._bodyID; }
            set { this._bodyID = value; }
        }


        public FVector2 Position
        {
            get
            {
                if (this._parent != null) { return this.LocalPosition + this._parent.Position; }
                return this._position;
            }
            set
            {
                if (this._parent != null) { this.LocalPosition = value - this._parent.Position; }
                else
                {
                    this._position = value;
                }
            }
        }

        public FlatBody Parent
        {
            get { return this._parent; }
        }

        public FVector2 LocalPosition
        {
            get { return this._localPosition; }
            set { this._localPosition = value; }
        }

        public Shape Shape
        {
            get { return this.Shape; }
        }

        public FVector2 LinearVelocity
        {
            get { return this._linearVelocity; }
            internal set
            {
                if (this.IsStatic) { return; }
                this._linearVelocity = value;
            }
        }

        public event OverlapEventHandler OnOverlap
        {
            add
            {
                this._onOverlap += value;
            }
            remove
            {
                this._onOverlap -= value;
            }
        }


        public bool Concave
        {
            get { return this._concave; }
            set
            {
                this._concave = value;
            }

        }

        public bool ActivePushbox;

        private FlatBody(FVector2 position, Fix64 mass, Fix64 restitution,
            bool isStatic, bool isPushbox, bool isTrigger, Fix64 radius, Fix64 width, Fix64 height,
            ShapeType shapeType, CollisionLayer layer, CollisionLayer collidesWith, FRigidbody gameObject)
        {
            this._position = position;
            this._localPosition = FVector2.zero;
            this._linearVelocity = FVector2.zero;
            this.Impulse = FVector2.zero;

            this._rotation = 0;
            this._rotationalVelocity = 0;

            this.force = FVector2.zero;

            this.Mass = mass;
            this.Restitution = restitution;

            this.IsStatic = isStatic;
            this.IsTrigger = isTrigger;
            //this.Radius = radius;
            //this.Width = width;
            //this.Height = height;
            this.ShapeType = shapeType;
            this.Layer = layer;
            this.CollidesWith = collidesWith;
            this.GameObject = gameObject;
            this.IsPushbox = isPushbox;
            this.ActivePushbox = this.IsPushbox;
            //have it be null to start out with, guarentees null if no parent
            this._parent = null;


            if (this.IsPushbox) { this.livingObject = this.GameObject.GetComponent<LivingObject>(); }

            if (this.ShapeType is ShapeType.Box)
            {
                this._shape = new Rectangle(width, height);
                //this.vertices = this.CreateRectVertices();
                this.CreateRectVertices();
                this.Triangles = FlatBody.CreateBoxTriangles();
                this.transformedVertices = new FVector2[this.vertices.Length];

            }
            else
            {
                this._shape = new Circle(radius);
                this.vertices = null;
                Triangles = null;
                this.transformedVertices = null;

            }

            //this.transformUpdateRequired = true;
            //this.aabbUpdateRequired = true;
        }

        public void OnColOverlap(ContactData c)
        {
            this._onOverlap?.Invoke(c);
        }

        public void SetParent(FlatBody newPar)
        {
            this._parent = newPar;
        }

        private void CreateRectVertices()
        {
            //this.transformUpdateRequired = true;
            //this.aabbUpdateRequired = true;
            this.vertices = FlatBody.CreateBoxVertices(this.Width, this.Height);
        }

        private static FVector2[] CreateBoxVertices(Fix64 width, Fix64 height)
        {
            Fix64 left = -width / 2;
            Fix64 right = left + width;
            Fix64 bottom = -height / 2;
            Fix64 top = bottom + height;

            FVector2[] vertices = new FVector2[4];
            vertices[0] = new FVector2(left, top);
            vertices[1] = new FVector2(right, top);
            vertices[2] = new FVector2(right, bottom);
            vertices[3] = new FVector2(left, bottom);

            return vertices;
        }

        private static int[] CreateBoxTriangles()
        {
            int[] triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
            return triangles;
        }

        public FVector2[] GetTransformedVertices()
        {
            //if (this.transformUpdateRequired)
            //{
            FlatTransform transform = new FlatTransform(this.Position, this._rotation);

            for (int i = 0; i < this.vertices.Length; i++)
            {
                FVector2 v = this.vertices[i];
                this.transformedVertices[i] = FVector2.Transform(v, transform);
            }
            //}

            //this.transformUpdateRequired = false;
            return this.transformedVertices;
        }

        public FlatAABB GetAABB()
        {
            var offset = new FVector2();
            if (this.Parent != null) { offset = this._parent._linearVelocity; }
            this.aabb = this._shape.GetAABB(this.Position, (this._linearVelocity + offset) * ((Fix64)1 / (Fix64)60), 1);
            //}


            //this.aabbUpdateRequired = false;
            return this.aabb;
        }

        public FlatAABB GetBox()
        {
            return this._shape.GetAABB(this.Position, FVector2.zero, 0);
        }

        internal void Step(Fix64 time, FVector2 gravity, int iterations)
        {
            if (this.IsStatic)
            {
                return;
            }

            time /= (Fix64)iterations;

            // force = mass * acc
            // acc = force / mass;

            //FVector2 acceleration = this.force / this.Mass;
            //this._linearVelocity += acceleration * time;
            if (this.HasParent()) { return; }

            //this._linearVelocity += gravity * time;
            //this._position += this._linearVelocity * time;
            this._position += this._linearVelocity * time;
            //this.Impulse = new FVector2(0, 0);
            //this._rotation += this._rotationalVelocity * time;

            this.force = FVector2.zero;
            //this.transformUpdateRequired = true;
            //this.aabbUpdateRequired = true;

            //if we have a parent, step based on parent
            //if (this.HasParent()) { this.StepParent(); }

        }

        public bool HasParent() { return this._parent != null; }

        public void StepParent()
        {
            this._position = this._parent.Position + this._localPosition;
        }

        public void Move(FVector2 amount)
        {
            if (this.IsStatic || this.IsTrigger) { return; }
            this.Position += amount;
            //this.transformUpdateRequired = true;
            //this.aabbUpdateRequired = true;
        }

        public void MoveTo(FVector2 position)
        {
            this.Position = position;
            //this.transformUpdateRequired = true;
            //this.aabbUpdateRequired = true;
        }

        public void Rotate(Fix64 amount)
        {
            this._rotation += amount;
            //this.transformUpdateRequired = true;
            //this.aabbUpdateRequired = true;
        }

        public void AddForce(FVector2 amount)
        {
            this.force = amount;
        }

        public static bool CreateCircleBody(Fix64 radius, Fix64 mass, FVector2 position, bool isStatic, bool isPushbox, bool isTrigger, Fix64 restitution, CollisionLayer layer, CollisionLayer collidesWith, FRigidbody gameObject, out FlatBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;


            restitution = Fix64.Clamp(restitution, 0, 1);

            // mass = area * depth * density

            body = new FlatBody(position, mass, restitution, isStatic, isPushbox, isTrigger, radius, 0, 0, ShapeType.Circle, layer, collidesWith, gameObject);
            return true;
        }

        public static bool CreateBoxBody(Fix64 width, Fix64 height, Fix64 mass, FVector2 position, bool isStatic, bool isPushbox, bool isTrigger, Fix64 restitution, CollisionLayer layer, CollisionLayer collidesWith, FRigidbody gameObject, out FlatBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;

            restitution = Fix64.Clamp(restitution, 0, 1);

            // mass = area * depth * density

            body = new FlatBody(position, mass, restitution, isStatic, isPushbox, isTrigger, 0, width, height, ShapeType.Box, layer, collidesWith, gameObject);
            return true;
        }
    }
}
