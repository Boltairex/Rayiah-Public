using System;
using Rayiah.AeriaStories.Objects;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;

namespace Rayiah.AeriaStories
{
    public class GamePanel
    {
        public GamePanel(CharacterInstance instance)
        {
            Character = new Character(instance);
        }

        public Character Character { get; protected set; }

        public Embed ReturnPanel()
        {
            EmbedBuilder embed = new EmbedBuilder();
            {

            }

            return embed.Build();
        }
    }
}
