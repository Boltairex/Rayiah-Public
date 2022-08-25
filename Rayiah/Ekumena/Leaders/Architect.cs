using System;
using System.Collections.Generic;

namespace Rayiah.Ekumena.Leaders
{
    public class Architect : PlayerCharacterClasses
    {
        public override PlayerCharacterType Type => PlayerCharacterType.Architect;

        public override Dictionary<ResourceType, int> OnBuildSomething(Dictionary<ResourceType, int> resources)
        {
            foreach (ResourceType type in resources.Keys)
                resources[type] = (int)Math.Floor(resources[type] * 0.95f);
            return resources;
        }
    }
}
