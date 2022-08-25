using Discord;
using Discord.WebSocket;
using Rayiah.Handlers;
using Rayiah.Objects.Abstracts;
using System.Linq;
using System.Threading.Tasks;

namespace Rayiah.Modules.SlashCommands.Guilds
{
    public class HelpGuildCommand : SlashCommandBase
    {
        public override ulong[] GuildID => new ulong[] { 817822681050120253, 913898440025579541, 438335271587676171 };

        public override bool IsPersonal => true;

        public async override Task ExecuteAsync(SocketSlashCommand interaction, IUser author) {
            await ReHandleTo("help", interaction, author);
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
                .WithName("helpguild")
                .WithDescription("Wyswietla dostepne zbiory komend.")
                .AddOption(command);

            return slash;
        }
    }
}
