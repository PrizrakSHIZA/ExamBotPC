using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class CuratorCmd : Command
    {
        public override string Name => "Мій куратор 💬";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            if (user.groups.Count == 0)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Вибачте але вам ще не назначили групу. Зверніться до адміністрації.");
                return;
            }
            if (user.curator == "0")
            {
                Program.Group group = new Program.Group(-1, "", "", "", 0);
                foreach (int g in user.groups)
                {
                    if (Program.groups.Find(x => x.id == g).type == Program.Type)
                        group = Program.groups.Find(x => x.id == g);
                }
                try
                {
                    if (!String.IsNullOrEmpty(group.curator))
                        await Program.bot.SendTextMessageAsync(user.id, $"Твій куратор {group.curator} відповість на всі твої запитання!\nПиши, не соромся 💌");
                    else
                        await Program.bot.SendTextMessageAsync(user.id, "Вашій групі ще не назначили куратора");
                }
                catch (Exception exception)
                {
                    await Program.bot.SendTextMessageAsync(user.id, "Вам ще не назначили групи за цим предметом");
                }
            }
            else
                await Program.bot.SendTextMessageAsync(user.id, $"Твій куратор {user.curator} відповість на всі твої запитання!\nПиши, не соромся 💌");
        }
    }
}
