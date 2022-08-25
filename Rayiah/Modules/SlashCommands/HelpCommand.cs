using Discord;
using Discord.WebSocket;
using Rayiah.Objects.Abstracts;
using System.Linq;
using System.Threading.Tasks;

namespace Rayiah.Modules.SlashCommands
{
    public class HelpCommand : SlashCommandBase
    {
        public override bool IsPersonal => true;

        public async override Task ExecuteAsync(SocketSlashCommand interaction, IUser author)
        {
            if (interaction.Data.Options.Count == 1)
            {
                var en = interaction.Data.Options.First().Value as string;
                var (x, y) = Managers.Help.Instance.GetEmbedBuilder(en);

                if (x)
                    await interaction.RespondAsync(embed: y[0].WithAuthor(author).Build(), ephemeral: IsPersonal);
                else 
                {
                    if (en == "colors")
                        await interaction.RespondAsync(embed: Managers.Container.embedColors.WithAuthor(author).Build(), ephemeral: IsPersonal);
                    else
                        await interaction.RespondAsync(text: "Nie ma takiej komendy! Wystąpił błąd.", ephemeral: IsPersonal);
                }
            }
            else
                await interaction.RespondAsync(embed: Managers.Help.Instance.GetEmbedBuilder("help").Item2[0].WithAuthor(author).Build(), ephemeral: true);
        }

        protected override SlashCommandBuilder GetCommand()
        {
            var command = new SlashCommandOptionBuilder()
                .WithName("type")
                .WithDescription("Wybierz typ pomocy.")
                .WithRequired(false)
                .AddChoice("writing", "writing")
                .AddChoice("utilities", "utilities")
                .AddChoice("cboards", "cboards")
                .AddChoice("colors", "colors")
                .WithType(ApplicationCommandOptionType.String);

            var slash = new SlashCommandBuilder()
                .WithName("help")
                .WithDescription("Wyswietla dostepne zbiory komend.")
                .AddOption(command);

            return slash;
        }
    }
}
