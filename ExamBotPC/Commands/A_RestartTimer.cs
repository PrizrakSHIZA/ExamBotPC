using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class A_RestartTimer : Command
    {
        public override string Name => "/restarttimer";

        public override bool forAdmin => true;

        public override void Execute(MessageEventArgs e)
        {
            Program.InitializeStopTimer();
        }
    }
}
