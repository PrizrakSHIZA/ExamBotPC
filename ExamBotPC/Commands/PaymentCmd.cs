using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class PaymentCmd : Command
    {
        public override string Name => "Оплата 💳";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User u = Program.GetCurrentUser(e);

            await Program.bot.SendTextMessageAsync(u.id, $"Твоє посилання на оплату 👉 https://examschool.online/order?phone={u.phone}");
        }
    }
}
