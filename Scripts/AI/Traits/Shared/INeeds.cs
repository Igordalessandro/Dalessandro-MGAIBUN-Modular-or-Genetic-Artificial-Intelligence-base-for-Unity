using System;

namespace AI.Traits.Shared
{
    public interface INeeds : IActionAble 
    {
        void Tick();
        float GetNeedExtended();
        float GetNeed();
        //passive = expected reward.
        float GetExpectedPleasantness();
        //active = actual reward or need.
        float GetActivePleasantness();
        String GetNeedName();
        
        String GetState();
        void Intake(float var);

        float GetFullState();
    }
}