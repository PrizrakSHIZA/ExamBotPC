using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class MainManuCmd : Command
    {
        public override string Name => "Головне меню ◀";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            string msg = "Повернення у головне меню";
            if(user.ontest)
                await Program.bot.SendTextMessageAsync(user.id, msg, replyMarkup: Program.menutest);
            else
                await Program.bot.SendTextMessageAsync(user.id, msg, replyMarkup: Program.menu);
        }
    }
}
