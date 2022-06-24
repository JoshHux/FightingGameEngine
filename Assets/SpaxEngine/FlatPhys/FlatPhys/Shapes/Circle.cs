using UnityEngine;
using FixMath.NET;
using FlatPhysics;
namespace FlatPhysics.Shapes
{

    public class Circle : Shape
    {
        private Fix64 _radius;

        public Fix64 Radius
        {
            get { return this._radius; }
            set { this._radius = value; }
        }
        public Circle(Fix64 rad)
        {
            this._radius = rad;
        }
        public override FlatAABB GetAABB(FVector2 position)
        {

            var minX = position.x - this._radius;
            var minY = position.y - this._radius;
            var maxX = position.x + this._radius;
            var maxY = position.y + this._radius;
            var ret = new FlatAABB(minX, minY, maxX, maxY);
            return ret;
        }
    }
}