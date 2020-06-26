using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class AskCmd : Command
    {
        public override string Name => "/ask";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            int index = e.Message.Text.IndexOf(" ");
            if (index == -1)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Використовуйте команду наступним чином: /ask 'Текст'");
            }
            else
            {
                string message = e.Message.Text.Substring(index + 1);
                await Program.bot.SendTextMessageAsync(user.curator, $"Повідомлення від {user.name} - {user.id}:\n{message}");
            }
        }
    }
}
