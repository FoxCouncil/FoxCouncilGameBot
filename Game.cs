using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FCGameBot.Models;
using LiteDB;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FCGameBot
{
    #region Aliases

    internal class InlineKeyboard : Dictionary<string, string> { }

    internal class Callback : Tuple<Message, ICommand> { public Callback(Message item1, ICommand item2) : base(item1, item2) { } }

    #endregion

    internal static class Game
    {
        public const string Version = "0.1";

        private static readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);

        /// <summary>List of every command available to the system.</summary>
        private static readonly Dictionary<string, ICommand> CommandHandlers = new Dictionary<string, ICommand>();
        private static readonly HashSet<ICommand> Commands = new HashSet<ICommand>();

        /// <summary>Keeps track of which command gets the callback messages.</summary>
        private static readonly Dictionary<string, Callback> CallbackQueue = new Dictionary<string, Callback>();
        private static readonly Dictionary<string, int> CallbackQueueCommands = new Dictionary<string, int>();

        /// <summary>The database object.</summary>
        public static LiteDatabase Database;

        // World Constructs
        public static LiteCollection<Food> Foods;

        public static LiteCollection<BankAccount> BankAccounts;
        public static LiteCollection<BankTransaction> BankTransactions;

        // Player State
        public static LiteCollection<Player> Users;
        public static LiteCollection<World> Worlds;
        public static LiteCollection<Status> Statuses;

        /// <summary>Telegram handlers</summary>
        public static TelegramBotClient Bot;
        public static User BotUser;

        public static void Run()
        {
            Config.Load();

            InitCommands();
            InitDatabase();
            InitTelegramBot();

            ResetEvent.WaitOne();

            Config.Save();
        }

        public static async Task SendMessage(Status playerStatus, string message)
        {
            await SendMessage(playerStatus.Id, message);
        }

        public static async Task SendMessage(ChatId chatId, string message, int replyToMessageId = 0)
        {
            // Swallow exceptions from Telegram API being unable to telegramUser a particular player.
            try
            {
                await Bot.SendTextMessageAsync(chatId, message, ParseMode.Markdown, replyToMessageId: replyToMessageId);
            }
            catch (Exception)
            {
                /* ignored */
            }
        }

        public static async Task SendMessage(ChatId chatId, string message, Dictionary<string, string> inlineKeyboard, ICommand sourceCommand)
        {
            var commandKey = $"{chatId}-{sourceCommand.GetType()}";

            if (CallbackQueueCommands.TryGetValue(commandKey, out var oldMessageId))
            {
                var oldMessageKey = $"{oldMessageId}-{chatId}";

                if (CallbackQueue.TryGetValue(oldMessageKey, out var oldCallbackData))
                {
                    // Keep things clean
                    await RemoveMessage(oldCallbackData?.Item1);

                    CallbackQueue.Remove(oldMessageKey);
                }

                CallbackQueueCommands.Remove(commandKey);
            }

            Message newMessage = null;

            // Swallow exceptions from Telegram API being unable to telegramUser a particular player.
            try
            {
                newMessage = await Bot.SendTextMessageAsync(chatId, message, ParseMode.Markdown, replyMarkup: InlineKeyboardMarkupMaker(inlineKeyboard));
            }
            catch (Exception)
            {
                /* ignored */
            }

            if (newMessage is null)
            {
                throw new Exception("OMG, oh no....");
            }

            // Clean State
            CallbackQueueCommands.Add(commandKey, newMessage.MessageId);
            CallbackQueue.Add($"{newMessage.MessageId}-{chatId}", new Callback(newMessage, sourceCommand));
        }

        public static async Task<Message> EditMessage(Message msgToEdit, string newText, Dictionary<string, string> inlineKeyboard = null)
        {
            Message editedMessage = null;

            try
            {
                editedMessage = await Bot.EditMessageTextAsync(msgToEdit.Chat.Id, msgToEdit.MessageId, newText, ParseMode.Markdown, true, InlineKeyboardMarkupMaker(inlineKeyboard));
            }
            catch (Exception)
            {
                /* ignored */
            }

            return editedMessage;
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

        public static World GetSelectedWorld(Status player)
        {
            return GetSelectedWorld(player.Player);
        }

        public static World GetSelectedWorld(Player player)
        {
            if (player.SelectedWorld == default(long))
            {
                return null;
            }

            return Worlds.FindOne(x => x.Authorized && x.Id == player.SelectedWorld);
        }

        private static void InitCommands()
        {
            var commandTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(ICommand).IsAssignableFrom(p)).Skip(1).ToArray();

            foreach (var handlerType in commandTypes)
            {
                var newHandlerObj = (ICommand)Activator.CreateInstance(handlerType);
                var newHandlerName = newHandlerObj.Names;

                Commands.Add(newHandlerObj);

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

            // Food Collection
            Foods = Database.GetCollection<Food>("foods");
            Foods.EnsureIndex(x => x.Id);
            Foods.EnsureIndex(x => x.Name);
            Foods.EnsureIndex(x => x.NamePlural);
            Foods.EnsureIndex(x => x.IsHealthy);

            // Bank Accounts Collection
            BankAccounts = Database.GetCollection<BankAccount>("bank_accounts");
            BankAccounts.EnsureIndex(x => x.Id);
            BankAccounts.EnsureIndex(x => x.OwnerId);

            // Bank Transactions Collection
            BankTransactions = Database.GetCollection<BankTransaction>("bank_transactions");
            BankTransactions.EnsureIndex(x => x.Id);
            BankTransactions.EnsureIndex(x => x.AccountId);

            // Player Collection
            Users = Database.GetCollection<Player>("players");
            Users.EnsureIndex(x => x.Id);
            Users.EnsureIndex(x => x.Username);

            // World Collection
            Worlds = Database.GetCollection<World>("worlds");
            Worlds.EnsureIndex(x => x.Id);

            // Status Collection
            Statuses = Database.GetCollection<Status>("statuses");
            Statuses.EnsureIndex(x => x.Player);
            Statuses.EnsureIndex(x => x.World);

            BsonMapper.Global.Entity<Status>().DbRef(x => x.Player, "players");
            BsonMapper.Global.Entity<Status>().DbRef(x => x.World, "worlds");
        }

        private static void InitTelegramBot()
        {
            Bot = new TelegramBotClient(Config.Data.TelegramBotApiKey);
            BotUser = Bot.GetMeAsync().Result;

            Bot.OnUpdate += OnUpdate;
            Bot.StartReceiving(new[] { UpdateType.Message, UpdateType.EditedMessage, UpdateType.CallbackQuery });
        }

        private static async void OnUpdate(object sender, UpdateEventArgs e)
        {
            var update = e.Update;

            if (update.Type == UpdateType.Message || update.Type == UpdateType.EditedMessage)
            {
                await HandleUpdateMessage(update);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(update.CallbackQuery);
            }
        }

        private static async Task HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            var callbackKey = $"{callbackQuery.Message.MessageId}-{callbackQuery.From.Id}";

            if (CallbackQueue.TryGetValue(callbackKey, out var callback))
            {
                var msg = callback.Item1;
                var command = callback.Item2;

                await command.Callback(callbackQuery.Data, callbackQuery.Message ?? msg, GetUser(callbackQuery.From));
            }
            else
            {
                await RemoveMessage(callbackQuery.Message);
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

            if (!isWorldPrivate)
            {
                UpdateWorldData(chat);
            }

            var world = isWorldPrivate ? new World(chat) : GetWorld(chat);

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
                await HandleHelp(user, msg, args);

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

            world.Chat = chat;

            if (command.Admin && !user.IsAdmin)
            {
                return;
            }

            var player = GetStatus(user, world);

            player.Message = msg;

            Status targetedPlayer = null;

            if (!isWorldPrivate && command.Targetable)
            {
                if (args.Count != 0)
                {
                    targetedPlayer = ParseTarget(world, ref args);
                }
            }

            if (!isWorldPrivate && chat.Type != ChatType.Channel)
            {
                await RemoveMessage(msg);
            }

            await Bot.SendChatActionAsync(isWorldPrivate ? user.Id : chat.Id, ChatAction.Typing);

            await command.Process(alias, args, player, targetedPlayer);
        }

        private static async Task HandleHelp(Player player, Message msg, Queue<string> args)
        {
            if (msg.Chat.Type != ChatType.Private && msg.Chat.Type != ChatType.Channel)
            {
                await RemoveMessage(msg);
            }

            if (args.TryPeek(out var commandString) && CommandHandlers.ContainsKey(commandString.ToLower()))
            {
                await CommandHandlers[commandString.ToLower()].Help(args, player);

                return;
            }

            const string adminTokenString = " [[ADMIN]]";

            var helpText = new StringBuilder();

            helpText.AppendLine($"The Fox Council Game - V{Version} - Help");

            var globalCommands = Commands.Where(x =>
                x.Private && x.Public && !x.Admin ||
                x.Private && x.Public && x.Admin == player.IsAdmin
            ).ToList();

            if (globalCommands.Count != 0)
            {
                helpText.AppendLine();
                helpText.AppendLine("**__Global__ Commands**:");

                foreach (var command in globalCommands)
                {
                    helpText.AppendLine($"  /{command.Names[0]} - %%SHORTDESC%%{(command.Admin ? adminTokenString : string.Empty)}");
                }
            }

            var worldCommands = Commands.Where(x =>
                !x.Private && x.Public && !x.Admin ||
                !x.Private && x.Public && x.Admin == player.IsAdmin
            ).ToList();

            if (worldCommands.Count != 0)
            {
                helpText.AppendLine();
                helpText.AppendLine("**__World__ Commands**:");

                foreach (var command in worldCommands)
                {
                    helpText.AppendLine($"  /{command.Names[0]} - %%SHORTDESC%%{(command.Admin ? adminTokenString : string.Empty)}");
                }
            }

            var botCommands = Commands.Where(x =>
                x.Private && !x.Public && !x.Admin ||
                x.Private && !x.Public && x.Admin == player.IsAdmin
            ).ToList();

            if (botCommands.Count != 0)
            {
                helpText.AppendLine();
                helpText.AppendLine("**__Bot__ Commands**:");

                foreach (var command in botCommands)
                {
                    helpText.AppendLine($"  /{command.Names[0]} - %%SHORTDESC%%{(command.Admin ? adminTokenString : string.Empty)}");
                }
            }

            await SendMessage(msg.From.Id, helpText.ToString());
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

        #region Helper Methods

        private static InlineKeyboardMarkup InlineKeyboardMarkupMaker(Dictionary<string, string> items)
        {
            var ik = items.Select(item => new[] { InlineKeyboardButton.WithCallbackData(item.Value, item.Key) }).ToArray();

            return new InlineKeyboardMarkup(ik);
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

        #endregion

        #region Status Methods

        public static Status GetStatus(Player player, World world)
        {
            var status = Statuses.IncludeAll().FindOne(x => x.Player.Id == player.Id && x.World.Id == world.Id) ?? new Status(player, world);

            status.World.Chat = world.Chat;

            return status;
        }

        #endregion

        #region World Methods

        private static void UpdateWorldData(Chat chat)
        {            
            if (chat.Type == ChatType.Private)
            {
                return;
            }

            var world = GetWorld(chat);

            Worlds.Upsert(world);
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

        #region Player Methods

        private static Player UpdateUserData(User telegramUser, Chat chat = null)
        {
            var user = GetUser(telegramUser, chat);

            Users.Upsert(user);

            return user;
        }

        private static Player GetUser(User telegramUser, Chat chat = null)
        {
            var user = GetUserById(telegramUser.Id);

            if (user == null)
            {
                var newUser = new Player(telegramUser);

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

        private static Player GetUserById(long chatId)
        {
            return Users.FindOne(Query.EQ("_id", chatId));
        }

        #endregion
    }
}
