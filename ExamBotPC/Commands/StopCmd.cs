using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class StopCmd : Command
    {
        public override string Name => "/stop";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            if(Program.ExecuteMySql($"UPDATE users SET Subjects = REPLACE(subjects, '{Program.Type};', '') WHERE id = {user.id}"))
                user.subjects = user.subjects.Replace($"{Program.Type};", "");
            await Program.bot.SendTextMessageAsync(user.id, "Цей предмет був видалений з вашого списку. Ви можете знов підписатися на цього бота, якщо напишите будь-що боту.");
        }
    }
}
