using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ExamBotPC.Tests.Questions
{
    [Serializable]
    class FreeQuestion : Question
    {
        public override string text { get; set; }
        public override int points { get; set; }
        public override dynamic answer { get; set; }
        public override string[] variants { get; set; }

        public string rule = "\n(Будь ласка, будьте уважні при написанні відповіді!)";
        public async override void Ask(long id)
        {
            User user = Program.GetCurrentUser(id);
            await Program.bot.SendTextMessageAsync(user.id, text + rule);
        }

        public FreeQuestion(string text, int points, string answer)
        {
            this.text = text;
            this.points = points;
            this.answer = answer;
        }
    }
}
