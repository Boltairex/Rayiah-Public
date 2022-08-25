using Rayiah.Objects.Interfaces;

namespace Rayiah.Objects.Structs
{
    public struct CharacterBoardData : IBoard
    {
        public string name;
        public string age;
        public string desc;
        public string url;
        public string clothes;
        public string color;
        public int power;
        public ulong guildID;
        public bool showRanking;
        public bool globalBoard;
        public EmbedCustomFieldData[] fields;

        string IBoard.name => name;
        EmbedCustomFieldData[] IBoard.customFields => fields;
    }
}