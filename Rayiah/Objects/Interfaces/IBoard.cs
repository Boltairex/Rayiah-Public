using Rayiah.Objects.Structs;

namespace Rayiah.Objects.Interfaces
{
    public interface IBoard
    {
        string name { get; }
        EmbedCustomFieldData[] customFields { get; }
    }
}
