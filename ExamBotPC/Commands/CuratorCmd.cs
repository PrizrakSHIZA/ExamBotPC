using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class CuratorCmd : Command
    {
        public override string Name => "📞Мій куратор📞";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);

            await Program.bot.SendTextMessageAsync(user.id, $"{user.curator}");
        }
    }
}
