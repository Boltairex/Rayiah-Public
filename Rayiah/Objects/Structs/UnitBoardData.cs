using Rayiah.Objects.Interfaces;

namespace Rayiah.Objects.Structs
{
    public struct UnitBoardData : IBoard
    {
        public string name;
        public string desc;
        public string url;
        public string weaponry;
        public string color;
        public string id;
        public int power;
        public ulong guildID;
        public EmbedCustomFieldData[] fields;

        string IBoard.name => name;
        public EmbedCustomFieldData[] customFields => fields;
    }
}