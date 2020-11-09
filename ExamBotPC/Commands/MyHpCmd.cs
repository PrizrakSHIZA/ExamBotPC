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

            string msg = "Ваші життя:\n\n";
            if (user.health[Program.Type - 1] == -7)
            {
                msg += "Система життів відключена.";
            }
            else
            {
                for (int i = 0; i < user.health[Program.Type - 1]; i++)
                {
                    msg += "♥";
                }
            }

            await Program.bot.SendTextMessageAsync(user.id, msg);
        }
    }
}
