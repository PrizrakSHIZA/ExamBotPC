using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;
using static ExamBotPC.Program;

namespace ExamBotPC.Commands
{
    class SheduleCmd : Command
    {
        public override string Name => "Розклад 📅";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            if (user.groups.Count ==  0)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Вибачте але вам ще не назначили групу. Зверніться до адміністрації.");
                return;
            }
            try
            {
                int groupid = 0;
                List<Group> thislist = Program.groups.FindAll(x => x.type == Program.Type);
                foreach (Group g in thislist)
                {
                    if (user.groups.Contains(g.id))
                    {
                        groupid = g.id;
                    }
                }
                MySqlConnection con = new MySqlConnection(Program.connectionString);
                con.Open();

                string command = $"SELECT * FROM shedules where shedules.Group = {groupid}";
                MySqlCommand cmd = new MySqlCommand(command, con);
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                InputOnlineFile imageFile = new InputOnlineFile(new MemoryStream(Convert.FromBase64String(reader.GetString("Image").Split(",")[1])));
                await Program.bot.SendPhotoAsync(user.id, imageFile);
            }
            catch (Exception exception)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Вибачте, але для вашої групи ще не підготували розклад.");
                Console.WriteLine(exception.Message);
            }
        }
    }
}
