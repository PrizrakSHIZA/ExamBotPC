using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class A_TestId : Command
    {
        public override string Name => "/testid";

        public override bool forAdmin => true;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.users.Find(u => u.id == e.Message.Chat.Id);
            int index = e.Message.Text.IndexOf(" ");
            if (index == -1)
            {
                await Program.bot.SendTextMessageAsync(user.id, $"Наразі номер тесту {User.currenttest}. Після проведення тесту змінна буде автоматично змінанена на +1 о 23:59\n(Для зміни номеру вручну введіть: /testid 'новий номер')");
            }
            else
            {
                try
                {
                    int number = Int32.Parse(e.Message.Text.Substring(index + 1));
                    if (number + 1 > Program.testlist.Count)
                    {
                        User.currenttest = number;
                        await Program.bot.SendTextMessageAsync(user.id, $"Увага! Тесту за даним номером не існує! Тест не буде відправлено у необхідний час чи за командою!");
                    }
                    else if (number < 0)
                    {
                        await Program.bot.SendTextMessageAsync(user.id, $"Номер не може бути нижче за нуль!");
                    }
                    else
                    {
                        User.currenttest = number;
                        await Program.bot.SendTextMessageAsync(user.id, $"Номер успішно встановлено!");
                    }
                }
                catch (Exception exception)
                {
                    await Program.bot.SendTextMessageAsync(user.id, $"Помилка при отримані номеру. Можливо дані було введено невірно.");
                }
            }
        }
    }
}
