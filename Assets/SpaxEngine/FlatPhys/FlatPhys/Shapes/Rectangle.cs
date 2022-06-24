using UnityEngine;
using FixMath.NET;
using FlatPhysics;
namespace FlatPhysics.Shapes
{

    public class Rectangle : Shape
    {
        private Fix64 _width;
        private Fix64 _height;

        public Fix64 Width
        {
            get { return this._width; }
            set { this._width = value; }
        }
        public Fix64 Height
        {
            get { return this._height; }
            set { this._height = value; }
        }
        public Rectangle(Fix64 wd, Fix64 ht)
        {
            this._width = wd;
            this._height = ht;
        }
        public override FlatAABB GetAABB(FVector2 position)
        {
            var wd = this._width / 2;
            var ht = this._height / 2;

            var minX = position.x - wd;
            var minY = position.y - ht;
            var maxX = position.x + wd;
            var maxY = position.y + ht;
            var ret = new FlatAABB(minX, minY, maxX, maxY);
            return ret;
        }
    }
}