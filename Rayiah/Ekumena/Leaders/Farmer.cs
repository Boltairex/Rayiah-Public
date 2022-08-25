using System;
using System.Collections.Generic;

namespace Rayiah.Ekumena.Leaders
{
    public class Farmer : PlayerCharacterClasses
    {
        public override PlayerCharacterType Type => PlayerCharacterType.Farmer;

        public override Dictionary<ResourceType, int> OnSuppliesAdded(Dictionary<ResourceType, int> resources)
        {
            if(resources.ContainsKey(ResourceType.Food))
                resources[ResourceType.Food] = (int)Math.Ceiling(resources[ResourceType.Food] * 1.1f);
            return resources;
        }
    }
}
