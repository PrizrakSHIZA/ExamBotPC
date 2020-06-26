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

            Program.con.Open();

            //Load groups
            string command = $"SELECT * FROM groups WHERE id = {user.group}";
            MySqlCommand cmd = new MySqlCommand(command, Program.con);

            MySqlDataReader reader = cmd.ExecuteReader();
            string webinarsarr = "";
            while (reader.Read())
            {
                webinarsarr = reader.GetString("webinars");
            }
            reader.Close();

            //Get only needed webinars
            string[] webinarsIds = webinarsarr.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in webinarsIds)
            {
                shedule.Add(Program.webinars[Convert.ToInt32(s) - 1]);
            }
            shedule.OrderBy( x => x.date).ToList();
            
            //Create shedules string
            string text = "";
            foreach (Webinar w in shedule)
            {
                text += $"{w.name} - {w.date}\n";
            }
            await Program.bot.SendTextMessageAsync(user.id, text);
        }
    }
}
