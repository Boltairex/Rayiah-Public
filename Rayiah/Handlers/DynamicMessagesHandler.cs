using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Rayiah.Objects.Abstracts;

namespace Rayiah.Handlers
{
    public sealed class DynamicMessagesHandler : HandlerBase
    {
        public static DynamicMessagesHandler instance { get; private set; }
        
        public delegate DynamicMessagesAction DynamicMessageClicked(EmbedBuilder[] embeds, SocketMessageComponent socket);

        readonly Dictionary<ulong, DynamicMessageInfo> cache;

        readonly DiscordSocketClient client;

        public DynamicMessagesHandler(DiscordSocketClient client) {
            if (instance != null) throw new Exception(GetType() + ": Singleton class is already initialized! Use 'instance' property instead.");

            cache = new Dictionary<ulong, DynamicMessageInfo>();
            this.client = client;
            this.client.ButtonExecuted += HandleButtonClicked;
            this.client.MessageDeleted += HandleMessageDeleted;
        }

        /// <summary>
        /// Needs buttons or menu attached to message to work.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="embeds"></param>
        /// <param name="action"></param>
        public void AddToListener(ulong messageId, EmbedBuilder[] embeds, DynamicMessageClicked action) {
            if (cache.ContainsKey(messageId)) throw new Exception("Message already have active listener!");

            cache.Add(messageId, new DynamicMessageInfo(embeds, action));
        }

        /// <summary>
        /// On button clicked.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        Task HandleButtonClicked(SocketMessageComponent socket) {
            if(cache.TryGetValue(socket.Message.Id, out var value)) {
                var info = value.action.Invoke(value.embeds, socket);
                switch (info) {
                    case DynamicMessagesAction.deleteFromCache:
                        cache.Remove(socket.Message.Id);
                        break;

                    case DynamicMessagesAction.destroyMessage:
                        cache.Remove(socket.Message.Id);
                        socket.Message.DeleteAsync();
                        break;

                    default: break;
                }
            }

            return Task.CompletedTask;
        }

        Task HandleMessageDeleted(Cacheable<IMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2) {
            if (cache.ContainsKey(arg1.Id))
                cache.Remove(arg1.Id);

            return Task.CompletedTask;
        }

        public override void Dispose() {
            cache.Clear();
            instance = null;
            this.client.ButtonExecuted -= HandleButtonClicked;
            this.client.MessageDeleted -= HandleMessageDeleted;
            return;
        }

        struct DynamicMessageInfo {
            public EmbedBuilder[] embeds;
            public DynamicMessageClicked action;

            public DynamicMessageInfo(EmbedBuilder[] embeds, DynamicMessageClicked action) {
                if (embeds.Length == 0 || action is null) throw new Exception("Passed nullable values: Embeds - " + 
                    (embeds.Length == 0 ? "Null" : "Fine") + ", " + (action is null ? "Null" : "Fine"));

                this.embeds = embeds;
                this.action = action;
            }
        }
    }

    public sealed class DynamicMessagesPrefabs {
        /// <summary>
        /// Recognizes: "previous, next, first, last" values. Another values will not be executed.
        /// </summary>
        /// <param name="embeds"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public DynamicMessagesAction PaginatorController(EmbedBuilder[] embeds, SocketMessageComponent socket) {
            int FindUsedEmbed() {
                var temp = socket.Message.Embeds.GetEnumerator(); temp.MoveNext();

                for (int x = 0; x < embeds.Length; x++)
                    if (Tools.Utilites.CompareEmbeds(embeds[x].Build(), temp.Current))
                        return x;
                return 0;
            }

            int usedEmbed = FindUsedEmbed();
            int newEmbed = 0;
            var en = socket.Data.Values.GetEnumerator();
            bool exit = false;
            while (en.MoveNext()) {
                switch (en.Current) {
                    case "previous":
                        exit = true;
                        newEmbed = --usedEmbed;
                        if (newEmbed == -1)
                            newEmbed = embeds.Length - 1;
                        break;

                    case "next":
                        exit = true;
                        newEmbed = ++usedEmbed;
                        if (newEmbed == embeds.Length)
                            newEmbed = 0;
                        break;

                    case "first":
                        exit = true;
                        newEmbed = 0;
                        break;

                    case "last":
                        exit = true;
                        newEmbed = embeds.Length - 1;
                        break;

                    default: break;
                }
                if (exit) break;
            }

            if (exit && usedEmbed != newEmbed)
                socket.Message.ModifyAsync(m => {
                    m.Embed = embeds[newEmbed].Build();
                }).GetAwaiter().GetResult();
            return DynamicMessagesAction.none;
        }
    }

    public enum DynamicMessagesAction {
        /// <summary>
        /// No action required
        /// </summary>
        none, 
        /// <summary>
        /// Delete message from cache and from Discord server
        /// </summary>
        destroyMessage,
        /// <summary>
        /// Delete message from cache
        /// </summary>
        deleteFromCache
    }
}