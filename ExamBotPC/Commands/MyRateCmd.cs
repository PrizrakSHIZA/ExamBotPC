using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class MyRateCmd : Command
    {
        public override string Name => "Мій рейтинг 📈";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);

            string[] stats = user.statistic.Split(";", StringSplitOptions.RemoveEmptyEntries);
            int points = 0;
            for (int i = 0; i < stats.Length; i++)
            {
                points += Int32.Parse(stats[i].Split(":")[2]);
            }
            string msg = $"Мій рейтинг:\n\nВи набрали загалом {points} балів!";
            await Program.bot.SendTextMessageAsync(user.id, msg);
        }
    }
}
