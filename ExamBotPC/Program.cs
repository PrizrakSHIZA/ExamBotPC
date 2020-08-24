using ExamBotPC.Commands;
using ExamBotPC.Tests;
using ExamBotPC.Tests.Questions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using ExamBotPC.UserSystem;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.Enums;
using System.Linq.Expressions;
using System.Configuration;
using Renci.SshNet.Security;

namespace ExamBotPC
{
    class Program
    {
        public static TelegramBotClient bot;
        public static ReplyKeyboardMarkup menu = new ReplyKeyboardMarkup(), menu2 = new ReplyKeyboardMarkup();
        public static List<Command> commands = new List<Command>();
        public static List<User> users = new List<User>();
        public static List<Question> questions = new List<Question>();
        public static List<Lesson> lessons = new List<Lesson>();
        public static List<Group> groups = new List<Group>();
        public static Lesson currentlesson = new Lesson();
        public static string connectionString = new MySqlConnectionStringBuilder()
        {
            Server = APIKeys.DBServer,
            Database = APIKeys.DBName,
            UserID = APIKeys.DBUser,
            Password = APIKeys.DBPassword,
            CharacterSet = "utf8",
            ConvertZeroDateTime = true
        }.ConnectionString;
        public static string password = APIKeys.password;
        public static bool useTimer = true;
        public static char[] delimiterChars = { ',', '.', '\t', '\n', ';' };
        public static int Type;

        static DateTime users_update, lessons_update, groups_update; 
        static Timer TestTimer, StopTimer, HMTimer, WebinarTimer, LinkTimer;

        static void Main(string[] args)
        {
            Console.WriteLine("Enter command:");
            while (true)
            {
                Console.Write("=>");
                string cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "help": Console.WriteLine("list - show all types of bot\nstart - start bot\nquit - close bot");  break;
                    case "list": Console.WriteLine("0 - localhost\n1 - Ukrainian\n2 - Math\n3 - Biology");  break;
                    case "start":
                        Console.WriteLine("Enter type number of bot:");
                        string x = Console.ReadLine();
                        switch (x)
                        {
                            case "0":
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
                                Type = (int)SubjectType.Ukrainian;
                                StartBot(APIKeys.UkrBotApi);
                                break;
                            case "2":
                                Type = (int)SubjectType.Math;
                                StartBot(APIKeys.MathBotApi);
                                break;
                            case "3":
                                Type = (int)SubjectType.Boilogy;
                                StartBot(APIKeys.BioBotApi);
                                break;

                            default: Console.WriteLine("Wrong number!"); break;
                        }
                        break;
                    case "msg":
                        {
                            foreach (User u in users)
                            {
                                if (u.subjects.Contains(Type + ";"))
                                {
                                    bot.SendTextMessageAsync(u.id, "Ку! У нас тут технічні роботи 👀\n" +
                                                                    "Вони потрібні для того, щоб бот гарно працював та не глючив.\n\n"+
                                                                    "Наш технічний супер - майстер вже скоро завершить оновлення. Будь ласка, поки що нічого сюди не пиши, бо повідомлення може бути загублено.\n\n"+
                                                                    "Ми дамо знати як тільки бот запрацює знову.\n"+
                                                                    "Дякуємо за розуміння! 🥰");
                                }
                            }
                            break;
                        }
                    case "quit":
                        {
                            foreach (User u in users)
                            {
                                if (u.subscriber[Type - 1] == "1")
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
                                    string state = $"{prefix}{(u.ontest ? 1 : 0)};{u.currentlesson.id};{u.currentquestion};{u.points};{u.mistakes}";
                                    ExecuteMySql($"UPDATE users SET State = REPLACE(State, '{u.state[Type - 1]}', '{state}') WHERE id = {u.id}");
                                }
                            }
                            Environment.Exit(0);
                            break;
                        }
                    default: Console.WriteLine("No such command found");  break;
                }
            }
        }

        private static void StartBot(string Api)
        {
            //Loading data
            LoadFromDB();

            users_update = lessons_update = groups_update = DateTime.Now;
            //Add all commands
            AddAllCommands();
            //Initialize timers
            UpdateDBTimer();
            HMNotificationTimer();
            WebinarNotificationTimer();
            InitializeTestTimer();
            LinkSenderTimer();

            //Initialize bot client
            bot = new TelegramBotClient(Api) { Timeout = TimeSpan.FromSeconds(10) };

            //Starting message
            var me = bot.GetMeAsync().Result;
            Console.WriteLine($"Its me, {me}!");

            bot.StartReceiving();
            bot.OnMessage += Bot_OnMessage;
            bot.OnCallbackQuery += Bot_OnCallbackQuery;
            CreateMenu();
            foreach (User u in users)
            {
                try 
                { 
                    if (u.subjects.Contains(Program.Type + ";"))
                    {
                        if (u.ontest)
                            bot.SendTextMessageAsync(u.id, "Повтори конспект поки чекаєш на наступний урок 😉", replyMarkup: menu2);
                        else
                            bot.SendTextMessageAsync(u.id, "Повтори конспект поки чекаєш на наступний урок 😉", replyMarkup: menu);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"There was exception for {u.id} with msg: {exception.Message}");
                }
            }
        }

        private static void CreateMenu()
        {
            menu.Keyboard = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Записи уроків ▶"),
                    new KeyboardButton("Допомога 💬")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Розклад 📅"),
                    new KeyboardButton("Моя статистика 📊")
                },
            };
            menu.ResizeKeyboard = true;
            menu.OneTimeKeyboard = false;

            menu2.Keyboard = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Записи уроків ▶"),
                    new KeyboardButton("Допомога 💬")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Розклад 📅"),
                    new KeyboardButton("Моя статистика 📊")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Повторити запитання з тесту 🔄"),
                },
            };
            menu2.ResizeKeyboard = true;
            menu2.OneTimeKeyboard = false;
        }

        private async static void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            User user = GetCurrentUser(e);
            //if user is on test
            if (user.ontest)
            {
                Question question = user.currentlesson.test.questions[user.currentquestion];
                //Check answer is right or wrong
                if (e.CallbackQuery.Data == question.answer)
                {
                    await bot.SendTextMessageAsync(user.id, "Правильно!");
                    user.currentquestion++;
                    user.points += question.points;
                }
                else
                {
                    await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.variants[Int32.Parse(question.answer) - 1]}");
                    user.mistakes++;
                    user.currentquestion++;
                }
                //Check if its last question in test
                if (user.currentquestion >= user.currentlesson.test.questions.Count)
                {
                    if (ExecuteMySql($"UPDATE users SET Stats = CONCAT(Stats, '{user.currentlesson.id}:{user.mistakes}/{user.currentlesson.test.questions.Count}:{user.points};') WHERE ID = {user.id}"))
                    {
                        user.statistic += $"{user.currentlesson.id}:{user.mistakes}/{user.currentlesson.test.questions.Count}:{user.points};";
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
        }

        private async static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            User user;

            //check if user is subscriber
            if (users.Find(x => x.id == e.Message.Chat.Id) != default(User))
            {
                user = GetCurrentUser(e);
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
                    }
                }
            }
            else
            {
                if (ExecuteMySql($"INSERT INTO users (ID, Name, Soname, Date, Subjects, Subscriber, Health, Curator, State) VALUES ({e.Message.Chat.Id}, '{e.Message.Chat.FirstName}', '{e.Message.Chat.LastName}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', '{Type};', '0;0;0;0;0;0;0;0', '5;5;5;5;5;5;5;5', '0', 'A0;0|B0;0|C0;0|D0;0|E0;0|F0;0|G0;0|K0;0')"))
                {
                    users.Add(new User(e.Message.Chat.Id, e.Message.Chat.FirstName + " " + e.Message.Chat.LastName, "0;0;0;0;0;0;0;0", "5;5;5;5;5;5;5;5", 0, 0, "0", $"{Type};", "", "A0;0|B0;0|C0;0|D0;0|E0;0|F0;0|G0;0|K0;0"));
                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Привіт!\n\n"+
                                            "💪 <b>Вітаю в POWER - групі!</b> 💪\n\n" +
                                            "Це бот, який буде повідомляти про:\n\n"+
                                            "- Трансляції\n"+
                                            "- Новини щодо груп та розкладу\n"+
                                            "- Домашки\n"+
                                            "- Просто круті штуки: 😽\n\n" +
                                            "<b>Скоро куратор приєднає тебе до групки та сюди почнуть приходити твої уроки.</b> Посилання на урок приходить за 5 хвилин до початку.\n\n" +
                                            "Не забувай робити домашні завдання, адже у тебе усього 5 життів на місяць 🤓\n"+
                                            "Побачимося на трансляціях! 👋🥰🚀", replyMarkup: menu, parseMode: ParseMode.Html);
                }
                return;
            }

            //Add user in temp var
            user = GetCurrentUser(e);

            var text = e?.Message?.Text;
            if (text == null) return;
            //Check commands
            else if(commands.Find(c => c.Name == text) != null)
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
                        await bot.SendTextMessageAsync(user.id, "У вашій відповіді була помічена латиниця. Будь ласка, використовуйте лише кирилицю! Повторіть вашу відповідь кирилицею.");
                    }
                    else
                    {
                        //Delete spaces
                        if (q.IsRight(answer))
                        {
                            await bot.SendTextMessageAsync(user.id, "Правильно!");
                            user.currentquestion++;
                            user.points += question.points;
                        }
                        else
                        {
                            await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.answer}");
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
                        await bot.SendTextMessageAsync(user.id, "Правильно!");
                        user.currentquestion++;
                        user.points += question.points;
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.answer}");
                        user.mistakes++;
                        user.currentquestion++;
                    }
                }
                //Check other type
                else if (user.currentlesson.test.questions[user.currentquestion] is TestQuestion)
                {
                    if (answer.ToLower() == question.variants[Int32.Parse(question.answer) - 1])
                    {
                        await bot.SendTextMessageAsync(user.id, "Правильно!");
                        user.currentquestion++;
                        user.points += question.points;
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.variants[Int32.Parse(question.answer) - 1]}");
                        user.mistakes++;
                        user.currentquestion++;
                    }
                }
                else if (answer.ToLower() == question.answer.ToLower())
                {
                    await bot.SendTextMessageAsync(user.id, "Правильно!");
                    user.currentquestion++;
                    user.points += question.points;
                }
                else
                {
                    await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.answer}");
                    user.mistakes++;
                    user.currentquestion++;
                }
                //Check if its last question in test
                if (user.currentquestion >= user.currentlesson.test.questions.Count)
                {
                    if (ExecuteMySql($"UPDATE users SET Stats = CONCAT(Stats, '{user.currentlesson.id}:{user.mistakes}/{user.currentlesson.test.questions.Count}:{user.points};') WHERE ID = {user.id}"))
                    {
                        user.statistic += $"{user.currentlesson.id}:{user.mistakes}/{user.currentlesson.test.questions.Count}:{user.points};";
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
        }

        private static void AddAllCommands()
        {
            commands.Add(new A_RestartTimer());
            //commands.Add(new A_Send());
            //commands.Add(new AskCmd());
            //commands.Add(new HelpCommand());

            //commands.Add(new BalanceCmd());
            commands.Add(new RecordsCmd());
            commands.Add(new CuratorCmd());
            commands.Add(new StatsMenuCmd());
            commands.Add(new SheduleCmd());
            commands.Add(new StopCmd());
            commands.Add(new MainManuCmd());
            commands.Add(new AskAgainCmd());
            //My menu part
            commands.Add(new MyHpCmd());
            commands.Add(new MyMistakesCmd());
            commands.Add(new MyRateCmd());
            //End of My menu
            commands.Sort((x, y) => string.Compare(x.Name, y.Name));
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

            return keyboardInline;
        }

        //Timers part
        public static void UpdateDBTimer()
        {
            System.Timers.Timer DBChecker = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            DBChecker.AutoReset = true;
            DBChecker.Elapsed += CheckForUpdates;
            DBChecker.Enabled = true;
        }

        public static void HMNotificationTimer()
        {
            //delete last timer
            if (HMTimer != null)
                HMTimer.Dispose();
            HMTimer = null;

            Lesson lesson = GetNextLesson();
            if (lesson == null) return;

            //set new timer
            HMTimer = new Timer(new TimerCallback(HomeworkNotification));
            DateTime temptime = lesson.datetime.AddHours(-10);
            if (temptime > DateTime.Now)
            {
                int msUntilTime = (int)((temptime - DateTime.Now).TotalMilliseconds);
                HMTimer.Change(msUntilTime, Timeout.Infinite);
            }
        }

        public static void WebinarNotificationTimer()
        {
            //delete last timer
            if (WebinarTimer != null)
                WebinarTimer.Dispose();
            WebinarTimer = null;

            Lesson lesson = GetNextLesson();
            if (lesson == null) return;

            //set new timer
            WebinarTimer = new Timer(new TimerCallback(WebinarNotification));
            DateTime temptime = lesson.datetime.AddHours(-2);

            if (temptime > DateTime.Now)
            {
                int msUntilTime = (int)((temptime - DateTime.Now).TotalMilliseconds);
                WebinarTimer.Change(msUntilTime, Timeout.Infinite);
            }
        }

        public static void LinkSenderTimer()
        {
            //delete last timer
            if (LinkTimer != null)
                LinkTimer.Dispose();
            LinkTimer = null;

            Lesson lesson = GetNextLesson();
            if (lesson == null) return;

            //set new timer
            LinkTimer = new Timer(new TimerCallback(SendWebinarLinks));
            DateTime temptime = lesson.datetime.AddMinutes(-5);
            if (temptime > DateTime.Now)
            {
                int msUntilTime = (int)((temptime - DateTime.Now).TotalMilliseconds);
                LinkTimer.Change(msUntilTime, Timeout.Infinite);
            }
        }

        public static void InitializeTestTimer()
        {
            int hour = 2;
            Lesson lesson = GetNextLesson(hour);
            if (lesson == null) return;

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
        }
        
        public static void InitializeStopTimer()
        {
            Lesson lesson = GetNextLesson(true, -1);
            if (lesson == null) return;

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
        }

        private async static void HomeworkNotification(object state)
        {
            foreach (User u in users)
            {
                try
                {
                    if (u.group == 0)
                        break;
                    if (currentlesson.group == u.group)
                        await bot.SendTextMessageAsync(u.id, "Нагудую, що тобі необхідно виконати домашнє завдання! В тебе ще 10 годин!");
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"There was exception for {u.id} with msg: {exception.Message}");
                }
            }
            HMNotificationTimer();
        }

        private async static void WebinarNotification(object state)
        {
            foreach (User u in users)
            {
                try
                {
                    if (u.group == 0)
                        break;
                    if (currentlesson.group == u.group)
                        await bot.SendTextMessageAsync(u.id, "Нагудую, що через 2 години вебінар!");
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"There was exception for {u.id} with msg: {exception.Message}");
                }
            }
            HMNotificationTimer();
        }

        public async static void SendWebinarLinks(object state)
        {
            if (currentlesson.tokens.Length == 0 || currentlesson.link.Length == 0)
                return;
            int i = 0;
            foreach (User u in users)
            {
                
                if (u.group == 0)
                    break;
                if (currentlesson.group == u.group && u.subscriber[Type - 1] == "1" && u.subjects.Contains(Type +";"))
                {
                    try
                    {
                        await bot.SendTextMessageAsync(u.id, "Привіт!\n\n" +
                                $"👉 Посилання на сьогоднішнє заняття: {currentlesson.link + currentlesson.tokens[i]}\n" +
                                "⏱ Чекаю тебе через 5 хвилин!");
                        i++;
                        if (i >= currentlesson.tokens.Length)
                            return;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine($"There was exception for {u.id} with msg: {exception.Message}");
                    }
                }
            }
            LinkSenderTimer();
        }

        public static Lesson GetNextLesson()
        {
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
                    return lesson;
                }
            }
            if (shedule.Count == 0)
                lesson = null;
            else
                lesson = shedule[0];
            return lesson;
        }
        private static Lesson GetNextLesson(int hour)
        {
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
                    return lesson;
                }
            }
            if (shedule.Count == 0)
                lesson = null;
            else
                lesson = shedule[0];
            return lesson;
        }
        private static Lesson GetNextLesson(bool minutes, int hour)
        {
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
                    return lesson;
                }
            }
            if (shedule.Count == 0)
                lesson = null;
            else
                lesson = shedule[0];
            return lesson;
        }

        //Tests part
        public async static void TestAll(object state)
        {
            //add test to DB
            foreach (User u in Program.users)
            {
                try
                {
                    if (u.subscriber[Type - 1] == "1" && u.subjects.Contains(Type.ToString() + ";") && Int32.Parse(u.health[Type - 1]) > 0)
                    {
                        u.currentlesson = currentlesson;
                        u.mistakes = 0;
                        u.points = 0;
                        u.ontest = true;
                        u.currentquestion = 0;
                        await Program.bot.SendTextMessageAsync(u.id, Program.currentlesson.test.Text, replyMarkup: menu2);
                        Program.currentlesson.test.questions[0].Ask(u.id);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"There was exception for {u.id} with msg: {exception.Message}");
                }
            }
            //Timer until next webinar
            InitializeStopTimer();
            InitializeTestTimer();
        }

        public async static void StopTest(object state)
        {
            foreach (User u in Program.users)
            {
                try
                {
                    if (u.group == 0)
                        break;
                    if (currentlesson.group == u.group && u.ontest)
                    {
                        u.ontest = false;
                        u.currentquestion = 0;
                        u.health[Type - 1] = (Int32.Parse(u.health[Type - 1]) - 1).ToString();
                        if (Int32.Parse(u.health[Type - 1]) <= 0)
                        {
                            await Program.bot.SendTextMessageAsync(u.id, $"Ви не виконали домашнє завдання! На жаль, ви втрачаєте життя.\nНа жаль у вас закінчились усі життя і ви вилітаєте з нашої програми.");
                            u.subscriber[Type - 1] = "0";
                            ExecuteMySql($"UPDATE users SET health = '{String.Join(";", u.health)}', Subscriber = '{String.Join(";", u.subscriber)}' WHERE id = {u.id}");
                        }
                        else
                        {
                            await Program.bot.SendTextMessageAsync(u.id, $"Ви не виконали домашнє завдання! На жаль, ви втрачаєте життя. Тепер у вас {u.health[Type - 1]} життів.");
                            ExecuteMySql($"UPDATE users SET health = '{String.Join(";", u.health)}' WHERE id = {u.id}");
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"There was exception for {u.id} with msg: {exception.Message}");
                }
            }
            //u.GetNextWebinar();
            InitializeStopTimer();
        }

        // Database part
        public static void LoadFromDB()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(connectionString);

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

                //Load Lessons
                command = "SELECT * FROM lessons";
                cmd = new MySqlCommand(command, con);

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetInt32("Type") == Program.Type)
                    {
                        string[] ids = reader.GetString("questions").Replace(" ", "").Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                        List<Question> q = new List<Question>();
                        for (int i = 0; i < ids.Length; i++)
                        {
                            q.Add(questions.Find(x => x.id == Int32.Parse(ids[i])));
                        }

                        Lesson lesson = new Lesson(reader.GetInt32("id"), reader.GetString("Name"), DateTime.Parse(reader.GetString("DateTime")), reader.GetInt32("Group"), new Test(reader.GetString("rule"), q), reader.GetInt32("Type"), reader.GetString("Link"), reader.GetString("Tokens"));
                        lessons.Add(lesson);
                    }
                }
                reader.Close();

                //Load groups
                command = $"SELECT * FROM groups";
                cmd = new MySqlCommand(command, con);

                reader = cmd.ExecuteReader();
                string curator = "";

                while (reader.Read())
                {
                    if (reader.GetString("Curator") != null)
                        curator = reader.GetString("Curator");
                    groups.Add(new Group(reader.GetInt32("id"), reader.GetString("Name"), curator, reader.GetString("Link")));
                }
                reader.Close();

                //Load Users
                command = "SELECT * FROM users";
                cmd = new MySqlCommand(command, con);

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User(
                        reader.GetInt32("ID"),
                        reader.GetString("Name") + " " + reader.GetString("Soname"),
                        reader.GetString("Subscriber"),
                        reader.GetString("Health"),
                        reader.GetInt32("Coins"),
                        reader.GetInt32("Group"),
                        reader.GetString("Curator"),
                        reader.GetString("Subjects"),
                        reader.GetString("Stats"),
                        reader.GetString("State")
                        ));
                }
                reader.Close();

                con.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Виникла помилка при завантажені бази даних");
                Console.WriteLine(exception.Message);
                LoadFromDB();
            }
        }

        public static void CheckForUpdates(Object source, System.Timers.ElapsedEventArgs e)
        {
            MySqlConnection con = new MySqlConnection(connectionString);
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
                            users_update = reader.GetDateTime("LastUpdate");
                            UpdateUsers();
                        }
                        break;
                    case "lessons":
                        if (lessons_update < reader.GetDateTime("LastUpdate"))
                        {
                            lessons_update = reader.GetDateTime("LastUpdate");
                            UpdateLessons();
                        }
                        break;
                    case "groups":
                        if (groups_update < reader.GetDateTime("LastUpdate"))
                        {
                            groups_update = reader.GetDateTime("LastUpdate");
                            UpdateGroups();
                        }
                        break;
                    default: break;
                }
            }
            con.Close();
        }

        public static void UpdateUsers()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(connectionString);
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
                        User user = users.Find(x => x.id == id);
                        user.subscriber = reader.GetString("Subscriber").Split(";", StringSplitOptions.RemoveEmptyEntries);
                        user.health = reader.GetString("Health").Split(";", StringSplitOptions.RemoveEmptyEntries);
                        user.coins = reader.GetInt32("Coins");
                        user.group = reader.GetInt32("Group");
                        user.curator = reader.GetString("Curator");
                        user.subjects = reader.GetString("Subjects");
                        user.statistic = reader.GetString("Stats");
                    }
                    else
                    {
                        users.Add(new User(
                            reader.GetInt32("ID"),
                            reader.GetString("Name") + " " + reader.GetString("Soname"),
                            reader.GetString("Subscriber"),
                            reader.GetString("Health"),
                            reader.GetInt32("Coins"),
                            reader.GetInt32("Group"),
                            reader.GetString("Curator"),
                            reader.GetString("Subjects"),
                            reader.GetString("Stats"),
                            reader.GetString("State")
                        ));
                    }
                }
                reader.Close();
                
                con.Close();
                for(int i = 0; i < users.Count; i++)
                {
                    if (!ids.Exists(x => x == users[i].id))
                    {
                        users.RemoveAt(i);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Виникла помилка при оновлені користувачів");
                Console.WriteLine(exception.Message);
                UpdateUsers();
            }
        }

        public static void UpdateLessons()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(connectionString);
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

                //Load Lessons
                command = "SELECT * FROM lessons";
                cmd = new MySqlCommand(command, con);
                lessons.Clear();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetInt32("Type") == Program.Type)
                    {
                        string[] ids = reader.GetString("questions").Replace(" ", "").Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                        List<Question> q = new List<Question>();
                        for (int i = 0; i < ids.Length; i++)
                        {
                            q.Add(questions.Find(x => x.id == Int32.Parse(ids[i])));
                        }

                        Lesson lesson = new Lesson(reader.GetInt32("id"), reader.GetString("Name"), DateTime.Parse(reader.GetString("DateTime")), reader.GetInt32("Group"), new Test(reader.GetString("rule"), q), reader.GetInt32("Type"), reader.GetString("Link"), reader.GetString("Tokens"));
                        lessons.Add(lesson);
                    }
                }
                reader.Close();

                con.Close();
                HMNotificationTimer();
                WebinarNotificationTimer();
                InitializeTestTimer();
                InitializeStopTimer();
                LinkSenderTimer();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Виникла помилка при оновлені уроків");
                Console.WriteLine(exception.Message);
                UpdateLessons();
            }
        }

        public static void UpdateGroups()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(connectionString);
                con.Open();

                string command = $"SELECT * FROM groups";
                MySqlCommand cmd = new MySqlCommand(command, con);

                MySqlDataReader reader = cmd.ExecuteReader();
                groups.Clear();
                while (reader.Read())
                {
                    groups.Add(new Group(reader.GetInt32("id"), reader.GetString("Name"), reader.GetString("Curator"), reader.GetString("Link")));
                }
                reader.Close();

                con.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Виникла помилка при оновлені груп");
                Console.WriteLine(exception.Message);
                UpdateLessons();
            }
        }

        public static bool ExecuteMySql(string command)
        {
            try
            {
                MySqlConnection con = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand(command, con);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                return true;
            }
            catch (Exception exception)
            {
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

            public Group(int id, string name, string curator, string link)
            {
                this.id = id;
                this.name = name;
                this.curator = curator;
                this.link = link;
            }
        }
        public enum SubjectType : int
        {
            Nothing = 0,
            Ukrainian = 1,
            Math = 2,
            Boilogy = 3
        }
    }
}
