using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class CuratorCmd : Command
    {
        public override string Name => "Допомога 💬";

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
                try
                {
                    if (Program.groups.Find(x => x.type == Program.Type).curator != "")
                        await Program.bot.SendTextMessageAsync(user.id, $"{Program.groups.Find(x => x.type == Program.Type).curator}");
                    else
                        await Program.bot.SendTextMessageAsync(user.id, "Вашій групі ще не назначили куратора");
                }
                catch (Exception exception)
                {
                    await Program.bot.SendTextMessageAsync(user.id, "Вам ще не назначили групи за цим предметом");
                }
            }
            else
                await Program.bot.SendTextMessageAsync(user.id, $"{user.curator}");
        }
    }
}
