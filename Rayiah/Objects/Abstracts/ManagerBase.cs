using System.Threading.Tasks;

namespace Rayiah.Objects.Abstracts
{
    public class ManagerBase : RayiahObject
    {
        protected override bool AutoSave => true;

        public override Task InitializeAsync()
        {
            return base.InitializeAsync();
        }
    }
}
