using System;
using AI.Traits.Shared;
using UnityEngine;

namespace AI.Traits.ObjectProperties
{
    public class NormalEdible : MonoBehaviour, INeedsConsumable, IEntityInteractable
    {
        public float kcal = 160; //fruit = 160; full meal = 700; bread = 265;
        public float minimalUseDistance = 1;
        public string foodName = "devApple";
        public string foodType = "normalFood";

        public string GetConsumableType()
        {
            throw new NotImplementedException();
        }

        public float GetItemRewardValue()
        {
            return (kcal);
        }
        public string GetName()
        {
            return foodType;
        }
        public int GetParentId()
        {
            return gameObject.GetInstanceID();
        }

        public float GetMinimalUseDistance()
        {
            return minimalUseDistance;
        }

        public void ConsumeItem(INeeds need)
        {
            if ((kcal - need.GetNeedExtended()) > 0)
            {
                bool tooFewLeft = ((kcal / 100) * 10) < (kcal - need.GetNeedExtended());
                kcal = kcal - need.GetNeedExtended();
                need.Intake(need.GetNeedExtended());
                if (tooFewLeft)
                {
                    Destroy(gameObject);
                }
                return;
            }
            need.Intake(kcal);
            Destroy(gameObject);
        }
        

        public float GetMinimalDistance()
        {
            return minimalUseDistance;
        }
    }
}