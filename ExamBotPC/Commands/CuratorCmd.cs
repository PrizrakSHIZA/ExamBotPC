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
            if (user.group == 0)
            {
                await Program.bot.SendTextMessageAsync(user.id, "Вибачте але вам ще не назначили групу. Зверніться до адміністрації.");
                return;
            }
            if (user.curator == "0")
            {
                if (Program.groups.Find(x => x.id == user.group).curator != "")
                    await Program.bot.SendTextMessageAsync(user.id, $"{Program.groups.Find(x => x.id == user.group).curator}");
                else
                    await Program.bot.SendTextMessageAsync(user.id, "Вашій групі ще не назначили куратора");
            }
            else
                await Program.bot.SendTextMessageAsync(user.id, $"{user.curator}");
        }
    }
}
