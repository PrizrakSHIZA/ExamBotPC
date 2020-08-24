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
            string[] stats = user.statistic.Split(";",StringSplitOptions.RemoveEmptyEntries);
            if (stats.Length == 0)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Ви ще не проходили тестів");
                return;
            }
            int mistakes = 0, questions = 0;
            for (int i = 0; i < stats.Length; i++)
            {
                mistakes += Int32.Parse(stats[i].Split(":")[1].Split("/")[0]);
                questions += Int32.Parse(stats[i].Split(":")[1].Split("/")[1]);
            }
            float percent = ((float)questions - (float)mistakes) / (float)questions * 100f;

            string msg = "Моя статистика помилок:\n\n";
            msg += $"Пройдено тестів - {stats.Length}\n";
            msg += $"Відношення помилок до пройдени питань - {mistakes}/{questions}\n";
            msg += $"У вас {percent}% правильних відповідей!";
            await Program.bot.SendTextMessageAsync(user.id, msg);
        }
    }
}
