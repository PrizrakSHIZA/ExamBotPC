using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExamTrainBot.Tests.Questions
{
    [Serializable]
    class ConformityQuestion : Question
    {
        public override string text { get; set; }
        public override int points { get; set; }
        public override dynamic answer { get; set; }
        public override string[] variants { get; set; }

        public string rule = "\n(Будь ласка заповнюйте відповідь у вигляді: 'А-1,Б-2,В-3,Г-4')";

        char[] delimiterChars = { ' ', ',', '.', '\t', '\n' };

        public async override void Ask(long id)
        {
            User user = Program.GetCurrentUser(id);
            await Program.bot.SendTextMessageAsync(user.id, text + rule);
        }

        public ConformityQuestion(string text, int points, string answer)
        {
            this.text = text;
            this.points = points;
            this.answer = answer;
        }

        public bool IsRight(string answer)
        {
            answer = answer.Replace("-", "").ToLower();
            List<string> answerarr = answer.Split(delimiterChars).ToList();
            for (int i = 0; i < answerarr.Count; i++)
            {
                if (answerarr[i] == "")
                    answerarr.Remove(answerarr[i]);
            }
            string[] rightnaswer = this.answer.Replace("-", "").ToLower().Split(delimiterChars);
            if (rightnaswer.Length != answerarr.Count)
                return false;
            for (int i = 0; i < rightnaswer.Length; i++)
            {
                for (int y = 0; y < answerarr.Count; y++)
                {
                    if (answerarr[i].Contains(rightnaswer[y].ToCharArray()[0]))
                        if (!answerarr[i].Contains(rightnaswer[y].ToCharArray()[1]))
                            return false;
                }
            }
            return true;
        }
    }
}
