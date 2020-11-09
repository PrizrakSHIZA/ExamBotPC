using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExamBotPC.Tests.Questions
{
    [Serializable]
    class TestQuestion : Question
    {
        public override int id { get ; set; }
        public override string text { get; set; }
        public override string image { get; set; }
        public override int points { get; set; }
        public override string[] variants { get; set; }

        public int columns;
        public override dynamic answer { get; set; }

        public async override void Ask(long id)
        {
            try
            {
                User user = Program.GetCurrentUser(id);
                int qcount = user.currentlesson.test.questions.Count;
                string prefix = "";

                if (user.currentquestion == 0)
                    prefix = Program.presets[0];
                else if (user.currentquestion + 1 == qcount)
                    prefix = Program.presets[1];
                else
                {
                    Random rnd = new Random();
                    prefix = Program.presets[rnd.Next(2, Program.presets.Length)];
                }

                string premsg = $"{prefix} Завдання {user.currentquestion + 1}/{qcount}\n\n";

                InlineKeyboardMarkup keyboard = Program.GetInlineKeyboard(variants, columns);
                if (image == "#")
                {
                    var message = await Program.bot.SendTextMessageAsync(user.id, premsg + text, replyMarkup: keyboard);
                    user.lastmsg = message.MessageId;
                }
                else
                {
                    InputOnlineFile imageFile = new InputOnlineFile(new MemoryStream(Convert.FromBase64String(image.Split(",")[1])));
                    await Program.bot.SendPhotoAsync(user.id, imageFile);
                    var message = await Program.bot.SendTextMessageAsync(user.id, premsg + text, replyMarkup: keyboard);
                    user.lastmsg = message.MessageId;
                }
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(typeof(Program));
                log.Error(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public TestQuestion(int id, string text, int points, string[] variants, int columns, string answer, string image)
        {
            this.id = id;
            this.text = text;
            this.points = points;
            this.variants = variants;
            this.columns = columns;
            this.answer = answer;
            this.image = image;
        }
    }
}
