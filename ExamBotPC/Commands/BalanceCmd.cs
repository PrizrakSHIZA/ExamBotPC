using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class BalanceCmd : Command
    {
        public override string Name => "💰Баланс💰";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            await Program.bot.SendTextMessageAsync(user.id, $"На вашому балансі {user.coins} 💵");
        }
    }
}
