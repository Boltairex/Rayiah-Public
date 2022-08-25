using Rayiah.Objects.Attributes;
using System;
using System.Threading.Tasks;

namespace Rayiah.Objects.Abstracts
{
    public abstract class HandlerBase : RayiahObject
    {
        public override string Name => GetType().Name;

        protected override bool AutoSave => true;

        [Save] protected bool RunHandler = true;

        public HandlerBase()
        {
            if (RunHandler) {
                Printl("Activated.");
            }
        }

        public override Task InitializeAsync()
        {
            return base.InitializeAsync();
        }
    }
}
