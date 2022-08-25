using System;
using System.Collections.Generic;

namespace Rayiah.Ekumena.Leaders
{
    public class Miner : PlayerCharacterClasses
    {
        public override PlayerCharacterType Type => PlayerCharacterType.Miner;

        public override Dictionary<ResourceType, int> OnSuppliesAdded(Dictionary<ResourceType, int> resources)
        {
            if (resources.ContainsKey(ResourceType.Copper))
                resources[ResourceType.Copper] = (int)Math.Ceiling(resources[ResourceType.Copper] * 1.1f);
            if (resources.ContainsKey(ResourceType.Metal))
                resources[ResourceType.Metal] = (int)Math.Ceiling(resources[ResourceType.Metal] * 1.1f);
            return resources;
        }
    }
}
