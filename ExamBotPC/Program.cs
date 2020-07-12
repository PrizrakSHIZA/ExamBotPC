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
using System.Globalization;
using System.IO;

namespace ExamBotPC
{
    class Program
    {
        public static TelegramBotClient bot;
        public static List<Command> commands = new List<Command>();
        public static List<User> users = new List<User>();
        public static List<Test> testlist = new List<Test>();
        public static List<Question> questions = new List<Question>();
        public static List<Webinar> webinars = new List<Webinar>();
        public static List<string> groups = new List<string>();
        public static string connectionString = new MySqlConnectionStringBuilder()
        {
            Server = APIKeys.DBServer,
            Database = APIKeys.DBName,
            UserID = APIKeys.DBUser,
            Password = APIKeys.DBPassword,
            ConvertZeroDateTime = true
        }.ConnectionString;
        public static string password = APIKeys.password;
        public static bool useTimer = true;
        public static char[] delimiterChars = { ',', '.', '\t', '\n', ';' };
        public static int Type = (int)SubjectType.Ukrainian;

        static int currentwebinar;
        static DateTime users_update, tests_update, webinars_update, groups_update; 
        static Timer TestTimer, StopTimer, HMTimer, WebinarTimer;

        static void Main(string[] args)
        {
            //Loading data
            LoadFromDB();

            users_update = tests_update = webinars_update = groups_update = DateTime.Now;
            //Add all commands
            AddAllCommands();

            //Initialize timers
            UpdateDBTimer();
            HMNotificationTimer();
            WebinarNotificationTimer();
            InitializeTestTimer();

            //Initialize bot client
            bot = new TelegramBotClient(APIKeys.TestBotApi) { Timeout = TimeSpan.FromSeconds(10) };

            //Starting message
            var me = bot.GetMeAsync().Result;
            Console.WriteLine($"Its me, {me}!");

            bot.StartReceiving();
            bot.OnMessage += Bot_OnMessage;
            bot.OnCallbackQuery += Bot_OnCallbackQuery;

            Console.ReadKey();
        }

        private async static void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            User user = GetCurrentUser(e);
            //if user is on test
            if (user.ontest)
            {
                Question question = testlist[User.currenttest].questions[user.currentquestion];
                //Check answer is right or wrong
                if (e.CallbackQuery.Data == question.answer)
                {
                    await bot.SendTextMessageAsync(user.id, "Правильно!");
                    user.currentquestion++;
                    user.points[testlist[User.currenttest].id - 1] += question.points;
                }
                else
                {
                    await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.answer}");
                    user.mistakes[testlist[User.currenttest].id - 1][user.currentquestion] = true;
                    user.currentquestion++;
                }
                //Check if its last question in test
                if (user.currentquestion >= testlist[User.currenttest].questions.Count)
                {
                    if (ExecuteMySql($"UPDATE Users SET CompletedTests = CONCAT(CompletedTests, '{testlist[User.currenttest].id};') ,Points = '{JsonSerializer.Serialize(user.points)}, Mistakes = '{JsonSerializer.Serialize(user.mistakes)}' WHERE ID = {user.id}"))
                    {
                        user.ontest = false;
                        user.currentquestion = 0;
                        await bot.SendTextMessageAsync(user.id, $"Вітаю! Ви закінчили тест. Ви набрали {user.points[testlist[User.currenttest].id - 1]} балів!");
                    }
                    else
                    {
                        Console.WriteLine("Error!");
                    }
                }
                else
                {
                    testlist[User.currenttest].questions[user.currentquestion].Ask(user.id);
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
                        user.subjects += $"{Type};";
                }
            }
            else
            {
                if (ExecuteMySql($"INSERT INTO Users(ID, Name, Soname, Date, Subjects) VALUES ({e.Message.Chat.Id}, '{e.Message.Chat.FirstName}', '{e.Message.Chat.LastName}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', '{Type};')"))
                    users.Add(new User(e.Message.Chat.Id, e.Message.Chat.FirstName + " " + e.Message.Chat.LastName, false, false, "", "", "", 0, 0, 0, 0, $"{Type};"));
                return;
            }

            //Add user in temp var
            user = GetCurrentUser(e);

            var text = e?.Message?.Text;
            if (text == null) return;
            //If user completing test
            else if (user.ontest)
            {
                string answer = e.Message.Text;
                Question question = testlist[User.currenttest].questions[user.currentquestion];
                //Check conformity question
                if (testlist[User.currenttest].questions[user.currentquestion] is ConformityQuestion)
                {
                    ConformityQuestion q = (ConformityQuestion)testlist[User.currenttest].questions[user.currentquestion];
                    //Delete spaces
                    if (q.IsRight(answer))
                    {
                        await bot.SendTextMessageAsync(user.id, "Правильно!");
                        user.currentquestion++;
                        user.points[testlist[User.currenttest].id - 1] += question.points;
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.answer}");
                        user.mistakes[testlist[User.currenttest].id - 1][user.currentquestion] = true;
                        user.currentquestion++;
                    }
                }
                //Check other type
                else if (answer.ToLower() == question.answer.ToLower())
                {
                    await bot.SendTextMessageAsync(user.id, "Правильно!");
                    user.currentquestion++;
                    user.points[testlist[User.currenttest].id - 1] += question.points;
                }
                else
                {
                    await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.answer}");
                    user.mistakes[testlist[User.currenttest].id - 1][user.currentquestion] = true;
                    user.currentquestion++;
                }
                //Check if its last question in test
                if (user.currentquestion >= testlist[User.currenttest].questions.Count)
                {
                    if (ExecuteMySql($"UPDATE Users SET CompletedTests = CONCAT(CompletedTests, '{testlist[User.currenttest].id};') ,Points = '{JsonSerializer.Serialize(user.points)}', Mistakes = '{JsonSerializer.Serialize(user.mistakes)}' WHERE ID = {user.id}"))
                    {
                        user.ontest = false;
                        user.currentquestion = 0;
                        await bot.SendTextMessageAsync(user.id, $"Вітаю! Ви закінчили тест. Ви набрали {user.points[testlist[User.currenttest].id - 1]} балів!");
                    }
                    else
                    {
                        Console.WriteLine("Error!");
                    }
                }
                else
                {
                    testlist[User.currenttest].questions[user.currentquestion].Ask(user.id);
                }
            }
            else
            {
                //Check commands
                text += " ";
                int index = text.IndexOf(" ");
                text = text.Substring(0, index);

                if (commands.Find(c => c.Name == text) != null)
                {
                    Command cmd = commands.Find(c => c.Name == text);
                    if (!cmd.forAdmin || (cmd.forAdmin && user.admin))
                    {
                        cmd.Execute(e);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(user.id, "У вас немає доступу до цієї команди");
                    }
                }
                else
                {
                    await bot.SendTextMessageAsync(user.id, "Такої команди не уснує. Для списку усіх команд введіть: '/help'");
                }
            }
        }

        private static void AddAllCommands()
        {
            commands.Add(new A_RestartTimer());
            //commands.Add(new A_Send());
            commands.Add(new A_TestAll());
            commands.Add(new A_TestId());
            commands.Add(new A_TestList());
            commands.Add(new A_TimerTurn());
            //commands.Add(new AskCmd());
            commands.Add(new BalanceCmd());
            commands.Add(new HelpCommand());
            commands.Add(new SheduleCmd());
            commands.Add(new StopCmd());
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
            int steps = (int)Math.Round((double)array.Length / column);
            var keyboardInline = new InlineKeyboardButton[steps][];
            for (int y = 0; y < steps; y++)
            {
                var keyboardButtons = new InlineKeyboardButton[column];
                for (int i = 0; i < column; i++)
                {
                    keyboardButtons[i] = new InlineKeyboardButton
                    {
                        Text = array[(y * column) + i],
                        CallbackData = array[(y * column) + i],
                    };
                }
                keyboardInline[y] = keyboardButtons;
            }

            return keyboardInline;
        }

        //Timers part
        public static void UpdateDBTimer()
        {
            System.Timers.Timer DBChecker = new System.Timers.Timer(TimeSpan.FromMinutes(2).TotalMilliseconds);
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

            Webinar webinar = GetNextWebinar();

            //set new timer
            HMTimer = new Timer(new TimerCallback(HomeworkNotification));
            DateTime temptime = webinar.time.AddHours(-10);
            if (temptime.TimeOfDay > DateTime.Now.TimeOfDay)
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

            Webinar webinar = GetNextWebinar();

            //set new timer
            WebinarTimer = new Timer(new TimerCallback(WebinarNotification));
            DateTime temptime = webinar.time.AddHours(-2);

            if (temptime.TimeOfDay > DateTime.Now.TimeOfDay)
            {
                int msUntilTime = (int)((temptime - DateTime.Now).TotalMilliseconds);
                WebinarTimer.Change(msUntilTime, Timeout.Infinite);
            }
        }
        
        public static void InitializeTestTimer()
        {
            Webinar webinar = GetNextWebinar();

            DateTime TestTime = webinar.time.AddHours(2);
            if (useTimer)
            {
                if (TestTimer != null)
                    TestTimer.Dispose();
                TestTimer = new Timer(new TimerCallback(TestAll));

                // Figure how much time until seted time
                DateTime now = DateTime.Now;

                int days = Math.Abs(webinar.day - ((int)DateTime.Now.DayOfWeek == 0 ? 7 : (int)DateTime.Now.DayOfWeek));
                int msUntilTime = (int)((TestTime.AddDays(days) - now).TotalMilliseconds);
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
            Webinar webinar = GetNextWebinar();
            DateTime TestTime = webinar.time;

            //delete last timer
            if (StopTimer != null)
                StopTimer.Dispose();
            StopTimer = null;
            //get next webinar datetime

            //set new timer
            StopTimer = new Timer(new TimerCallback(StopTest));
            int days = Math.Abs(webinar.day - ((int)DateTime.Now.DayOfWeek == 0 ? 7 : (int)DateTime.Now.DayOfWeek));
            int msUntilTime = (int)((TestTime.AddDays(days) - DateTime.Now).TotalMilliseconds);
            StopTimer.Change(msUntilTime, Timeout.Infinite);
        }

        private async static void HomeworkNotification(object state)
        {
            foreach (User u in users)
            {
                if (u.group == 0)
                    break;
                if (groups[u.group - 1].Contains(currentwebinar.ToString() +";"))
                    await bot.SendTextMessageAsync(u.id, "Нагудую, що тобі необхідно виконати домашнє завдання! В тебе ще 10 годин!");
            }
            HMNotificationTimer();
        }

        private async static void WebinarNotification(object state)
        {
            foreach (User u in users)
            {
                if (u.group == 0)
                    break;
                if (groups[u.group - 1].Contains(currentwebinar.ToString() + ";"))
                    await bot.SendTextMessageAsync(u.id, "Нагудую, що через 2 години вебінар!");
            }
            HMNotificationTimer();
        }

        private static Webinar GetNextWebinar()
        {
            List<Webinar> shedule = new List<Webinar>(webinars);
            foreach (Webinar w in webinars)
            {
                if (w.date < DateTime.Now)
                    shedule.Remove(w);
            }
            shedule = shedule.OrderBy(x => x.day).ToList();

            //Find nearest webinar for current subject
            Webinar webinar = new Webinar();
            for (int i = 0; i < shedule.Count; i++)
            {
                if (shedule[i].day >= ((int)DateTime.Now.DayOfWeek == 0 ? 7 : (int)DateTime.Now.DayOfWeek) && shedule[i].time.TimeOfDay > DateTime.Now.TimeOfDay)
                {
                    currentwebinar = shedule[i].id;
                    webinar = shedule[i];
                    return webinar;
                }
            }
            webinar = shedule[0];
            return webinar;
        }
        
        //Tests part
        public async static void TestAll(object state)
        {
            //Check if program have test to send
            if (User.currenttest + 1 > testlist.Count)
            {
                //Send msg to admins if no
                foreach (User u in Program.users)
                {
                    if (u.admin)
                    {
                        await bot.SendTextMessageAsync(u.id, $"Неможливо відправити тести користувачам оскільки немає тесту за індексом {User.currenttest}!");
                    }
                }
            }
            else
            {
                //add test to DB
                if (true)
                {
                    foreach (User u in Program.users)
                    {
                        if (u.subscriber && u.subjects.Contains(Type.ToString()) && u.health > 0)
                        {
                            bool[] tempbool = Enumerable.Repeat(false, testlist[User.currenttest].questions.Count).ToArray();
                            u.mistakes[testlist[User.currenttest].id - 1] = tempbool;

                            u.points[testlist[User.currenttest].id - 1] = 0;
                            u.completedtests += $"{Program.testlist[User.currenttest].id};";
                            u.ontest = true;
                            u.currentquestion = 0;
                            await Program.bot.SendTextMessageAsync(u.id, Program.testlist[User.currenttest].Text);
                            Program.testlist[User.currenttest].questions[0].Ask(u.id);
                        }
                    }
                    //Timer until next webinar
                    InitializeStopTimer();
                    InitializeTestTimer();
                }
                else
                {
                    foreach (User u in Program.users)
                    {
                        if (u.admin)
                        {
                            await bot.SendTextMessageAsync(u.id, "Виникла помилка у роботі з базою даних при відправленні тестів. Будь ласка зверніться до техніячного адміністратора!");
                        }
                    }
                }
            }
        }

        public async static void StopTest(object state)
        {
            foreach (User u in Program.users)
            {
                if (groups[u.group + 1].Contains(currentwebinar.ToString() + ";") && u.ontest)
                {
                    u.ontest = false;
                    u.currentquestion = 0;
                    u.health -= 1;
                    if (u.health <= 0)
                    {
                        await Program.bot.SendTextMessageAsync(u.id, $"Ви не виконали домашнє завдання! На жаль, ви втрачаєте життя.\nНа жаль у вас закінчились усі життя і ви вилітаєте з нашої програми.");
                        u.subscriber = false;
                        ExecuteMySql($"UPDATE users SET (health) VALUES (0) WHERE id = {u.id}");
                    }
                    else
                    {
                        await Program.bot.SendTextMessageAsync(u.id, $"Ви не виконали домашнє завдання! На жаль, ви втрачаєте життя. Теперь у вас {u.health} життів.");
                        ExecuteMySql($"UPDATE users SET health = {u.health} WHERE id = {u.id}");
                    }
                }
                //u.GetNextWebinar();
                InitializeStopTimer();
            }
        }

        // Database part
        public static void LoadFromDB()
        {
            //try
            //{
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
                                reader.GetString("variants").Replace(" ", "").Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries),
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
                        default: break;
                    }
                }
                reader.Close();

                //Load Tests
                command = "SELECT * FROM tests";
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
                        q.Add(questions[Int32.Parse(ids[i]) - 1]);
                    }

                    testlist.Add(new Test(reader.GetInt32("id"), reader.GetString("rule"), q));
                }
                }
                reader.Close();

                //Load shedules
                command = "SELECT * FROM webinars WHERE Type = " + Type;
                cmd = new MySqlCommand(command, con);

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    webinars.Add(new Webinar(
                        reader.GetInt32("id"), 
                        reader.GetString("Name"), 
                        reader.GetInt32("Day"),
                        DateTime.ParseExact(reader.GetString("Time"), "HH:mm:ss", CultureInfo.InvariantCulture), 
                        reader.GetDateTime("EndDate"), 
                        reader.GetInt32("Type")));
                }
                reader.Close();

                //Load groups
                command = $"SELECT * FROM groups";
                cmd = new MySqlCommand(command, con);

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    groups.Add(reader.GetString("webinars"));
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
                        Convert.ToBoolean(reader.GetUInt32("Subscriber")),
                        Convert.ToBoolean(reader.GetUInt32("Admin")),
                        reader.GetString("Points"),
                        reader.GetString("CompletedTests"),
                        reader.GetString("Mistakes"),
                        reader.GetInt32("Coins"),
                        reader.GetInt32("Health"),
                        reader.GetInt32("Group"),
                        reader.GetInt32("Curator"),
                        reader.GetString("Subjects")
                        ));
                }
                reader.Close();

                con.Close();
            //}
            //catch (Exception exception)
            //{
            //    Console.WriteLine(exception.Message);
            //    Console.WriteLine("Потрібен перезапуск");
            //}
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
                    case "tests":
                        if (tests_update < reader.GetDateTime("LastUpdate"))
                        {
                            tests_update = reader.GetDateTime("LastUpdate");
                            UpdateTests();
                        }
                        break;
                    case "webinars":
                        if (webinars_update < reader.GetDateTime("LastUpdate"))
                        {
                            webinars_update = reader.GetDateTime("LastUpdate");
                            UpdateWebinars();
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
            MySqlConnection con = new MySqlConnection(connectionString);
            con.Open();
            //Load Users
            string command = "SELECT * FROM users";
            MySqlCommand cmd = new MySqlCommand(command, con);

            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                long id = reader.GetInt32("id");
                if (users.Find(x => x.id == id) != default(User))
                {
                    User user = users.Find(x => x.id == id);
                    user.subscriber = Convert.ToBoolean(reader.GetUInt32("Subscriber"));
                    user.admin = Convert.ToBoolean(reader.GetUInt32("Admin"));
                    if(reader.GetString("Points") != "")
                        user.points = JsonSerializer.Deserialize<int[]>(reader.GetString("Points"));
                    user.completedtests = "";
                    //int[] temp = reader.GetString("CompletedTests").Replace(" ", "").Split(Program.delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();                
                    user.completedtests = reader.GetString("CompletedTests");
                    if (reader.GetString("Mistakes") != "")
                        user.mistakes = JsonSerializer.Deserialize<bool[][]>(reader.GetString("Mistakes"));
                    user.coins = reader.GetInt32("Coins");
                    user.health = reader.GetInt32("Health");
                    user.group = reader.GetInt32("Group");
                    user.curator = reader.GetInt32("Curator");
                }
                else
                {
                    users.Add(new User(
                        reader.GetInt32("ID"),
                        reader.GetString("Name") + " " + reader.GetString("Soname"),
                        Convert.ToBoolean(reader.GetUInt32("Subscriber")),
                        Convert.ToBoolean(reader.GetUInt32("Admin")),
                        reader.GetString("Points"),
                        reader.GetString("CompletedTests"),
                        reader.GetString("Mistakes"),
                        reader.GetInt32("Coins"),
                        reader.GetInt32("Health"),
                        reader.GetInt32("Group"),
                        reader.GetInt32("Curator"),
                        reader.GetString("Subjects")
                    ));
                }
            }
            reader.Close();

            con.Close();

        }

        public static void UpdateTests()
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
                                reader.GetString("variants").Replace(" ", "").Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries),
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
                        default: break;
                    }

                }
                reader.Close();

                //Load Tests
                command = "SELECT * FROM tests";
                cmd = new MySqlCommand(command, con);

                reader = cmd.ExecuteReader();
                testlist.Clear();
                while (reader.Read())
                {
                    if (reader.GetInt32("Type") == Program.Type)
                    {
                        string[] ids = reader.GetString("questions").Replace(" ", "").Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                        List<Question> q = new List<Question>();
                        for (int i = 0; i < ids.Length; i++)
                        {
                            q.Add(questions[Int32.Parse(ids[i]) - 1]);
                        }
                        testlist.Add(new Test(reader.GetInt32("id"), reader.GetString("rule"), q));
                    }
                }
                reader.Close();

                con.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine("Потрібен перезапуск");
            }
        }

        public static void UpdateWebinars()
        {
            MySqlConnection con = new MySqlConnection(connectionString);
            con.Open();
            string command = "SELECT * FROM webinars WHERE Type = " + Type;
            MySqlCommand cmd = new MySqlCommand(command, con);

            MySqlDataReader reader = cmd.ExecuteReader();
            webinars.Clear();
            while (reader.Read())
            {
                webinars.Add(new Webinar(reader.GetInt32("id"), reader.GetString("Name"), reader.GetInt32("Day"), reader.GetDateTime("Time"), reader.GetDateTime("EndDate"), reader.GetInt32("Type")));
            }
            reader.Close();

            con.Close();
            HMNotificationTimer();
            WebinarNotificationTimer();
        }

        public static void UpdateGroups()
        {
            MySqlConnection con = new MySqlConnection(connectionString);
            con.Open();

            string command = $"SELECT * FROM groups";
            MySqlCommand cmd = new MySqlCommand(command, con);

            MySqlDataReader reader = cmd.ExecuteReader();
            groups.Clear();
            while (reader.Read())
            {
                groups.Add(reader.GetString("webinars"));
            }
            reader.Close();

            con.Close();
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

        public enum SubjectType : int
        {
            Nothing = 0,
            Ukrainian = 1,
            TrainUkrainian = 2,
        }
    }
}
