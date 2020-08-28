using ExamBotPC.Tests.Questions;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class RecordsCmd : Command
    {
        public override string Name => "Записи уроків ▶";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            if(user.group == 0)
                await Program.bot.SendTextMessageAsync(user.id, "Вам ще не назначили групу");
            else if (Program.groups.Find(x => x.id == user.group).link.Length == 0)
                await Program.bot.SendTextMessageAsync(user.id, "Посилання ще не було додано до вашої групи. Спробуйте пізніше.");
            else
                await Program.bot.SendTextMessageAsync(user.id, $"Посилання на плейлист вашої групи: {Program.groups.Find(x => x.id == user.group).link}");
        }
    }
}
