using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using LiteDB;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FCGameBot
{
    internal static class Game
    {
        private static readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);

        /// <summary>List of every command available to the system.</summary>
        private static readonly Dictionary<string, ICommand> CommandHandlers = new Dictionary<string, ICommand>();

        /// <summary>The database object.</summary>
        public static LiteDatabase Database;

        public static LiteCollection<Player> Players;

        public static TelegramBotClient Bot;
        public static User Me;

        public static void Run()
        {
            Config.Load();

            InitCommands();
            InitDatabase();
            InitTelegramBot();

            ResetEvent.WaitOne();

            Config.Save();
        }

        public static void SendMessage(ChatId chatId, string message)
        {
            // Swallow exceptions from Telegram API being unable to message a particular user.
            try
            {
                var messageTask = Bot.SendTextMessageAsync(chatId, Debugger.IsAttached ? $"[DEV MODE]\n{message}" : message, ParseMode.Markdown);

                // Run the task to send the message and wait for it to finish executing.
                messageTask.Wait();
            }
            catch (ChatNotInitiatedException)
            {
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static void RemoveMessage(Message msg)
        {
            try
            {
                Bot.DeleteMessageAsync(msg.Chat.Id, msg.MessageId).Wait();
            }
            catch (Exception)
            {
                /* ignored */
            }
        }

        private static void InitCommands()
        {
            var commandTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(ICommand).IsAssignableFrom(p)).Skip(1).ToArray();

            foreach (var handlerType in commandTypes)
            {
                var newHandlerObj = (ICommand)Activator.CreateInstance(handlerType);
                var newHandlerName = newHandlerObj.GetNames();

                foreach (var alias in newHandlerName)
                {
                    if (CommandHandlers.ContainsKey(alias))
                    {
                        throw new ApplicationException("A command with this name already exists!");
                    }

                    CommandHandlers.Add(alias, newHandlerObj);
                }
            }
        }

        private static void InitDatabase()
        {
            Database = new LiteDatabase("FCGameBot.db");

            // Player Collection
            Players = Database.GetCollection<Player>("players");
            Players.EnsureIndex(x => x.Id);
        }

        private static void InitTelegramBot()
        {
            Bot = new TelegramBotClient(Config.Data.TelegramBotApiKey);
            Me = Bot.GetMeAsync().Result;

            Bot.OnUpdate += OnUpdate;
            Bot.StartReceiving(new [] { UpdateType.Message });
        }

        private static void OnUpdate(object sender, UpdateEventArgs e)
        {
            var update = e.Update;
            var msg = update.Message;
            var chat = msg.Chat;
            var user = msg.From;

            UpdateUserData(user);

            // Only process text messages, not pictures or stickers.
            if (msg.Type != MessageType.Text)
            {
                return;
            }

            // Ignore messages that don't start with a slash command.
            if (!msg.Text.StartsWith('/'))
            {
                return;
            }

            var args = new Queue<string>(msg.Text.Substring(1).Split(' '));

            if (args.Count == 0 || string.IsNullOrWhiteSpace(args.Peek()))
            {
                return;
            }

            var alias = args.Dequeue().ToLower();

            if (!CommandHandlers.ContainsKey(alias))
            {
                return;
            }

            var isCommandPrivate = chat.Type == ChatType.Private;
            var command = CommandHandlers[alias];

            if ((!isCommandPrivate || !command.Private) && (isCommandPrivate || !command.Public))
            {
                return;
            }

            if (!isCommandPrivate && chat.Type != ChatType.Channel)
            {
                RemoveMessage(msg);
            }

            //if (msg.ReplyToMessage != null)
            //{
            //    _inReplyToMsgId = msg.ReplyToMessage.MessageId;
            //}

            var player = GetUser(user.Id);

            Bot.SendChatActionAsync(isCommandPrivate ? user.Id : chat.Id, ChatAction.Typing).Wait();

            command.Process(alias, args, chat, player);
        }

        private static void UpdateUserData(User user)
        {
            var player = GetUser(user.Id);

            if (player == null)
            {
                player = new Player
                {
                    Id = user.Id,
                    Firstname = user.FirstName,
                    Lastname = user.LastName,
                    Username = user.Username,
                    LanguageCode = user.LanguageCode
                };
            }
            else
            {
                player.Firstname = user.FirstName;
                player.Lastname = user.LastName;
                player.Username = user.Username;
                player.LanguageCode = user.LanguageCode;
            }

            Players.Upsert(player);
        }

        private static Player GetUser(int userId)
        {
            return Players.FindOne(Query.EQ("_id", userId));
        }
    }
}
