using UnityEngine;

namespace AI.Traits.Shared
{
    public class DetectedObject
    {
        public GameObject actualGameObject;
        public float Distance;
        public int Quantity;
        public int Index;

            public DetectedObject( int cQuantity,float cDistance,GameObject cObj, int cIndex)
            {
                Distance = cDistance;
                actualGameObject = cObj;
                Quantity = cQuantity;
                Index = cIndex;
            }
        }
}
