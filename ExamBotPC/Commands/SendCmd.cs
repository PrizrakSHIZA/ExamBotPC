using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class SendCmd : Command
    {
        public override string Name => "/send";

        public override bool forAdmin => true;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            string text = e.Message.Text;
            int index = text.IndexOf(" ");
            int index2 = text.IndexOf(" ", e.Message.Text.IndexOf(" ") + 1);
            if (index == -1 || index2 == -1)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Використовуйте команду наступним чином: /send 'id' 'text'");
            }
            else
            {
                string id = text.Split(" ")[1];
                string message = text.Substring(text.IndexOf(id) + id.Length + 1);
                await Program.bot.SendTextMessageAsync(Int32.Parse(id), $"Повідомлення від куратора:\n{message}");
            }
        }
    }
}
