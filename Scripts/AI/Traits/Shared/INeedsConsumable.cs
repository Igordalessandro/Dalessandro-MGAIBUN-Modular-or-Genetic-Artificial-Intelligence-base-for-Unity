namespace AI.Traits.Shared
{
    public interface INeedsConsumable
    {
        void ConsumeItem(INeeds need);

        string GetConsumableType();

        float GetItemRewardValue();

        string GetName();

        int GetParentId();

        float GetMinimalUseDistance();

    }
}