using ExamBotPC.UserSystem;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class SheduleCmd : Command
    {
        public override string Name => "/shedule";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            List<Webinar> shedule = new List<Webinar>();
            User user = Program.GetCurrentUser(e);
            if (user.group == 0)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Вам ще не назначили групу! Зверніться до куратора або адміністрації.");
                return;
            }
            MySqlConnection con = new MySqlConnection(Program.connectionString);
            con.Open();

            //Get only needed webinars
            string[] webinarsIds = Program.groups[user.group - 1].Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in webinarsIds)
            {
                shedule.Add(Program.webinars[Convert.ToInt32(s) - 1]);
            }
            shedule = shedule.OrderBy(x => x.day).ToList();
            for (int i = 0; i < shedule.Count; i++)
            {
                if (shedule[i].date <= DateTime.Now)
                    shedule.RemoveAt(i);
            }
            //Create shedules string
            string text = "";
            foreach (Webinar w in shedule)
            {
                text += $"{DayOfWeek[w.day]}: {w.time.TimeOfDay} - {w.name}\n";
            }
            await Program.bot.SendTextMessageAsync(user.id, text);

            con.Close();
        }

        public Dictionary<int, string> DayOfWeek = new Dictionary<int, string> 
        {
            { 1, "Понеділок" },
            { 2, "Вівторок" },
            { 3, "Середа" },
            { 4, "Четвер" },
            { 5, "П'ятниця" },
            { 6, "Субота" },
            { 7, "Неділя" },
        };
    }
}
