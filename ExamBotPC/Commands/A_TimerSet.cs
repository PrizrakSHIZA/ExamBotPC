using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class A_TimerSet : Command
    {
        public override string Name => "/timerset";

        public override bool forAdmin => true;

        public async override void Execute(MessageEventArgs e)
        {
            User currentuser = Program.users.Find(u => u.id == e.Message.Chat.Id);
            int index = e.Message.Text.IndexOf(" ");
            if (index == -1)
            {
                await Program.bot.SendTextMessageAsync(currentuser.id, "Використовуйте команду наступним чином: /timerset 'час у форматі HH:MM'");
            }
            else
            {
                string message = e.Message.Text.Substring(index + 1);
                string[] messagearr = message.Replace(" ", "").Split(':');
                try
                {
                    Program.InitializeTimer(Int32.Parse(messagearr[0]), Int32.Parse(messagearr[1]));
                    await Program.bot.SendTextMessageAsync(currentuser.id, $"Таймер встановлено на {messagearr[0]}:{messagearr[1]}");
                }
                catch (Exception exception)
                {
                    await Program.bot.SendTextMessageAsync(currentuser.id, $"Виникла помилка. Текст помилки: {exception.Message}");
                }
            }
        }
    }
}
