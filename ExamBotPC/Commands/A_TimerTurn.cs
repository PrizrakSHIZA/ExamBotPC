using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class A_TimerTurn : Command
    {
        public override string Name => "/timerturn";

        public override bool forAdmin => true;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.users.Find(u => u.id == e.Message.Chat.Id);
            int index = e.Message.Text.IndexOf(" ");
            if (index == -1)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Використовуйте команду наступним чином: /timerturn 'on/off'");
            }
            else
            {
                string message = e.Message.Text.Substring(index + 1);
                message = message.ToLower();
                switch (message)
                {
                    case "on": 
                        Program.useTimer = true;
                        Program.InitializeTimer(Program.TestTime.Hour, Program.TestTime.Minute);
                        await Program.bot.SendTextMessageAsync(user.id, "Таймер було включено!"); break;
                    case "off":
                        Program.useTimer = false;
                        Program.InitializeTimer(Program.TestTime.Hour, Program.TestTime.Minute);
                        await Program.bot.SendTextMessageAsync(user.id, "Таймер було вимкнено!"); break;
                    default: break;
                }
            }
        }
    }
}
