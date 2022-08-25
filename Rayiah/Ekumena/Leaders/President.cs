using System;
using System.Collections.Generic;

namespace Rayiah.Ekumena.Leaders
{
    public class President : PlayerCharacterClasses
    {
        public override PlayerCharacterType Type => PlayerCharacterType.President;

        public override Dictionary<ResourceType, int> OnSuppliesAdded(Dictionary<ResourceType, int> resources)
        {
            if (resources.ContainsKey(ResourceType.Money))
                resources[ResourceType.Money] = (int)Math.Ceiling(resources[ResourceType.Money] * 1.1f);
            return resources;
        }
    }
}
