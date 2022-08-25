using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Rayiah.Objects.Abstracts
{
    public abstract class MessageCommandBase
    {
        /// <summary>
        /// Wybudowana komenda po inicjalizacji.
        /// </summary>
        public MessageCommandProperties Command { get; private set; }

        /// <summary>
        /// Czy wymaga permisji wewnętrznych bota.
        /// </summary>
        public virtual bool RequireAuthorization => false;

        /// <summary>
        /// Zabezpieczenie przed niepowołanym użyciem.
        /// </summary>
        public virtual ApplicationCommandPermission Permission { get; set; }

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
        /// <returns></returns>
        public abstract Task ExecuteAsync(SocketMessageCommand socket, SocketUser user);

        protected abstract MessageCommandBuilder GetCommand();
    }
}
