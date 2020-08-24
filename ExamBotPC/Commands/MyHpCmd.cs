using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class MyHpCmd : Command
    {
        public override string Name => "Мої життя ♥";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);

            string msg = "Ваше життя:\n\n";
            for (int i = 0; i < Int32.Parse(user.health[Program.Type - 1]); i++)
            {
                msg += "♥";
            }

            await Program.bot.SendTextMessageAsync(user.id, msg);
        }
    }
}
