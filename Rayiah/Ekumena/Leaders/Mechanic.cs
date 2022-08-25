using System;
using System.Collections.Generic;

namespace Rayiah.Ekumena.Leaders
{
    public class Mechanic : PlayerCharacterClasses
    {
        public override PlayerCharacterType Type => PlayerCharacterType.Mechanic;

        public override Dictionary<ResourceType, int> OnSuppliesUpkeep(Dictionary<ResourceType, int> resources)
        {
            if (resources.ContainsKey(ResourceType.Oil))
                resources[ResourceType.Oil] = (int)Math.Floor(resources[ResourceType.Oil] * 0.9f);
            return resources;
        }
    }
}
