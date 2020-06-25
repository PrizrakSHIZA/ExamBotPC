using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ExamTrainBot.Commands
{
    class HelpCommand : Command
    {
        public override string Name => "/help";

        public override bool forAdmin => false;

        public override async void Execute(MessageEventArgs e)
        {
            string list = "";
            foreach (Command command in Program.commands)
            {
                if (command.forAdmin & Program.users.Find(u => u.id == e.Message.Chat.Id).isadmin)
                    list += "Admin: " + command.Name + "\n";
                else if (!command.forAdmin)
                    list += command.Name + "\n";
            }
            await Program.bot.SendTextMessageAsync(e.Message.Chat.Id, list);
        }
    }
}
