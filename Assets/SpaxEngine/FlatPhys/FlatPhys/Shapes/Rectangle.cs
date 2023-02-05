using UnityEngine;
using System;
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
        public override FlatAABB GetAABB(FVector2 position, FVector2 velocity, Fix64 padding)
        {
            //get box modifier via velocity
            //sign masks
            var xMag = Fix64.Sign(velocity.x) * Fix64.Sign(velocity.x);
            var yMag = Fix64.Sign(velocity.y) * Fix64.Sign(velocity.y);
            //  min value has all bits, so we can use that to check our sign
            //  & this and only negative values will pass
            var xNegMask = (((long)velocity.x) & long.MinValue) >> 63;
            var yNegMask = (((long)velocity.y) & long.MinValue) >> 63;

            //  & this and only positive, nonzero values will pass
            var xPosMask = (xNegMask ^ (long)-1) * xMag;
            var yPosMask = (yNegMask ^ (long)-1) * yMag;



            //if x vel is positive, get that velocity
            var posXmod = new Fix64((int)(xPosMask & (long)velocity.x));
            var posYmod = new Fix64((int)(yPosMask & (long)velocity.y));
            var negXmod = new Fix64((int)(xNegMask & (long)velocity.x));
            var negYmod = new Fix64((int)(yNegMask & (long)velocity.y));



            //get the dimensions
            var wd = this._width / 2;
            var ht = this._height / 2;

            //modify the dimensions by respective positive/negative mask

            //adding some padding to objects to make sure that they don't miss out on objects close by
            var minX = position.x - wd - padding + negXmod;
            var minY = position.y - ht - padding + negYmod;
            //var minY = position.y;
            var maxX = position.x + wd + padding + posXmod;
            var maxY = position.y + ht + padding + posYmod;
            //var maxY = position.y + (ht * 2);

            var ret = new FlatAABB(minX, minY, maxX, maxY);

            /*
                         if (Width == 1)
                         {
                             Debug.Log("vel = " + velocity.x + ", " + velocity.y);
                             Debug.Log("negxmask " + negXmod);
                             Debug.Log("posxmask " + posXmod);
                             Debug.Log("negxmask " + negYmod);
                             Debug.Log("posxmask " + posYmod);
                             Debug.Log("max = " + maxX + ", " + maxY);
                             Debug.Log("min = " + minX + ", " + minY);
                             Debug.Log("max = " + ret.Max.x + ", " + ret.Max.y);
                             Debug.Log("min = " + ret.Min.x + ", " + ret.Min.y);
                         }
            */
            return ret;
        }
    }
}