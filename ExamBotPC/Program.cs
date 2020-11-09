using ExamBotPC.Commands;
using ExamBotPC.Tests;
using ExamBotPC.Tests.Questions;
using ExamBotPC.UserSystem;
using log4net;
using log4net.Config;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExamBotPC
{
    class Program
    {
        public static TelegramBotClient bot;
        public static ReplyKeyboardMarkup menu = new ReplyKeyboardMarkup(), menutest = new ReplyKeyboardMarkup(), menuphone = new ReplyKeyboardMarkup();
        public static List<Command> commands = new List<Command>();
        public static List<User> users = new List<User>();
        public static List<Question> questions = new List<Question>();
        public static List<Lesson> lessons = new List<Lesson>();
        public static List<Group> groups = new List<Group>();
        public static Lesson currentlesson = new Lesson();
        public static Lesson nextlesson = new Lesson();
        public static DateTime startdate = new DateTime();
        public static string connectionString = new MySqlConnectionStringBuilder()
        {
            Server = APIKeys.DBServer,
            Database = APIKeys.DBName,
            UserID = APIKeys.DBUser,
            Password = APIKeys.DBPassword,
            CharacterSet = "utf8",
            ConvertZeroDateTime = true,
            ConnectionTimeout = 60,
            Pooling = true,
            SslMode = MySqlSslMode.None
        }.ConnectionString;
        public static bool useTimer = true;
        public static char[] delimiterChars = { ',', '.', '\t', '\n', ';' };
        public static int Type;
        public static string[] presets = {"", "Фінішна пряма 🏁", "Не засинати❗️", "Тримаємо темп 🏃", "Поїхали далі 👉", "Можеш ще краще💪" };
        public static ILog log;


        static DateTime users_update, lessons_update, groups_update; 
        static Timer TestTimer, StopTimer, HMTimer, WebinarTimer, LinkTimer;

        [STAThread]
        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log = LogManager.GetLogger("Bot");
            //log4net.Util.LogLog.InternalDebugging = true;

            AppDomain currentDomain = default(AppDomain);
            currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;

            Console.WriteLine("Enter command:");
            while (true)
            {
                Console.Write("=>");

                string cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "help": Console.WriteLine("list - show all types of bot\nstart - start bot\nquit - close bot");  break;
                    case "list": Console.WriteLine("0 - localhost\n1 - Ukrainian\n2 - Math\n3 - Biology\n4 - History\n5 - Geography\n6 - English");  break;
                    case "start":
                        Console.WriteLine("Enter type number of bot:");
                        string x = Console.ReadLine();
                        switch (x)
                        {
                            case "0":
                                //log = LogManager.GetLogger(SubjectType.Ukrainian.ToString());
                                log4net.GlobalContext.Properties["BotName"] = "test";
                                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                                Type = (int)SubjectType.Ukrainian;
                                connectionString = new MySqlConnectionStringBuilder()
                                {
                                    Server = APIKeys.DBLocalServer,
                                    Database = APIKeys.DBLocalName,
                                    UserID = APIKeys.DBLocalUser,
                                    Password = APIKeys.DBLocalPassword,
                                    ConvertZeroDateTime = true
                                }.ConnectionString;
                                StartBot(APIKeys.TestBotApi);
                                break;
                            case "1":
                                //log = LogManager.GetLogger(SubjectType.Ukrainian.ToString());
                                log4net.GlobalContext.Properties["BotName"] = "ukr";
                                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                                Type = (int)SubjectType.Ukrainian;
                                StartBot(APIKeys.UkrBotApi);
                                break;
                            case "2":
                                //log = LogManager.GetLogger(SubjectType.Math.ToString());
                                log4net.GlobalContext.Properties["BotName"] = "math";
                                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                                Type = (int)SubjectType.Math;
                                StartBot(APIKeys.MathBotApi);
                                break;
                            case "3":
                                //log = LogManager.GetLogger(SubjectType.Biology.ToString());
                                log4net.GlobalContext.Properties["BotName"] = "bio";
                                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                                Type = (int)SubjectType.Biology;
                                StartBot(APIKeys.BioBotApi);
                                break;
                            case "4":
                                //log = LogManager.GetLogger(SubjectType.History.ToString());
                                log4net.GlobalContext.Properties["BotName"] = "his";
                                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                                Type = (int)SubjectType.History;
                                StartBot(APIKeys.HisBotApi);
                                break;
                            case "5":
                                //log = LogManager.GetLogger(SubjectType.Geography.ToString());
                                log4net.GlobalContext.Properties["BotName"] = "geo";
                                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                                Type = (int)SubjectType.Geography;
                                StartBot(APIKeys.GeoBotApi);
                                break;
                            case "6":
                                //log = LogManager.GetLogger(SubjectType.English.ToString());
                                log4net.GlobalContext.Properties["BotName"] = "eng";
                                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                                Type = (int)SubjectType.English;
                                StartBot(APIKeys.EngBotApi);
                                break;
                            default: Console.WriteLine("Wrong number!"); break;
                        }
                        break;
                    case "msg":
                        {
                            for(int i = 0; i < users.Count; i++)
                            {
                                try
                                {
                                    if (users[i].subjects.Contains(Type.ToString() + ";"))
                                    {
                                        bot.SendTextMessageAsync(users[i].id, "Ще раз оновили 🤓\nТепер натискай: 👇", replyMarkup: users[i].ontest ? menutest : menu);
                                    }
                                }
                                catch(Exception e)
                                {
                                    log.Error($"There was an exception for {users[i].id} with msg: {e.Message}");
                                    Console.WriteLine($"There was an exception for {users[i].id} with msg: {e.Message}");
                                }
                            }
                            break;
                        }
                    case "quit":
                        {
                            SaveState();
                            Environment.Exit(0);
                            break;
                        }
                    default: Console.WriteLine("No such command found");  break;
                }
            }
        }

        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = default(Exception);
            ex = (Exception)e.ExceptionObject;
            log.Error(ex.Message + "\n" + ex.StackTrace);
        }

        private static void StartBot(string Api)
        {
            log.Info("Starting bot...");

            //Loading data
            LoadFromDB();

            log.Info("Loaded");

            users_update = lessons_update = groups_update = DateTime.Now;
            //Add all commands
            AddAllCommands();
            //Initialize timers
            UpdateDBTimer();
            HMNotificationTimer();
            WebinarNotificationTimer();
            InitializeTestTimer();
            LinkSenderTimer();
            
            //Console.WriteLine(currentlesson.id +" || "+ currentlesson.group.id+"\n"+nextlesson.id +" || "+ nextlesson.group.id);
            //Initialize bot client
            bot = new TelegramBotClient(Api);// { Timeout = TimeSpan.FromSeconds(10) };

            //Starting message
            var me = bot.GetMeAsync().Result;
            Console.WriteLine($"Its me, {me}!");

            startdate = DateTime.Now;
            bot.StartReceiving();
            bot.OnMessage += Bot_OnMessage;
            bot.OnCallbackQuery += Bot_OnCallbackQuery;
            CreateMenu();

            log.Info("Bot started");
        }

        private static void CreateMenu()
        {
            log.Info("Creating menu...");
            KeyboardButton btn = KeyboardButton.WithRequestContact("Відправити контакт ☎");
            menuphone = new ReplyKeyboardMarkup(btn);
            menuphone.ResizeKeyboard = true;

            menu.Keyboard = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Записи уроків ▶"),
                    new KeyboardButton("Мій куратор 💬")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Розклад 📅"),
                    new KeyboardButton("Мій акаунт 🧑🏼‍🎓")
                },
            };
            menu.ResizeKeyboard = true;
            menu.OneTimeKeyboard = false;

            menutest.Keyboard = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Записи уроків ▶"),
                    new KeyboardButton("Мій куратор 💬")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Розклад 📅"),
                    new KeyboardButton("Мій акаунт 🧑🏼‍🎓")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Повторити запитання з тесту 🔄"),
                },
            };
            menutest.ResizeKeyboard = true;
            menutest.OneTimeKeyboard = false;
            log.Info("Menu created");
        }

        public static void SaveState()
        {
            log.Info("Saving state for all users...");
            for(int i = 0; i < users.Count; i++)
            {
                if (users[i].groups.Contains(currentlesson.group.id) && currentlesson.group.type == Type)
                {
                    string prefix = "";
                    switch (Type)
                    {
                        case 0:
                            {
                                prefix = "A";
                                break;
                            }
                        case 1:
                            {
                                prefix = "A";
                                break;
                            }
                        case 2:
                            {
                                prefix = "B";
                                break;
                            }
                        case 3:
                            {
                                prefix = "C";
                                break;
                            }
                        case 4:
                            {
                                prefix = "D";
                                break;
                            }
                        case 5:
                            {
                                prefix = "E";
                                break;
                            }
                        case 6:
                            {
                                prefix = "F";
                                break;
                            }
                        case 7:
                            {
                                prefix = "G";
                                break;
                            }
                        case 8:
                            {
                                prefix = "K";
                                break;
                            }
                        default: break;
                    }
                    string state = $"{prefix}{(users[i].ontest ? 1 : 0)};{users[i].currentlesson.id};{users[i].currentquestion};{users[i].points};{users[i].mistakes}";
                    ExecuteMySql($"UPDATE users SET State = REPLACE(State, '{users[i].state[Type - 1]}', '{state}') WHERE id = {users[i].id}");
                    users[i].state[Type - 1] = state;
                }
            }
            log.Info("States saved");
        }

        public static void SaveState(User u)
        {
            log.Info($"Saving state for {u.id}");
            string prefix = "";
            switch (Type)
            {
                case 0:
                    {
                        prefix = "A";
                        break;
                    }
                case 1:
                    {
                        prefix = "A";
                        break;
                    }
                case 2:
                    {
                        prefix = "B";
                        break;
                    }
                case 3:
                    {
                        prefix = "C";
                        break;
                    }
                case 4:
                    {
                        prefix = "D";
                        break;
                    }
                case 5:
                    {
                        prefix = "E";
                        break;
                    }
                case 6:
                    {
                        prefix = "F";
                        break;
                    }
                case 7:
                    {
                        prefix = "G";
                        break;
                    }
                case 8:
                    {
                        prefix = "K";
                        break;
                    }
                default: break;
            }
            string state = $"{prefix}{(u.ontest ? 1 : 0)};{u.currentlesson.id};{u.currentquestion};{u.points};{u.mistakes}";
            ExecuteMySql($"UPDATE users SET State = REPLACE(State, '{u.state[Type - 1]}', '{state}') WHERE id = {u.id}");
            u.state[Type - 1] = state;
            log.Info("State saved");
        }

        private async static void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            log.Info($"On callback querry '{e.CallbackQuery.Data}' from {e.CallbackQuery.From.Id}");
            try 
            {
                if (e.CallbackQuery.Message.Date.AddHours(3) < startdate)
                {
                    log.Info($"Old callback. Callback from {e.CallbackQuery.From.Id} end");
                    return;
                }
                User user = GetCurrentUser(e);
                if (e.CallbackQuery.Message.MessageId != user.lastmsg)
                {
                    log.Info($"Old callback. Callback from {e.CallbackQuery.From.Id} end");
                    return;
                }
                user.lastmsg = 0;

                //check if we have phone number
                if (String.IsNullOrEmpty(user.phone))
                {
                    await bot.SendTextMessageAsync(user.id, "Привіт! 👋\n\nДля успішної роботи з ботом потрібно відправити йому свій контакт. Це допоможе боту зв'язати тебе з твоєю анкетою 📝", replyMarkup: menuphone);
                    return;
                }

                //if user is on test
                if (user.ontest)
                {
                    Question question = user.currentlesson.test.questions[user.currentquestion];
                    if (!(user.currentlesson.test.questions[user.currentquestion] is TestQuestion))
                        return;
                    //Check answer is right or wrong
                    if (e.CallbackQuery.Data == question.answer)
                    {
                        log.Info($"Answer '{e.CallbackQuery.Data}' from {user.id} on {user.currentlesson.id}/{user.currentquestion+1} was right!");
                        await bot.SendTextMessageAsync(user.id, "Правильно!");
                        user.currentquestion++;
                        user.points += question.points;
                    }
                    else
                    {
                        log.Info($"Answer '{e.CallbackQuery.Data}' from {user.id} on {user.currentlesson.id}/{user.currentquestion+1} was wrong!");
                        await bot.SendTextMessageAsync(user.id, $"Неправильно! Можеш дізнатись рішення у завдання у свого куратора.\n\nПравильна відповідь: { question.variants[Int32.Parse(question.answer) - 1]}");
                        user.mistakes++;
                        user.currentquestion++;
                    }
                    //Check if its last question in test
                    if (user.currentquestion >= user.currentlesson.test.questions.Count)
                    {
                        string prefix = "";
                        switch (Type)
                        {
                            case 0:
                                {
                                    prefix = "A";
                                    break;
                                }
                            case 1:
                                {
                                    prefix = "A";
                                    break;
                                }
                            case 2:
                                {
                                    prefix = "B";
                                    break;
                                }
                            case 3:
                                {
                                    prefix = "C";
                                    break;
                                }
                            case 4:
                                {
                                    prefix = "D";
                                    break;
                                }
                            case 5:
                                {
                                    prefix = "E";
                                    break;
                                }
                            case 6:
                                {
                                    prefix = "F";
                                    break;
                                }
                            case 7:
                                {
                                    prefix = "G";
                                    break;
                                }
                            case 8:
                                {
                                    prefix = "K";
                                    break;
                                }
                            default: break;
                        }

                        if (ExecuteMySql($"UPDATE users SET Stats = CONCAT(Stats, '{prefix}:{user.currentlesson.id}:{user.mistakes}/{user.currentlesson.test.questions.Count}:{user.points};') WHERE ID = {user.id}"))
                        {
                            user.statistic += $"{prefix}:{user.currentlesson.id}:{user.mistakes}/{user.currentlesson.test.questions.Count}:{user.points};";
                            user.ontest = false;
                            user.currentquestion = 0;
                            await bot.SendTextMessageAsync(user.id, $"Вітаю! Ви закінчили тест. Ви набрали {user.points} балів!", replyMarkup: menu);
                            user.points = 0;
                            user.mistakes = 0;
                        }
                        else
                        {
                            Console.WriteLine("Error!");
                        }
                    }
                    else
                    {
                        user.currentlesson.test.questions[user.currentquestion].Ask(user.id);
                    }
                }
                SaveState(user);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "\n" + ex.StackTrace);
            }
            log.Info($"CallbackQuerry from {e.CallbackQuery.From.Id} end");
        }

        private async static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            log.Info($"On message recieved '{e.Message.Text}' from {e.Message.From.Id}");
            try
            {
                if (e.Message.Date.AddHours(3) < startdate)
                    return;

                User user;

                //check if user is subscriber
                if (users.Find(x => x.id == e.Message.Chat.Id) != default(User))
                {
                    user = GetCurrentUser(e);
                    //check if name or username changed
                    if (e.Message.Chat.Username != user.username || e.Message.Chat.FirstName + " " + e.Message.Chat.LastName != user.name)
                    {
                        if (ExecuteMySql($"UPDATE users SET Name = '{e.Message.Chat.FirstName}', Soname = '{e.Message.Chat.LastName}', Username = '{e.Message.Chat.Username}' WHERE id = {user.id}"))
                        {
                            user.username = e.Message.Chat.Username;
                            user.name = e.Message.Chat.FirstName + " " + e.Message.Chat.LastName;
                        }
                    }

                    //check if we have phone nubmer
                    if (String.IsNullOrEmpty(user.phone))
                    {
                        if (e.Message.Contact != null)
                        {
                            if (e.Message.Contact.FirstName + " " + e.Message.Contact.LastName == user.name)
                            {
                                if (ExecuteMySql($"UPDATE users SET Phone = '{e.Message.Contact.PhoneNumber}' WHERE id = {user.id}"))
                                {
                                    user.phone = e.Message.Contact.PhoneNumber;
                                    if (user.ontest)
                                        await bot.SendTextMessageAsync(user.id, "Готово! 🎉", replyMarkup: menutest);
                                    if (!user.ontest)
                                        await bot.SendTextMessageAsync(user.id, "Готово! 🎉", replyMarkup: menu);
                                }
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(user.id, "На жаль, без відправки контакту працювати з ботом не вийде 😿\n\nТисни на кнопку 👇", replyMarkup: menuphone);
                            }
                        }
                        else
                        {
                            await bot.SendTextMessageAsync(user.id, "Привіт! 👋\n\nДля успішної роботи з ботом потрібно відправити йому свій контакт. Це допоможе боту зв'язати тебе з твоєю анкетою 📝", replyMarkup: menuphone);
                        }
                        return;
                    }

                    //add subscribe to bot
                    if (!user.subjects.Contains($"{Type};"))
                    {
                        if (ExecuteMySql($"UPDATE users SET subjects = CONCAT(subjects, '{Type};') WHERE id = {user.id}"))
                        {
                            user.subjects += $"{Type};";
                            await bot.SendTextMessageAsync(e.Message.Chat.Id, "Привіт!\n\n" +
                                                    "💪 <b>Вітаю в POWER - групі!</b> 💪\n\n" +
                                                    "Це бот, який буде повідомляти про:\n\n" +
                                                    "- Трансляції\n" +
                                                    "- Новини щодо груп та розкладу\n" +
                                                    "- Домашки\n" +
                                                    "- Просто круті штуки: 😽\n\n" +
                                                    "<b>Скоро куратор приєднає тебе до групки та сюди почнуть приходити твої уроки.</b> Посилання на урок приходить за 5 хвилин до початку.\n\n" +
                                                    "Не забувай робити домашні завдання, адже у тебе усього 5 життів на місяць 🤓\n" +
                                                    "Побачимося на трансляціях! 👋🥰🚀", replyMarkup: menu, parseMode: ParseMode.Html);
                            return;
                        }
                    }
                }
                else
                {
                    UpdateUsers();
                    if (users.Find(x => x.id == e.Message.Chat.Id) == default(User))
                        if (ExecuteMySql($"INSERT INTO users (ID, Name, Soname, Username, Date, Subjects, Health, Curator, State) VALUES ({e.Message.Chat.Id}, '{e.Message.Chat.FirstName}', '{e.Message.Chat.LastName}', '{e.Message.Chat.Username}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', '{Type};', '5;5;5;5;5;5;5;5', '0', 'A0;0|B0;0|C0;0|D0;0|E0;0|F0;0|G0;0|K0;0')"))
                        {
                            users.Add(new User(e.Message.Chat.Id, "", e.Message.Chat.FirstName + " " + e.Message.Chat.LastName, e.Message.Chat.Username, "5;5;5;5;5;5;5;5", 0, "", "0", $"{Type};", "", "A0;0|B0;0|C0;0|D0;0|E0;0|F0;0|G0;0|K0;0"));
                            await bot.SendTextMessageAsync(e.Message.Chat.Id, "Привіт!\n\n" +
                                                    "💪 <b>Вітаю в POWER - групі!</b> 💪\n\n" +
                                                    "Це бот, який буде повідомляти про:\n\n" +
                                                    "- Трансляції\n" +
                                                    "- Новини щодо груп та розкладу\n" +
                                                    "- Домашки\n" +
                                                    "- Просто круті штуки: 😽\n\n" +
                                                    "<b>Скоро куратор приєднає тебе до групки та сюди почнуть приходити твої уроки.</b> Посилання на урок приходить за 5 хвилин до початку.\n\n" +
                                                    "Не забувай робити домашні завдання, адже у тебе усього 5 життів на місяць 🤓\n" +
                                                    "Побачимося на трансляціях! 👋🥰🚀", replyMarkup: menuphone, parseMode: ParseMode.Html);
                        }
                    return;
                }

                //Add user in temp var
                user = GetCurrentUser(e);

                var text = e?.Message?.Text;
                if (text == null) return;
                //Check commands
                else if (commands.Find(c => c.Name == text) != null)
                {
                    Command cmd = commands.Find(c => c.Name == text);
                    if (!cmd.forAdmin)
                    {
                        cmd.Execute(e);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(user.id, "У вас немає доступу до цієї команди");
                    }
                }
                //If user completing test
                else if (user.ontest)
                {
                    string answer = e.Message.Text;
                    Question question = user.currentlesson.test.questions[user.currentquestion];
                    //Check conformity question
                    if (user.currentlesson.test.questions[user.currentquestion] is ConformityQuestion)
                    {
                        ConformityQuestion q = (ConformityQuestion)user.currentlesson.test.questions[user.currentquestion];
                        if (Regex.IsMatch(answer, "[a-z]", RegexOptions.IgnoreCase))
                        {
                            log.Info($"Answer '{answer}' from {user.id} has latin");
                            await bot.SendTextMessageAsync(user.id, "У вашій відповіді була помічена латиниця. Будь ласка, використовуйте лише кирилицю! Повторіть вашу відповідь кирилицею.");
                        }
                        else
                        {
                            //Delete spaces
                            if (q.IsRight(answer))
                            {
                                log.Info($"Answer '{answer}' from {user.id} on {user.currentlesson.id}/{user.currentquestion+1} was right!");
                                await bot.SendTextMessageAsync(user.id, "Правильно!");
                                user.currentquestion++;
                                user.points += question.points;
                            }
                            else
                            {
                                log.Info($"Answer '{answer}' from {user.id} on {user.currentlesson.id}/{user.currentquestion+1} was wrong! Right answer is {question.answer}");
                                await bot.SendTextMessageAsync(user.id, $"Неправильно! Можеш дізнатись рішення у завдання у свого куратора.\n\nПравильна відповідь: {question.answer}");
                                user.mistakes++;
                                user.currentquestion++;
                            }
                        }
                    }
                    else if (user.currentlesson.test.questions[user.currentquestion] is MultipleQuestion)
                    {
                        MultipleQuestion q = (MultipleQuestion)user.currentlesson.test.questions[user.currentquestion];
                        //Delete spaces
                        if (q.IsRight(answer))
                        {
                            log.Info($"Answer '{answer}' from {user.id} on {user.currentlesson.id}/{user.currentquestion + 1} was right!");
                            await bot.SendTextMessageAsync(user.id, "Правильно!");
                            user.currentquestion++;
                            user.points += question.points;
                        }
                        else
                        {
                            log.Info($"Answer '{answer}' from {user.id} on {user.currentlesson.id}/{user.currentquestion + 1} was wrong! Right answer is {question.answer}");
                            await bot.SendTextMessageAsync(user.id, $"Неправильно! Можеш дізнатись рішення у завдання у свого куратора.\n\nПравильна відповідь: {question.answer}");
                            user.mistakes++;
                            user.currentquestion++;
                        }
                    }
                    //Check other type
                    else if (user.currentlesson.test.questions[user.currentquestion] is TestQuestion)
                    {
                        if (answer.ToLower() == question.variants[Int32.Parse(question.answer) - 1].ToLower())
                        {
                            log.Info($"Answer '{answer}' from {user.id} on {user.currentlesson.id}/{user.currentquestion + 1} was right!");
                            await bot.SendTextMessageAsync(user.id, "Правильно!");
                            user.currentquestion++;
                            user.points += question.points;
                        }
                        else
                        {
                            log.Info($"Answer '{answer}' from {user.id} on {user.currentlesson.id}/{user.currentquestion + 1} was wrong! Right answer is {question.variants[Int32.Parse(question.answer) - 1]}");
                            await bot.SendTextMessageAsync(user.id, $"Неправильно! Можеш дізнатись рішення у завдання у свого куратора.\n\nПравильна відповідь: {question.variants[Int32.Parse(question.answer) - 1]}");
                            user.mistakes++;
                            user.currentquestion++;
                        }
                    }
                    else if (answer.ToLower() == question.answer.ToLower())
                    {
                        log.Info($"Answer '{answer}' from {user.id} on {user.currentlesson.id}/{user.currentquestion + 1} was right!");
                        await bot.SendTextMessageAsync(user.id, "Правильно!");
                        user.currentquestion++;
                        user.points += question.points;
                    }
                    else
                    {
                        log.Info($"Answer '{answer}' from {user.id} on {user.currentlesson.id}/{user.currentquestion + 1} was wrong! Right answer is {question.answer}");
                        await bot.SendTextMessageAsync(user.id, $"Неправильно! Можеш дізнатись рішення у завдання у свого куратора.\n\nПравильна відповідь: {question.answer}");
                        user.mistakes++;
                        user.currentquestion++;
                    }
                    //Check if its last question in test
                    if (user.currentquestion >= user.currentlesson.test.questions.Count)
                    {
                        string prefix = "";
                        switch (Type)
                        {
                            case 0:
                                {
                                    prefix = "A";
                                    break;
                                }
                            case 1:
                                {
                                    prefix = "A";
                                    break;
                                }
                            case 2:
                                {
                                    prefix = "B";
                                    break;
                                }
                            case 3:
                                {
                                    prefix = "C";
                                    break;
                                }
                            case 4:
                                {
                                    prefix = "D";
                                    break;
                                }
                            case 5:
                                {
                                    prefix = "E";
                                    break;
                                }
                            case 6:
                                {
                                    prefix = "F";
                                    break;
                                }
                            case 7:
                                {
                                    prefix = "G";
                                    break;
                                }
                            case 8:
                                {
                                    prefix = "K";
                                    break;
                                }
                            default: break;
                        }

                        if (ExecuteMySql($"UPDATE users SET Stats = CONCAT(Stats, '{prefix}:{user.currentlesson.id}:{user.mistakes}/{user.currentlesson.test.questions.Count}:{user.points};') WHERE ID = {user.id}"))
                        {
                            user.statistic += $"{prefix}:{user.currentlesson.id}:{user.mistakes}/{user.currentlesson.test.questions.Count}:{user.points};";
                            user.ontest = false;
                            user.currentquestion = 0;
                            await bot.SendTextMessageAsync(user.id, $"Вітаю! Ви закінчили тест. Ви набрали {user.points} балів!", replyMarkup: menu);
                            user.points = 0;
                            user.mistakes = 0;
                        }
                        else
                        {
                            Console.WriteLine("Error в конце теста!");
                        }
                    }
                    else
                    {
                        //if(currentlesson.test.questions[user.currentquestion].variants.Length < 0 )
                        user.currentlesson.test.questions[user.currentquestion].Ask(user.id);
                    }
                    SaveState(user);
                }
                else if (commands.Find(c => c.Name == text) == null)
                {
                    await bot.SendTextMessageAsync(user.id, "Я ще не знаю такої команди :)", replyMarkup: menu);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "\n" + ex.StackTrace);
            }
            log.Info($"On message from {e.Message.From.Id} end");
        }

        private static void AddAllCommands()
        {
            log.Info("Adding all commands...");
            commands.Add(new A_RestartTimer());
            //commands.Add(new A_Send());
            //commands.Add(new AskCmd());
            //commands.Add(new HelpCommand());

            //commands.Add(new BalanceCmd());
            commands.Add(new RecordsCmd());
            commands.Add(new CuratorCmd());
            commands.Add(new MyAccountCmd());
            commands.Add(new SheduleCmd());
            commands.Add(new StopCmd());
            commands.Add(new MainManuCmd());
            commands.Add(new AskAgainCmd());
            commands.Add(new MenuCmd());
            //My menu part
            commands.Add(new MyHpCmd());
            commands.Add(new MyMistakesCmd());
            commands.Add(new MyRateCmd());
            commands.Add(new PaymentCmd());
            //End of My menu
            commands.Sort((x, y) => string.Compare(x.Name, y.Name));
            log.Info("Commands added");
        }

        public static User GetCurrentUser(long id)
        {
            return users.Find(u => u.id == id);
        }
        public static User GetCurrentUser(MessageEventArgs e)
        {
            long id = e.Message.Chat.Id;
            return users.Find(u => u.id == id);
        }
        public static User GetCurrentUser(CallbackQueryEventArgs e)
        {
            long id = e.CallbackQuery.From.Id;
            return users.Find(u => u.id == id);
        }

        public static InlineKeyboardMarkup GetInlineKeyboard(string[] array, int column)
        {
            log.Info("Creating inline KB...");
            int steps = (int)Math.Round((double)array.Length / column, MidpointRounding.AwayFromZero);
            var keyboardInline = new InlineKeyboardButton[steps][];
            int count = 0;
            int dif = steps * column - array.Length;

            for (int y = 0; y < steps; y++)
            {
                var keyboardButtons = new InlineKeyboardButton[column];
                if (y == steps - 1)
                    keyboardButtons = new InlineKeyboardButton[column - dif];
                for (int i = 0; i < column; i++)
                {
                    if (count >= array.Length)
                        break;
                    keyboardButtons[i] = new InlineKeyboardButton
                    {
                        Text = array[(y * column) + i],
                        CallbackData = ((y * column) + i + 1).ToString(),
                    };
                    count++;
                }
                keyboardInline[y] = keyboardButtons;
            }

            log.Info("KB created");
            return keyboardInline;
        }

        //Timers part
        public static void UpdateDBTimer()
        {
            log.Info("Update DB timer");
            System.Timers.Timer DBChecker = new System.Timers.Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            DBChecker.AutoReset = true;
            DBChecker.Elapsed += CheckForUpdates;
            DBChecker.Enabled = true;
            log.Info("Update DB timer end");
        }

        public static void HMNotificationTimer()
        {
            log.Info("In HMNotificationTimer");
            //delete last timer
            if (HMTimer != null)
                HMTimer.Dispose();
            HMTimer = null;

            Lesson lesson = GetNextLesson(10);
            if (lesson == null)
            {
                log.Info("No lesson found. HMNotification end");
                return;
            }

            //set new timer
            HMTimer = new Timer(new TimerCallback(HomeworkNotification));
            DateTime temptime = lesson.datetime.AddHours(-10);
            if (temptime > DateTime.Now)
            {
                int msUntilTime = (int)((temptime - DateTime.Now).TotalMilliseconds);
                HMTimer.Change(msUntilTime, Timeout.Infinite);
            }
            log.Info("Tiemr setted up. HMNotificationTimer end");
        }

        public static void WebinarNotificationTimer()
        {
            log.Info("In WebinarNotificationTimer");
            //delete last timer
            if (WebinarTimer != null)
                WebinarTimer.Dispose();
            WebinarTimer = null;

            Lesson lesson = GetNextLesson(2);
            if (lesson == null)
            {
                log.Info("No lesson found. WebinarNotification end");
                return;
            }

            //set new timer
            WebinarTimer = new Timer(new TimerCallback(WebinarNotification));
            DateTime temptime = lesson.datetime.AddHours(-2);

            if (temptime > DateTime.Now)
            {
                int msUntilTime = (int)((temptime - DateTime.Now).TotalMilliseconds);
                WebinarTimer.Change(msUntilTime, Timeout.Infinite);
            }
            log.Info("Timer setted up. WebinarNotificationTimer end");
        }

        public static void LinkSenderTimer()
        {
            log.Info("In LinkSenderTimer");
            //delete last timer
            if (LinkTimer != null)
                LinkTimer.Dispose();
            LinkTimer = null;

            Lesson lesson = GetNextLesson(true, 5);
            if (lesson == null)
            {
                log.Info("No lesson found. LinkSender end");
                return;
            }
            nextlesson = lesson;

            //set new timer
            LinkTimer = new Timer(new TimerCallback(SendWebinarLinks));
            DateTime temptime = lesson.datetime.AddMinutes(-5);
            if (temptime > DateTime.Now)
            {
                int msUntilTime = (int)((temptime - DateTime.Now).TotalMilliseconds);
                LinkTimer.Change(msUntilTime, Timeout.Infinite);
                log.Info($"Ms until next lessson: {msUntilTime}. Next lesson have id: {lesson.id} and time {lesson.datetime}");
            }
            log.Info("Timer setted up. LinkSenderTimer end");
        }

        public static void InitializeTestTimer()
        {
            log.Info("In InitializeTestTimer");
            int hour = 2;
            Lesson lesson = GetNextLesson(hour);
            if (lesson == null)
            {
                log.Info("No lesson found. InitializeTimer end");
                return;
            }

            currentlesson = lesson;

            DateTime TestTime = lesson.datetime.AddHours(hour);
            if (useTimer)
            {
                if (TestTimer != null)
                    TestTimer.Dispose();
                TestTimer = new Timer(new TimerCallback(TestAll));

                // Figure how much time until seted time
                DateTime now = DateTime.Now;

                int msUntilTime = (int)((TestTime - now).TotalMilliseconds);
                // Set the timer to elapse only once, at setted teme.
                TestTimer.Change(msUntilTime, Timeout.Infinite);
            }
            else
            {
                if (TestTimer != null)
                    TestTimer.Dispose();
                TestTimer = null;
            }
            log.Info("Timer setted up. InitializeTestTimer end");
        }

        public static void InitializeStopTimer()
        {
            log.Info("In InitializeStopTimer");
            Lesson lesson = GetNextLesson();
            if (lesson == null)
            {
                log.Info("No lesson found. StopTimer end");
                return;
            }

            DateTime TestTime = lesson.datetime;

            //delete last timer
            if (StopTimer != null)
                StopTimer.Dispose();
            StopTimer = null;
            //get next webinar datetime

            //set new timer
            StopTimer = new Timer(new TimerCallback(StopTest));
            int msUntilTime = (int)((TestTime - DateTime.Now).TotalMilliseconds);
            StopTimer.Change(msUntilTime, Timeout.Infinite);
            log.Info("Timer setted up. InitializeStopTimer end");
        }

        private async static void HomeworkNotification(object state)
        {
            log.Info("Homework notification");
            try
            {
                for (int i = 0; i < users.Count; i++)
                {
                    try
                    {
                        if (users[i].groups.Count == 0)
                            continue;
                        if (users[i].groups.Contains(currentlesson.group.id) && users[i].ontest)
                            await bot.SendTextMessageAsync(users[i].id, "Нагадую, що тобі необхідно виконати домашнє завдання! В тебе ще 10 годин!");
                    }
                    catch (Exception exception)
                    {
                        log.Error($"There was exception for {users[i].id} with msg: {exception.Message}");
                    }
                }
                HMNotificationTimer();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "\n" + ex.StackTrace);
            }
            log.Info("Homework notification end");
        }

        private async static void WebinarNotification(object state)
        {
            log.Info("Webinar notification");
            try
            {
                for (int i = 0; i < users.Count; i++)
                {
                    try
                    {
                        if (users[i].groups.Count == 0)
                            continue;
                        if (users[i].groups.Contains(currentlesson.group.id))
                            await bot.SendTextMessageAsync(users[i].id, "Нагадую, що через 2 години вебінар!");
                    }
                    catch (Exception exception)
                    {
                        log.Error($"There was exception for {users[i].id} with msg: {exception.Message}");
                    }
                }
                HMNotificationTimer();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "\n" + ex.StackTrace);
            }
            WebinarNotificationTimer();
            log.Info("Webinar notification end");
        }

        public async static void SendWebinarLinks(object state)
        {
            log.Info("Sending webinar links...");
            try
            {
                if (currentlesson.tokens.Length == 0 || currentlesson.link.Length == 0)
                    return;
                int y = 0;
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].groups.Count == 0)
                        continue;
                    if (users[i].groups.Contains(nextlesson.group.id) && users[i].health[Type - 1] > 0 && users[i].subjects.Contains(Type + ";"))
                    {
                        log.Info($"Sending link for {users[i].id}");
                        try
                        {
                            await bot.SendTextMessageAsync(users[i].id, "Привіт!\n\n" +
                                    $"👉 Посилання на сьогоднішнє заняття: {nextlesson.link + nextlesson.tokens[y + 3]}\n" +
                                    "⏱ Чекаю тебе через 5 хвилин!");
                            y++;
                            if (y + 3 >= nextlesson.tokens.Length)
                                return;
                        }
                        catch (Exception exception)
                        {
                            log.Info($"There was exception for {users[i].id} with msg: {exception.Message} {exception.StackTrace}");
                        }
                    }
                    else 
                    {
                        log.Info($"{users[i].id} is flooping away. Users grops contain current lesson group({currentlesson.group.id}) = {users[i].groups.Contains(currentlesson.group.id)}\n"+
                            $"  User health is more than 0({users[i].health}) = {users[i].health[Type - 1] > 0}\n"+
                            $"  User subjects contain current subj({users[i].subjects}) = {users[i].subjects.Contains(Type + ";")}"
                            );
                    }
                }
                LinkSenderTimer();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "\n" + ex.StackTrace);
            }
            log.Info("Webinar links sended");
        }

        public static Lesson GetNextLesson()
        {
            log.Info("Get next lesson");
            List<Lesson> shedule = new List<Lesson>(lessons);
            foreach (Lesson w in lessons)
            {
                if (w.datetime < DateTime.Now)
                    shedule.Remove(w);
            }
            shedule = shedule.OrderBy(x => x.datetime).ToList();
            //Find nearest webinar for current subject
            Lesson lesson = new Lesson();
            for (int i = 0; i < shedule.Count; i++)
            {
                if (shedule[i].datetime > DateTime.Now)
                {
                    lesson = shedule[i];
                    log.Info($"Next lesson is {lesson.id}");
                    return lesson;
                }
            }

            if (shedule.Count == 0)
                lesson = null;
            else
                lesson = shedule[0];

            log.Info($"Next lesson is {lesson.id}");
            log.Info("Get next lesson end");
            return lesson;
        }
        private static Lesson GetNextLesson(int hour)
        {
            log.Info("Get next lesson(hour)");
            List<Lesson> shedule = new List<Lesson>(lessons);
            foreach (Lesson w in lessons)
            {
                if (w.datetime.AddHours(hour) < DateTime.Now)
                    shedule.Remove(w);
            }
            shedule = shedule.OrderBy(x => x.datetime).ToList();

            //Find nearest webinar for current subject
            Lesson lesson = new Lesson();
            for (int i = 0; i < shedule.Count; i++)
            {
                if (shedule[i].datetime.AddHours(hour) > DateTime.Now)
                {
                    lesson = shedule[i];
                    log.Info($"Next lesson is {lesson.id}");
                    return lesson;
                }
            }
            if (shedule.Count == 0)
                lesson = null;
            else
                lesson = shedule[0];
            log.Info("Get next lesson(hour) end");
            return lesson;
        }
        private static Lesson GetNextLesson(bool minutes, int hour)
        {
            log.Info("Get next lesson(min,hour)");
            List<Lesson> shedule = new List<Lesson>(lessons);
            foreach (Lesson w in lessons)
            {
                if (w.datetime.AddMinutes(hour) < DateTime.Now)
                    shedule.Remove(w);
            }
            shedule = shedule.OrderBy(x => x.datetime).ToList();

            //Find nearest webinar for current subject
            Lesson lesson = new Lesson();
            for (int i = 0; i < shedule.Count; i++)
            {
                if (shedule[i].datetime.AddMinutes(hour) > DateTime.Now)
                {
                    lesson = shedule[i];
                    log.Info($"Next lesson is {lesson.id}");
                    return lesson;
                }
            }
            if (shedule.Count == 0)
                lesson = null;
            else
                lesson = shedule[0];
            log.Info("Get next lesson(min,hour) end");
            return lesson;
        }

        //Tests part
        public async static void TestAll(object state)
        {
            log.Info("Testing all...");
            try
            {
                //add test to DB
                for (int i = 0; i < users.Count; i++)
                {
                    User u = users[i];
                    try
                    {
                        if (u.groups.Exists(x => x == currentlesson.group.id) && u.subjects.Contains(Type.ToString() + ";") && (u.health[Type - 1] > 0 || u.health[Type - 1] == -7))
                        {
                            u.currentlesson = currentlesson;
                            u.mistakes = 0;
                            u.points = 0;
                            u.ontest = true;
                            u.currentquestion = 0;
                            await Program.bot.SendTextMessageAsync(u.id, Program.currentlesson.test.Text, replyMarkup: menutest);
                            Program.currentlesson.test.questions[0].Ask(u.id);
                            SaveState(u);
                        }
                        else 
                        {
                            log.Info($"User {u.id} dont match:\n    user have group - {u.groups.Exists(x => x == currentlesson.group.id)}\n    user have hp - {(u.health[Type - 1] > 0 || u.health[Type - 1] == -7)}\n   user have subject - {u.subjects.Contains(Type.ToString() + ";")}");
                        }
                    }
                    catch (Exception exception)
                    {
                        log.Error($"There was exception in TestAll for {u.id} with msg: {exception.Message}");
                    }
                }
                //SaveState();
                //Timer until next webinar
                InitializeStopTimer();
                InitializeTestTimer();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "\n" + ex.StackTrace);
            }
            log.Info("All tests sended");
        }

        public async static void StopTest(object state)
        {
            log.Info("Stopping tests");
            try
            {
                for (int i = 0; i < users.Count; i++)
                {
                    User u = users[i];
                    try
                    {
                        if (u.groups.Count == 0)
                            continue;
                        if (u.groups.Exists(x => x == currentlesson.group.id) && u.subjects.Contains(Type.ToString() + ";") && u.ontest)
                        {
                            if (u.health[Type - 1] == -7)
                            {
                                u.ontest = false;
                                u.currentquestion = 0;
                                SaveState(u);
                                await Program.bot.SendTextMessageAsync(u.id, $"Час на виконання домашнього завдання закінчився!", replyMarkup: menu);
                            }
                            else
                            {
                                u.ontest = false;
                                u.currentquestion = 0;
                                u.health[Type - 1]--;
                                SaveState(u);
                                if (u.health[Type - 1] <= 0)
                                {
                                    await Program.bot.SendTextMessageAsync(u.id, $"Ви не виконали домашнє завдання! На жаль, ви втрачаєте життя.\nНа жаль у вас закінчились усі життя і ви вилітаєте з нашої програми.", replyMarkup: menu);
                                    //u.subscriber[Type - 1] = 0;
                                    ExecuteMySql($"UPDATE users SET health = '{String.Join(";", u.health)}' WHERE id = {u.id}");
                                }
                                else
                                {
                                    await Program.bot.SendTextMessageAsync(u.id, $"Ви не виконали домашнє завдання! На жаль, ви втрачаєте життя. Тепер у вас {u.health[Type - 1]} життя.", replyMarkup: menu);
                                    ExecuteMySql($"UPDATE users SET health = '{String.Join(";", u.health)}' WHERE id = {u.id}");
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        log.Error($"There was exception in Stoptest for {u.id} with msg: {exception.Message}");
                    }
                }
                //u.GetNextWebinar();
                InitializeStopTimer();
            }
            catch (Exception e)
            {
                log.Error(e.Message + "\n" + e.StackTrace);
            }
            log.Info("Tests stopped");
        }

        // Database part
        public static void LoadFromDB()
        {
            log.Info("Loading from database...");
            MySqlConnection con = new MySqlConnection(connectionString);
            try
            {
                con.Open();

                //Load Questions
                string command = "SELECT * FROM questions";
                MySqlCommand cmd = new MySqlCommand(command, con);
                
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    switch (reader.GetInt32("type"))
                    {
                        case 1:
                            questions.Add(new TestQuestion(
                                reader.GetInt32("id"),
                                reader.GetString("text"),
                                reader.GetInt32("points"),
                                reader.GetString("variants").Split(";", StringSplitOptions.RemoveEmptyEntries),
                                reader.GetInt32("columns"),
                                reader.GetString("answer"),
                                reader.GetString("image")
                                )); break;
                        case 2:
                            questions.Add(new FreeQuestion(
                                reader.GetInt32("id"),
                                reader.GetString("text"),
                                reader.GetInt32("points"),
                                reader.GetString("answer"),
                                reader.GetString("image")
                                )); break;
                        case 3:
                            questions.Add(new ConformityQuestion(
                                reader.GetInt32("id"),
                                reader.GetString("text"),
                                reader.GetInt32("points"),
                                reader.GetString("answer"),
                                reader.GetString("image")
                                )); break;
                        case 4:
                            questions.Add(new MultipleQuestion(
                                reader.GetInt32("id"),
                                reader.GetString("text"),
                                reader.GetString("image"),
                                reader.GetInt32("points"),
                                reader.GetString("answer")
                                )); break;
                        default: break;
                    }
                }
                reader.Close();
                reader.Dispose();
                //Question end--

                //Load groups
                command = $"SELECT * FROM groups";
                cmd = new MySqlCommand(command, con);

                reader = cmd.ExecuteReader();
                string curator = "";

                while (reader.Read())
                {
                    if (reader.GetString("Curator") != null)
                        curator = reader.GetString("Curator");
                    groups.Add(new Group(reader.GetInt32("id"), reader.GetString("Name"), curator, reader.GetString("Link"), reader.GetInt32("Type")));
                }
                reader.Close();
                reader.Dispose();
                //group end --

                //Load Lessons
                command = "SELECT * FROM lessons";
                cmd = new MySqlCommand(command, con);

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (groups.Find(x => x.id == reader.GetInt32("Group")).type == Program.Type)
                    {
                        string[] ids = reader.GetString("questions").Replace(" ", "").Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                        List<Question> q = new List<Question>();
                        for (int i = 0; i < ids.Length; i++)
                        {
                            q.Add(questions.Find(x => x.id == Int32.Parse(ids[i])));
                        }

                        Lesson lesson = new Lesson(reader.GetInt32("id"), reader.GetString("Name"), DateTime.Parse(reader.GetString("DateTime")), reader.GetInt32("Group"), new Test(reader.GetString("rule"), q), reader.GetString("Link"), reader.GetString("Tokens"));
                        lessons.Add(lesson);
                    }
                }
                reader.Close();
                reader.Dispose();
                //Lessons end--

                //Load Users
                command = "SELECT * FROM users";
                cmd = new MySqlCommand(command, con);

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User(
                        reader.GetInt32("ID"),
                        reader.GetString("Phone"),
                        reader.GetString("Name") + " " + reader.GetString("Soname"),
                        reader.GetString("Username"),
                        reader.GetString("Health"),
                        reader.GetInt32("Coins"),
                        reader.GetString("Group"),
                        reader.GetString("Curator"),
                        reader.GetString("Subjects"),
                        reader.GetString("Stats"),
                        reader.GetString("State")
                        ));
                }
                reader.Close();
                reader.Dispose();
                //users end--

                con.Close();
                //con.Dispose();
            }
            catch (Exception ex)
            {
                con.Close();
                //con.Dispose();
                log.Error(ex.Message + "\n" + ex.StackTrace);
                Thread.Sleep(10000);
                LoadFromDB();
            }
            log.Info("Loading from database completed");
        }

        public static void CheckForUpdates(Object source, System.Timers.ElapsedEventArgs e)
        {
            log.Info("Checking for updates...");
            MySqlConnection con = new MySqlConnection(connectionString);
            try
            {
                con.Open();
                string command = "SELECT * FROM updates";
                MySqlCommand cmd = new MySqlCommand(command, con);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    switch (reader.GetString("Name"))
                    {
                        case "users":
                            if (users_update < reader.GetDateTime("LastUpdate"))
                            {
                                if (UpdateUsers())
                                    users_update = reader.GetDateTime("LastUpdate");
                            }
                            break;
                        case "lessons":
                            if (lessons_update < reader.GetDateTime("LastUpdate"))
                            {
                                if (UpdateLessons())
                                    lessons_update = reader.GetDateTime("LastUpdate");
                            }
                            break;
                        case "groups":
                            if (groups_update < reader.GetDateTime("LastUpdate"))
                            {
                                if(UpdateGroups())
                                    groups_update = reader.GetDateTime("LastUpdate");
                            }
                            break;
                        default: break;
                    }
                }
                con.Close();
                //con.Dispose();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Виникла помилка при отримані оновлень з бази даних");
                Console.WriteLine(exception.Message);
                con.Close();
                log.Error($"Error in checkupdate: {exception.Message}\n{exception.StackTrace}");
                //con.Dispose();
            }
            log.Info("Updates checked");
        }

        public static bool UpdateUsers()
        {
            log.Info("Updating users...");
            MySqlConnection con = new MySqlConnection(connectionString);
            try
            {
                con.Open();
                //Load Users
                string command = "SELECT * FROM users";
                MySqlCommand cmd = new MySqlCommand(command, con);
                List<long> ids = new List<long>();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ids.Add(reader.GetInt32("id"));
                    long id = reader.GetInt32("id");
                    if (users.Find(x => x.id == id) != default(User))
                    {
                        int index = users.FindIndex(x => x.id == id);
                        users[index] = new User(
                            reader.GetInt32("ID"),
                            reader.GetString("Phone"),
                            reader.GetString("Name") + " " + reader.GetString("Soname"),
                            reader.GetString("Username"),
                            reader.GetString("Health"),
                            reader.GetInt32("Coins"),
                            reader.GetString("Group"),
                            reader.GetString("Curator"),
                            reader.GetString("Subjects"),
                            reader.GetString("Stats"),
                            reader.GetString("State"),
                            users[index].lastmsg
                            );
                        /*
                        user.health = reader.GetString("Health").Split(";", StringSplitOptions.RemoveEmptyEntries);
                        user.coins = reader.GetInt32("Coins");
                        user.groups = reader.GetString("Group");
                        user.curator = reader.GetString("Curator");
                        user.subjects = reader.GetString("Subjects");
                        user.statistic = reader.GetString("Stats");1
                        */
                    }
                    else
                    {
                        users.Add(new User(
                            reader.GetInt32("ID"),
                            reader.GetString("Phone"),
                            reader.GetString("Name") + " " + reader.GetString("Soname"),
                            reader.GetString("Username"),
                            reader.GetString("Health"),
                            reader.GetInt32("Coins"),
                            reader.GetString("Group"),
                            reader.GetString("Curator"),
                            reader.GetString("Subjects"),
                            reader.GetString("Stats"),
                            reader.GetString("State")
                        ));
                    }
                }
                reader.Close();
                reader.Dispose();

                con.Close();
                //con.Dispose();
                for (int i = 0; i < users.Count; i++)
                {
                    if (!ids.Exists(x => x == users[i].id))
                    {
                        users.RemoveAt(i);
                    }
                }
                log.Info("Users updated");
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Виникла помилка при оновлені користувачів");
                Console.WriteLine(exception.Message);
                con.Close();
                //con.Dispose();
                log.Error(exception.Message + "\n" + exception.StackTrace);
                log.Info("Users not updated");
                return false;
            }
        }

        public static bool UpdateLessons()
        {
            log.Info("Updating lessons...");
            MySqlConnection con = new MySqlConnection(connectionString);
            try
            {
                con.Open();

                //Load Questions
                string command = "SELECT * FROM questions";
                MySqlCommand cmd = new MySqlCommand(command, con);

                MySqlDataReader reader = cmd.ExecuteReader();
                questions.Clear();
                while (reader.Read())
                {
                    switch (reader.GetInt32("type"))
                    {
                        case 1:
                            questions.Add(new TestQuestion(
                                reader.GetInt32("id"),
                                reader.GetString("text"),
                                reader.GetInt32("points"),
                                reader.GetString("variants").Split(";", StringSplitOptions.RemoveEmptyEntries),
                                reader.GetInt32("columns"),
                                reader.GetString("answer"),
                                reader.GetString("image")
                                )); break;
                        case 2:
                            questions.Add(new FreeQuestion(
                                reader.GetInt32("id"),
                                reader.GetString("text"),
                                reader.GetInt32("points"),
                                reader.GetString("answer"),
                                reader.GetString("image")
                                )); break;
                        case 3:
                            questions.Add(new ConformityQuestion(
                                reader.GetInt32("id"),
                                reader.GetString("text"),
                                reader.GetInt32("points"),
                                reader.GetString("answer"),
                                reader.GetString("image")
                                )); break;
                        case 4:
                            questions.Add(new MultipleQuestion(
                                reader.GetInt32("id"),
                                reader.GetString("text"),
                                reader.GetString("image"),
                                reader.GetInt32("points"),
                                reader.GetString("answer")
                                )); break;
                        default: break;
                    }

                }
                reader.Close();
                reader.Dispose();

                //Load Lessons
                command = "SELECT * FROM lessons";
                cmd = new MySqlCommand(command, con);

                lessons.Clear();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (groups.Find(x => x.id == reader.GetInt32("Group")).type == Program.Type)
                    {
                        string[] ids = reader.GetString("questions").Replace(" ", "").Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                        List<Question> q = new List<Question>();
                        for (int i = 0; i < ids.Length; i++)
                        {
                            q.Add(questions.Find(x => x.id == Int32.Parse(ids[i])));
                        }

                        Lesson lesson = new Lesson(reader.GetInt32("id"), reader.GetString("Name"), DateTime.Parse(reader.GetString("DateTime")), reader.GetInt32("Group"), new Test(reader.GetString("rule"), q), reader.GetString("Link"), reader.GetString("Tokens"));
                        lessons.Add(lesson);
                    }
                }
                reader.Close();
                reader.Dispose();

                con.Close();
                //con.Dispose();
                HMNotificationTimer();
                WebinarNotificationTimer();
                InitializeTestTimer();
                InitializeStopTimer();
                LinkSenderTimer();
                log.Info("Lessons updated");
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Виникла помилка при оновлені уроків");
                Console.WriteLine(exception.Message);
                con.Close();
                //con.Dispose();
                log.Error(exception.Message + "\n" + exception.StackTrace);
                log.Info("Lessons not updated");
                return false;
            }
        }

        public static bool UpdateGroups()
        {
            log.Info("Updating groups..");
            MySqlConnection con = new MySqlConnection(connectionString);
            try
            {
                con.Open();

                string command = $"SELECT * FROM groups";
                MySqlCommand cmd = new MySqlCommand(command, con);

                MySqlDataReader reader = cmd.ExecuteReader();
                groups.Clear();
                while (reader.Read())
                {
                    groups.Add(new Group(reader.GetInt32("id"), reader.GetString("Name"), reader.GetString("Curator"), reader.GetString("Link"), reader.GetInt32("Type")));
                }
                reader.Close();
                reader.Dispose();

                con.Close();
                //con.Dispose();
                log.Info("Groups updated");
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Виникла помилка при оновлені груп");
                Console.WriteLine(exception.Message);
                con.Close();
                //con.Dispose();
                log.Error(exception.Message +"\n"+ exception.StackTrace);
                log.Info("Groups not updated");
                return false;
            }
        }

        public static bool ExecuteMySql(string command)
        {
            log.Info("In executing mysql");
            MySqlConnection con = new MySqlConnection(connectionString);
            try
            {
                MySqlCommand cmd = new MySqlCommand(command, con);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                //con.Dispose();
                log.Info("Executing mysql end");
                return true;
            }
            catch (Exception exception)
            {
                con.Close();
                log.Error(exception.Message +"\n"+ exception.StackTrace);
                Console.WriteLine($"Помилка у виконанні команди до Бази данних. Текст помилки:\n{exception.Message}");
                return false;
            }
        }

        public struct Group
        {
            public int id;
            public string name;
            public string curator;
            public string link;
            public int type;

            public Group(int id, string name, string curator, string link, int type)
            {
                this.id = id;
                this.name = name;
                this.curator = curator;
                this.link = link;
                this.type = type;
            }
        }
        public enum SubjectType : int
        {
            Nothing = 0,
            Ukrainian = 1,
            Math = 2,
            Biology = 3,
            History = 4,
            Geography = 5,
            English = 6
        }
    }
}
