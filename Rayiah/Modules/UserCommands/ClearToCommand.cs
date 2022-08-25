using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Rayiah.Objects.Abstracts;

namespace Rayiah.Modules.UserCommands
{
    public class ClearToCommand : MessageCommandBase
    {
        public override bool RequireAuthorization => true;

        public override async Task ExecuteAsync(SocketMessageCommand socket, SocketUser user)
        {
            await socket.RespondAsync(text: $"Usuwam do wiadomości: {socket.Data.Message.GetJumpUrl()}");

            IMessage[] messages = new IMessage[] { };
            int mark = -1;
            int counter = 1;
            do
            {
                if (counter > 5)
                    break;

                try
                {
                    messages = socket.Channel.GetMessagesAsync(100 * counter).FlattenAsync().Result.ToArray();
                    for (int x = 100 * (counter - 1); x <= 100 * counter; x++)
                    {
                        //Console.WriteLine(x + ": " + messages[x].Content);
                        if (messages[x].Id == socket.Data.Message.Id)
                        {
                            mark = x;
                            break;
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e); }
                counter++;
            }
            while (mark == -1 && counter <= 5);

            if (counter > 5)
                await socket.RespondAsync(text: "Wiadomość jest zbyt daleko.");

            if (mark == -1)
                return;

            //Console.WriteLine("mark: " + mark);

            float limit = (float)Math.Ceiling(messages.Length / 50f);
            int xPos = -50, yPos = 0;

            try
            {
                for (int i = 0; i < limit; i++)
                {
                    xPos += 50;
                    yPos += 50;
                    if (yPos > mark + 1)
                    {
                        yPos = mark + 1;
                        limit = 0;
                    }
                    //Console.WriteLine($"{xPos}, {yPos}, {limit}, {messages.Length}");
                    await(socket.Channel as ITextChannel).DeleteMessagesAsync(messages[xPos..yPos]);
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        protected override MessageCommandBuilder GetCommand()
        {
            var guildMessageCommand = new MessageCommandBuilder()
                .WithName("clearTo")
                .WithDMPermission(false);

            return guildMessageCommand;
        }
    }
}
