using ExamTrainBot.Commands;
using ExamTrainBot.Tests;
using ExamTrainBot.Tests.Questions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExamTrainBot
{
    class Program
    {
        public static TelegramBotClient bot;
        public static List<Command> commands = new List<Command>();
        public static List<User> users = new List<User>();
        public static List<Test> testlist = new List<Test>();
        public static List<Question> questions = new List<Question>();
        public static DateTime TestTime = DateTime.Today.AddHours(14);
        public static MySqlConnection con = new MySqlConnection(
                    new MySqlConnectionStringBuilder()
                    {
                        Server = APIKeys.DBServer,
                        Database = APIKeys.DBName,
                        UserID = APIKeys.DBUser,
                        Password = APIKeys.DBPassword
                    }.ConnectionString
                );
        public static string password = APIKeys.password;
        public static bool useTimer = false;
        public static char[] delimiterChars = { ',', '.', '\t', '\n', ';' };

        static Timer timer;

        static void Main(string[] args)
        {
            //Loading data
            LoadFromDB();
            //SaveSystem.Load();

            //Add all commands
            AddAllCommands();


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
                }
                else
                {
                    await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.answer}");
                    user.currentquestion++;
                }
                //Check if its last question in test
                if (user.currentquestion >= testlist[User.currenttest].questions.Count)
                {
                    //ExecuteMySql($"UPDATE Users SET Points = CONCAT(points, '{user.points[^1] + 1};'), Mistakes = CONCAT(Mistakes, '{string.Join(';', user.mistakes)}') WHERE ID = {user.id}");
                    user.ontest = false;
                    user.currentquestion = 0;
                    //await bot.SendTextMessageAsync(user.id, $"Вітаю! Ви закінчили тест. Ви набрали {user.points[^1]} балів!");
                }
                else
                {
                    testlist[User.currenttest].questions[user.currentquestion].Ask(user.id);
                }
            }
        }

        private async static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            //check if new user is subscriber

            //Add user in temp var
            User user = users.Find(u => u.id == e.Message.Chat.Id);

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
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.answer}");
                        user.currentquestion++;
                    }
                }
                //Check other type
                else if (answer.ToLower() == question.answer.ToLower())
                {
                    await bot.SendTextMessageAsync(user.id, "Правильно!");
                    user.currentquestion++;
                }
                else
                {
                    await bot.SendTextMessageAsync(user.id, $"Неправильно! Правильна відповідь: {question.answer}");
                    user.currentquestion++;
                }
                //Check if its last question in test
                if (user.currentquestion >= testlist[User.currenttest].questions.Count)
                {
                    //ExecuteMySql($"UPDATE Users SET Points = CONCAT(points, '{user.points[^1] + 1};'), Mistakes = '{JsonSerializer.Serialize(user.mistakes)}' WHERE ID = {user.id}");
                    user.ontest = false;
                    user.currentquestion = 0;
                    //await bot.SendTextMessageAsync(user.id, $"Вітаю! Ви закінчили тест. Ви набрали {user.points[^1]} балів!");
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
                    if (!cmd.forAdmin || (cmd.forAdmin && user.isadmin))
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
            commands.Add(new HelpCommand());
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

        public static void LoadFromDB()
        {
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
                        case 1: questions.Add(new TestQuestion(
                            reader.GetString("text"), 
                            reader.GetInt32("points"),
                            reader.GetString("variants").Replace(" ","").Split(delimiterChars), 
                            reader.GetInt32("columns"), 
                            reader.GetString("answer"))); break;
                        case 2: questions.Add(new FreeQuestion(
                            reader.GetString("text"),
                            reader.GetInt32("points"),
                            reader.GetString("answer")
                            )); break;
                        case 3: questions.Add(new ConformityQuestion(
                            reader.GetString("text"),
                            reader.GetInt32("points"),
                            reader.GetString("answer")
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
                    string[] ids = reader.GetString("questions").Replace(" ", "").Split(delimiterChars);
                    List<Question> q = new List<Question>();
                    for (int i = 0; i < ids.Length; i++)
                    {
                        q.Add(questions[ Int32.Parse(ids[i]) - 1 ]);
                    }
                    testlist.Add(new Test(reader.GetString("rule"), q));
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
                        reader.GetDateTime("Date")
                        ));
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

        public static bool ExecuteMySql(string command)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(command, Program.con);
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
    }
}
