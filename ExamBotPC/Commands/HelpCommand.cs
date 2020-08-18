using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class HelpCommand : Command
    {
        public override string Name => "/help";

        public override bool forAdmin => true;

        public override async void Execute(MessageEventArgs e)
        {
            string list = "";
            foreach (Command command in Program.commands)
            {
                if (command.forAdmin & Program.users.Find(u => u.id == e.Message.Chat.Id).admin)
                    list += "Admin: " + command.Name + "\n";
                else if (!command.forAdmin)
                    list += command.Name + "\n";
            }
            await Program.bot.SendTextMessageAsync(e.Message.Chat.Id, list);
        }
    }
}
