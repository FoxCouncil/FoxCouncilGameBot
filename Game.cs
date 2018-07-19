using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public static async Task SendMessage(ChatId chatId, string message)
        {
            // Swallow exceptions from Telegram API being unable to message a particular user.
            try
            {
                await Bot.SendTextMessageAsync(chatId, message, ParseMode.Markdown);
            }
            catch (Exception)
            {
                /* ignored */
            }
        }

        public static async Task RemoveMessage(Message msg)
        {
            // Swallow exceptions from Telegram API being unable to remote a message.
            try
            {
                await Bot.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
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
            Bot.StartReceiving(new [] { UpdateType.Message, UpdateType.EditedMessage });
        }

        private static async void OnUpdate(object sender, UpdateEventArgs e)
        {
            var update = e.Update;

            if (update.Type == UpdateType.Message || update.Type == UpdateType.EditedMessage)
            {
                await HandleUpdateMessage(update);
            }
        }

        private static async Task HandleUpdateMessage(Update update)
        {
            var msg = update.Message;

            switch (msg.Type)
            {
                case MessageType.Text:
                {
                    await HandleMessageText(msg);
                }
                break;

                case MessageType.Photo:
                    break;
                case MessageType.Audio:
                    break;
                case MessageType.Video:
                    break;
                case MessageType.Voice:
                    break;
                case MessageType.Document:
                    break;
                case MessageType.Sticker:
                    break;
                case MessageType.Location:
                    break;
                case MessageType.Contact:
                    break;
                case MessageType.Game:
                    break;
                case MessageType.VideoNote:
                    break;

                case MessageType.ChatMembersAdded:
                {
                    await HandleMessageChatMembersAdded(msg);
                }
                break;

                case MessageType.ChatTitleChanged:
                    break;
                case MessageType.ChatPhotoChanged:
                    break;
                case MessageType.MessagePinned:
                    break;
                case MessageType.ChatPhotoDeleted:
                    break;
            }
        }

        private static async Task HandleMessageText(Message msg)
        {
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

            var chat = msg.Chat;
            var isCommandPrivate = chat.Type == ChatType.Private;
            var command = CommandHandlers[alias];

            if ((!isCommandPrivate || !command.Private) && (isCommandPrivate || !command.Public))
            {
                return;
            }

            //if (msg.ReplyToMessage != null)
            //{
            //    _inReplyToMsgId = msg.ReplyToMessage.MessageId;
            //}

            var user = msg.From;

            UpdateUserData(user);

            var player = GetUser(user.Id);

            if (command.Admin && !player.IsAdmin)
            {
                return;
            }

            Player targetedPlayer = null;

            if (!isCommandPrivate && command.Targetable)
            {
                if (args.Count == 0)
                {
                    return;
                }

                targetedPlayer = ParseTarget(ref args);

                if (targetedPlayer == null)
                {
                    return;
                }
            }

            if (!isCommandPrivate && chat.Type != ChatType.Channel)
            {
                await RemoveMessage(msg);
            }

            await Bot.SendChatActionAsync(isCommandPrivate ? user.Id : chat.Id, ChatAction.Typing);

            await command.Process(alias, args, chat, player, targetedPlayer);
        }

        private static async Task HandleMessageChatMembersAdded(Message msg)
        {
            foreach (var newUser in msg.NewChatMembers)
            {
                UpdateUserData(newUser);

                var welcomeText = new StringBuilder();

                welcomeText.AppendLine($"Welcome `@{newUser.Username}` to The Fox Council world!");

                await msg.Chat.Reply(welcomeText.ToString());
            }
        }

        private static Player ParseTarget(ref Queue<string> args)
        {
            var argsList = args.ToList();
            var usernameString = argsList.DequeueFirst(x => x.StartsWith('@')).Substring(1).ToLower();

            if (string.IsNullOrWhiteSpace(usernameString))
            {
                return null;
            }

            args = new Queue<string>(argsList);

            return Players.FindOne(x => x.Username.ToLower().Equals(usernameString));
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
                    LanguageCode = user.LanguageCode,
                    Actions = 10,
                    Credits = 20000,
                    Health = 100,
                    Weight = 75
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
