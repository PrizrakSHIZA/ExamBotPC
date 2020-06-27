using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class A_TestAll : Command
    {
        public override string Name => "/testall";

        public override bool forAdmin => true;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            Program.TestAll(new object());
            await Program.bot.SendTextMessageAsync(user.id, "Тести відправлені!");
            /*
            foreach (User u in Program.users)
            {
                if (u.isadmin)
                {
                    u.points.Add(0);
                    u.completedtests.Add(Program.testlist[User.currenttest]);
                    u.ontest = true;
                    u.currentquestion = 0;
                    await Program.bot.SendTextMessageAsync(u.id, Program.testlist[User.currenttest].Text);
                    Program.testlist[User.currenttest].questions[0].Ask(u.id);
                }
            }*/
        }
    }
}
