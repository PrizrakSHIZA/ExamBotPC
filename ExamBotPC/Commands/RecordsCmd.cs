using ExamBotPC.Tests.Questions;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class RecordsCmd : Command
    {
        public override string Name => "Записи уроків ▶";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            Program.Group group = new Program.Group(-1, "", "", "", 0);
            try
            {
                if (user.groups.Count == 0)
                    await Program.bot.SendTextMessageAsync(user.id, "Вам ще не назначили групу");

                foreach (int g in user.groups)
                {
                    if (Program.groups.Find(x => x.id == g).type == Program.Type)
                        group = Program.groups.Find(x => x.id == g);
                }
                if(group.id == -1)
                    await Program.bot.SendTextMessageAsync(user.id, "Вам ще не назначили групу");
                else if (group.link.Length == 0)
                    await Program.bot.SendTextMessageAsync(user.id, "Посилання ще не було додано до вашої групи. Спробуйте пізніше.");
                else
                    await Program.bot.SendTextMessageAsync(user.id, $"Посилання на плейлист вашої групи: {group.link}");
            }
            catch (Exception excpetion)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Вам ще не назначили групу за цим предметом");
            }
        }
    }
}
