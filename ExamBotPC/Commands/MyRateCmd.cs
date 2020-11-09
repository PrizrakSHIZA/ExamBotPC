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

            string[] stats = user.statistic.Split(";", StringSplitOptions.RemoveEmptyEntries);
            int points = 0;
            for (int i = 0; i < stats.Length; i++)
            {
                if (stats[i].Split(":")[0] == prefix)
                    points += Int32.Parse(stats[i].Split(":")[3]);
            }
            string msg = $"Мій рейтинг:\n\nВи набрали загалом {points} балів!";
            await Program.bot.SendTextMessageAsync(user.id, msg);
        }
    }
}
