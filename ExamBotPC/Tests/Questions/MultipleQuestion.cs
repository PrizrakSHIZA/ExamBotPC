using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Telegram.Bot.Types.InputFiles;

namespace ExamBotPC.Tests.Questions
{
    [Serializable]
    class MultipleQuestion : Question
    {
        public override int id { get; set; }
        public override string text { get; set; }
        public override string image { get; set; }
        public override int points { get; set; }
        public override string[] variants { get; set; }
        public override dynamic answer { get; set; }

        public string rule = "\n(Будь ласка заповнюйте відповідь у вигляді: 'А,Б,В,Г')";

        public async override void Ask(long id)
        {
            User user = Program.GetCurrentUser(id);
            if (image == "#")
                await Program.bot.SendTextMessageAsync(user.id, text + rule);
            else
            {
                InputOnlineFile imageFile = new InputOnlineFile(new MemoryStream(Convert.FromBase64String(image.Split(",")[1])));
                await Program.bot.SendPhotoAsync(user.id, imageFile);
                await Program.bot.SendTextMessageAsync(user.id, text + rule);
            }
        }

        public MultipleQuestion(int id, string text, string image, int points, string answer)
        {
            this.id = id;
            this.text = text;
            this.image = image;
            this.points = points;
            this.answer = answer;
        }

        public bool IsRight(string answer)
        {
            answer = answer.Replace("-", "").Replace(" ", "").ToLower();
            List<string> answerarr = answer.Split(Program.delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList();
            string[] rightnaswer = this.answer.Replace(" ", "").ToLower().Split(Program.delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            if (rightnaswer.Length != answerarr.Count)
                return false;
            if (!Enumerable.SequenceEqual(answerarr.OrderBy(t => t), rightnaswer.OrderBy(t => t)))
                return false;
            return true;
        }
    }
}
