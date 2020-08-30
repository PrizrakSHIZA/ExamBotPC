using ExamBotPC.UserSystem;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

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
                MySqlConnection con = new MySqlConnection(Program.connectionString);
                con.Open();

                string command = $"SELECT * FROM shedules where shedules.Group = {user.groups}";
                MySqlCommand cmd = new MySqlCommand(command, con);
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                InputOnlineFile imageFile = new InputOnlineFile(new MemoryStream(Convert.FromBase64String(reader.GetString("Image").Split(",")[1])));
                await Program.bot.SendPhotoAsync(user.id, imageFile);
            }
            catch (Exception exception)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Вибачте, але для вашої групи ще не підготували розклад.");
            }
        }
    }
}
