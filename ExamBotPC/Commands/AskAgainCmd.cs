using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class AskAgainCmd : Command
    {
        public override string Name => "Повторити запитання з тесту 🔄";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            if (user.ontest)
                user.currentlesson.test.questions[user.currentquestion].Ask(user.id);
            else
                await Program.bot.SendTextMessageAsync(user.id, "Наразі ви виконали усі ДЗ");
        }
    }
}
