using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class MenuCmd : Command
    {
        public override string Name => "/menu";

        public override bool forAdmin => false;

        public override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            if(user.ontest)
                Program.bot.SendTextMessageAsync(user.id, "Головне меню:", replyMarkup: Program.menutest);
            else
                Program.bot.SendTextMessageAsync(user.id, "Головне меню:", replyMarkup: Program.menu);
        }
    }
}
