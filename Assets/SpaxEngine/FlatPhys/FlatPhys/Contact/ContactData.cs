using UnityEngine;
using FixMath.NET;
using FlatPhysics.Unity;
namespace FlatPhysics.Contact
{
    public class ContactData
    {
        public FVector2 contactPos;
        public FRigidbody other;

        public ContactData(FVector2 cp, FRigidbody go)
        {
            contactPos = cp;
            other = go;
        }
    }
}