using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ExamBotPC.Tests.Questions
{
    [Serializable]
    public abstract class Question
    {
        public abstract string text { get; set; }
        public abstract int points { get; set; }
        public abstract string[] variants { get; set; }
        public abstract dynamic answer { get; set; }
        public abstract void Ask(long id);
    }
}
