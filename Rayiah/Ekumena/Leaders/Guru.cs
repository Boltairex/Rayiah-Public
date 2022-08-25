using System;
using System.Collections.Generic;

namespace Rayiah.Ekumena.Leaders
{
    public class Guru : PlayerCharacterClasses
    {
        public override PlayerCharacterType Type => PlayerCharacterType.Guru;

        public override Dictionary<ResourceType, int> OnSuppliesAdded(Dictionary<ResourceType, int> resources)
        {
            if (resources.ContainsKey(ResourceType.Happiness))
                resources[ResourceType.Happiness] = (int)Math.Ceiling(resources[ResourceType.Happiness] * 1.1f);
            return resources;
        }
    }
}
