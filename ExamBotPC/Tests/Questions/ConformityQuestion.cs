﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExamBotPC.Tests.Questions
{
    [Serializable]
    class ConformityQuestion : Question
    {
        public override int id { get; set; }
        public override string text { get; set; }
        public override string image { get; set; }
        public override int points { get; set; }
        public override dynamic answer { get; set; }
        public override string[] variants { get; set; }

        public string rule = "\n(Будь ласка заповнюйте відповідь у вигляді: 'А-1,Б-2,В-3,Г-4')";

        char[] delimiterChars = { ' ', ',', '.', '\t', '\n', ';' };

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

        public ConformityQuestion(int id, string text, int points, string answer, string image)
        {
            this.id = id;
            this.text = text;
            this.points = points;
            this.answer = answer;
            this.image = image;
        }

        public bool IsRight(string answer)
        {
            answer = answer.Replace("-", "").Replace(" ", "").ToLower();
            List<string> answerarr = answer.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList();
            string[] rightnaswer = this.answer.Replace("-", "").Replace(" ", "").ToLower().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
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
