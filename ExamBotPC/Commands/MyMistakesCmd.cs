using System;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class MyMistakesCmd : Command
    {
        public override string Name => "Статистика помилок ❗";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);

            string prefix = "";
            switch (Program.Type)
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
            int length = 0;

            string[] stats = user.statistic.Split(";",StringSplitOptions.RemoveEmptyEntries);
            if (stats.Length == 0)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Ви ще не проходили тестів");
                return;
            }
            int mistakes = 0, questions = 0;
            for (int i = 0; i < stats.Length; i++)
            {
                if (stats[i].Split(":")[0] == prefix)
                {
                    length++;
                    mistakes += Int32.Parse(stats[i].Split(":")[2].Split("/")[0]);
                    questions += Int32.Parse(stats[i].Split(":")[2].Split("/")[1]);
                }
            }
            float percent = ((float)questions - (float)mistakes) / (float)questions * 100f;

            string msg = "Моя статистика помилок:\n\n";
            msg += $"Пройдено тестів - {length}\n";
            msg += $"Відношення помилок до пройдени питань - {mistakes}/{questions}\n";
            msg += $"У вас {percent}% правильних відповідей!";
            await Program.bot.SendTextMessageAsync(user.id, msg);
        }
    }
}
