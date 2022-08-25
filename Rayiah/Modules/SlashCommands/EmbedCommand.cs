using Discord;
using Discord.WebSocket;
using Rayiah.Objects.Abstracts;
using System.Threading.Tasks;

namespace Rayiah.Modules.SlashCommands
{
    class EmbedCommand : SlashCommandBase
    {
        //public override ulong[] GuildID => new ulong[] { 762234532451057664 };

        public async override Task ExecuteAsync(SocketSlashCommand interaction, IUser author)
        {
            if (interaction.Data.Options.Count == 0)
                return;

            string[] embedFields = new string[3] {"", "", ""};
            EmbedBuilder builder = new EmbedBuilder();
            var en = interaction.Data.Options.GetEnumerator();

            while(en.MoveNext())
            {
                switch (en.Current.Name)
                {
                    case "text":
                        embedFields[0] = en.Current.Value as string;
                        break;

                    case "title":
                        embedFields[1] = en.Current.Value as string;
                        break;

                    case "url":
                        embedFields[2] = en.Current.Value as string;
                        break;

                    case "attachment":
                        embedFields[2] = (en.Current.Value as Attachment).Url;
                        break;
                }
            }

            builder.AddField(embedFields[1], embedFields[0])
                .WithColor((Color)Managers.Container.GetRandomColor());
            if (!string.IsNullOrEmpty(embedFields[2]))
                builder.WithImageUrl(embedFields[2]);

            await interaction.Channel.SendMessageAsync(embed: builder.Build());
        }

        protected override SlashCommandBuilder GetCommand()
        {
            var slash = new SlashCommandBuilder()
                .WithName("embed")
                .WithDescription("generuje embed z tekstu.")
                .AddOption(name: "title", type: ApplicationCommandOptionType.String, description: "Tytuł Embeda.")
                .AddOption(name: "text", type: ApplicationCommandOptionType.String, description: "Treść Embedu, np. 'jajco'.")
                .AddOption(name: "url", type: ApplicationCommandOptionType.String, description: "Link do zdjęcia.")
                .AddOption(name: "attachment", type: ApplicationCommandOptionType.Attachment, description: "Plik, który zastępuje link.");

            return slash;
        }
    }
}
