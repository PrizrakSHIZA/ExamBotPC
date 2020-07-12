using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;

namespace ExamBotPC.Tests.Questions
{
    [Serializable]
    class FreeQuestion : Question
    {
        public override int id { get; set; }
        public override string text { get; set; }
        public override string image { get; set; }
        public override int points { get; set; }
        public override dynamic answer { get; set; }
        public override string[] variants { get; set; }

        public string rule = "\n(Будь ласка, будьте уважні при написанні відповіді!)";
        public async override void Ask(long id)
        {
            User user = Program.GetCurrentUser(id);
            if(image == "#")
                await Program.bot.SendTextMessageAsync(user.id, text + rule);
            else
            {
                InputOnlineFile imageFile = new InputOnlineFile(new MemoryStream(Convert.FromBase64String(image.Split(",")[1])));
                await Program.bot.SendPhotoAsync(user.id, imageFile);
                await Program.bot.SendTextMessageAsync(user.id, text + rule);
            }
        }

        public FreeQuestion(int id, string text, int points, string answer, string image)
        {
            this.text = text;
            this.points = points;
            this.answer = answer;
            this.image = image;
        }
    }
}
