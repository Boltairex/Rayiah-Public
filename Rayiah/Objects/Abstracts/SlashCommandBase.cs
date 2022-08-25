using Discord;
using Discord.WebSocket;
using Rayiah.Handlers;
using System.Threading.Tasks;

namespace Rayiah.Objects.Abstracts
{
    public abstract class SlashCommandBase
    {
        static SlashCommandHandler defaultHandler { get; } = RayiahCore.GetInstance().provider.GetService(typeof(SlashCommandHandler)) as SlashCommandHandler;

        /// <summary>
        /// Uproszczony sposób na przekazanie innej komendzie zadania.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interaction"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        protected async Task ReHandleTo(string command, SocketSlashCommand interaction, IUser author)
        {
            await defaultHandler.ReHandleTo(command, interaction, author);
        }

        /// <summary>
        /// Wybudowana komenda po inicjalizacji.
        /// </summary>
        public SlashCommandProperties Command { get; private set; }

        /// <summary>
        /// Odpowiada za to, czy output będzie widoczny tylko dla jednego użytkownika.
        /// </summary>
        public virtual bool IsPersonal { get; } = false;

        /// <summary>
        /// Ustawienie wartości na inną niż zero powoduje, że komenda staje się serwerowa.
        /// </summary>
        public virtual ulong[] GuildID { get; } = { };

        /// <summary>
        /// Inicjalizuje i buduje obiekt, który jest następnie przeznaczany do rejestru. Wywoływany niedługo po konstruktorze.
        /// </summary>
        public virtual void Initialize() { Command = GetCommand().Build(); }

        /// <summary>
        /// Przekazuje interakcje przy wywołaniu konkretnej komendy.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public abstract Task ExecuteAsync(SocketSlashCommand interaction, IUser author);

        /// <summary>
        /// Odbiera komendę do rejestracji.
        /// </summary>
        /// <returns></returns>
        protected abstract SlashCommandBuilder GetCommand();

        /*        /// <summary>
        /// Gdy komenda działa identycznie po stronie gildii i globalnie, można tym go skopiować, zamiast pisać na nowo.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected SlashCommandBuilder CopyCommand<T>() where T : SlashCommandBase
        {
            return typeof(T).GetMethod("GetCommand").Invoke(this, null) as SlashCommandBuilder;
        }*/
    }
}