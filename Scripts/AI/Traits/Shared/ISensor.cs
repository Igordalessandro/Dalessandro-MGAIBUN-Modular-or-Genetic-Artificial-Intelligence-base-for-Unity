using System.Collections.Generic;

namespace AI.Traits.Shared
{
    public interface ISensor 
    {
        bool DatabaseIsOnline();

        void SetControlListOfIndexesAndPackedObjects(
            SortedList<int, DetectedObject> controlListOfIndexesAndPackedObjects);
        void SetAmountOfSensorsPickingUpEachObject(SortedList<int, int> amountOfSensorsPickingUpEachObject);
        void SetIndexesByDistance(SortedList<float, int> indexesByDistance);
        void SetMissingIndexesControlList(List<int> missingIndexesControlList);
    }
}
