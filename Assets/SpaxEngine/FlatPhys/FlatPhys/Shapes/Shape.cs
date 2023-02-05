using UnityEngine;
using FixMath.NET;
using FlatPhysics;
namespace FlatPhysics.Shapes
{

    public abstract class Shape
    {
        public abstract FlatAABB GetAABB(FVector2 position, FVector2 velocity, Fix64 padding);
    }
}