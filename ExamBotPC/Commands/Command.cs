using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExamBotPC.Commands
{
    abstract class Command
    {
        public abstract string Name { get; }
        public abstract bool forAdmin { get; }

        public abstract void Execute(MessageEventArgs e);
    }
}
