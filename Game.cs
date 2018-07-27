using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FCGameBot.Models;
using LiteDB;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = FCGameBot.Models.User;
using TelegramUser = Telegram.Bot.Types.User;

namespace FCGameBot
{
    internal static class Game
    {
        private static readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);

        /// <summary>List of every command available to the system.</summary>
        private static readonly Dictionary<string, ICommand> CommandHandlers = new Dictionary<string, ICommand>();

        /// <summary>The database object.</summary>
        public static LiteDatabase Database;

        public static LiteCollection<User> Users;
        public static LiteCollection<World> Worlds;
        public static LiteCollection<Status> Statuses;

        public static TelegramBotClient Bot;
        public static TelegramUser BotUser;

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
            // Swallow exceptions from Telegram API being unable to telegramUser a particular user.
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
            // Swallow exceptions from Telegram API being unable to remote a telegramUser.
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
            Database = new LiteDatabase(Config.Data.DatabaseFilename);

            // User Collection
            Users = Database.GetCollection<User>("users");
            Users.EnsureIndex(x => x.Id);
            Users.EnsureIndex(x => x.Username);

            // World Collection
            Worlds = Database.GetCollection<World>("worlds");
            Worlds.EnsureIndex(x => x.Id);

            Statuses = Database.GetCollection<Status>("statuses");
            Statuses.EnsureIndex(x => x.User);
            Statuses.EnsureIndex(x => x.World);

            BsonMapper.Global.Entity<Status>().DbRef(x => x.World, "worlds");
            BsonMapper.Global.Entity<Status>().DbRef(x => x.User, "users");
        }

        private static void InitTelegramBot()
        {
            Bot = new TelegramBotClient(Config.Data.TelegramBotApiKey);
            BotUser = Bot.GetMeAsync().Result;

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
            var chat = msg.Chat;
            var isWorldPrivate = chat.Type == ChatType.Private;

            var user = UpdateUserData(msg.From, msg.Chat);
            var world = isWorldPrivate ? GetWorldById(user.SelectedWorld) : UpdateWorldData(chat);

            world.Chat = chat;

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

            if (alias.Equals("help"))
            {
                await HandleHelp(msg, args);

                return;
            }

            if (!CommandHandlers.ContainsKey(alias))
            {
                return;
            }

            var command = CommandHandlers[alias];

            if ((!isWorldPrivate || !command.Private) && (isWorldPrivate || !command.Public))
            {
                return;
            }

            if (world == null)
            {
                await SendMessage(msg.From.Id, "The bot is in no room, basic setup mode...");

                return;
            }

            if (command.Admin && !user.IsAdmin)
            {
                return;
            }

            var player = GetStatus(user, world);

            player.Message = msg;

            Status targetedPlayer = null;

            if (!isWorldPrivate && command.Targetable)
            {
                if (args.Count == 0)
                {
                    return;
                }

                targetedPlayer = ParseTarget(world, ref args);

                if (targetedPlayer == null)
                {
                    return;
                }
            }

            if (!isWorldPrivate && chat.Type != ChatType.Channel)
            {
                await RemoveMessage(msg);
            }

            await Bot.SendChatActionAsync(isWorldPrivate ? user.Id : chat.Id, ChatAction.Typing);

            await command.Process(alias, args, player, targetedPlayer);
        }

        private static async Task HandleHelp(Message msg, Queue<string> args)
        {
            if (msg.Chat.Type != ChatType.Private && msg.Chat.Type != ChatType.Channel)
            {
                await RemoveMessage(msg);
            }

            await SendMessage(msg.From.Id, "NothingV2...");
        }

        private static async Task HandleMessageChatMembersAdded(Message msg)
        {
            foreach (var newUser in msg.NewChatMembers)
            {
                UpdateUserData(newUser);

                var welcomeText = new StringBuilder();

                welcomeText.AppendLine($"Welcome `@{newUser.Username}` to The Fox Council world!");

                await SendMessage(msg.Chat.Id, welcomeText.ToString());
            }
        }

        private static Status ParseTarget(World world, ref Queue<string> args)
        {
            var argsList = args.ToList();
            var usernameString = argsList.DequeueFirst(x => x.StartsWith('@')).Substring(1).ToLower();

            if (string.IsNullOrWhiteSpace(usernameString))
            {
                return null;
            }

            args = new Queue<string>(argsList);

            var user = Users.FindOne(x => x.Username.ToLower().Equals(usernameString));

            return user == null ? null : GetStatus(user, world);
        }

        #region Status Methods

        private static Status GetStatus(User user, World world)
        {
            var status = Statuses
                             .Include(x => x.User)
                             .Include(x => x.World)
                             .FindOne(x => x.User.Id == user.Id && x.World.Id == world.Id) ?? new Status(user, world);

            status.World.Chat = world.Chat;

            return status;
        }

        #endregion

        #region World Methods

        private static World UpdateWorldData(Chat chat)
        {            
            if (chat.Type == ChatType.Private)
            {
                throw new Exception("Oops");
            }

            var world = GetWorld(chat);

            Worlds.Upsert(world);

            return world;
        }

        private static World GetWorld(Chat chat)
        {
            var world = GetWorldById(chat.Id);

            if (world == null)
            {
                return new World(chat);
            }

            world.Title = chat.Title;
            world.Type = chat.Type.ToString();

            return world;
        }

        private static World GetWorldById(long chatId)
        {
            return Worlds.FindOne(Query.EQ("_id", chatId));
        }

        #endregion

        #region User Methods

        private static User UpdateUserData(TelegramUser telegramUser, Chat chat = null)
        {
            var user = GetUser(telegramUser, chat);

            Users.Upsert(user);

            return user;
        }

        private static User GetUser(TelegramUser telegramUser, Chat chat = null)
        {
            var user = GetUserById(telegramUser.Id);

            if (user == null)
            {
                var newUser = new User(telegramUser);

                if (chat != null && chat.Type != ChatType.Private)
                {
                    newUser.SelectedWorld = chat.Id;
                }

                return newUser;
            }

            user.Firstname = telegramUser.FirstName;
            user.Lastname = telegramUser.LastName;
            user.Username = telegramUser.Username;
            user.LanguageCode = telegramUser.LanguageCode;

            if (user.SelectedWorld == 0 && chat != null && chat.Type != ChatType.Private)
            {
                // Default Selected World
                user.SelectedWorld = chat.Id;
            }

            return user;
        }

        private static User GetUserById(long chatId)
        {
            return Users.FindOne(Query.EQ("_id", chatId));
        }

        #endregion
    }
}
