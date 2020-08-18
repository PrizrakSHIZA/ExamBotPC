using ExamBotPC.Tests.Questions;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class RecordsCmd : Command
    {
        public override string Name => "📅Розклад📅";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            await Program.bot.SendTextMessageAsync(user.id, "Тут ви зможете передивитись усі записи вебінарів: \nhttps://www.youtube.com/watch?v=NCDdRTGqDRI&list=PLqvueu1TRj7_mkhJ7yuZzpAuoFm27NxQQ");
        }
    }
}
