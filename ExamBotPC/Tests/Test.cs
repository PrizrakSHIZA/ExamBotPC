using ExamBotPC.Tests.Questions;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Requests;

namespace ExamBotPC.Tests
{
    [Serializable]
    public class Test
    {
        /// <summary>
        /// 1 - test
        /// 2 - homework
        /// </summary>
        public string Text;
        public List<Question> questions;

        public Test(string text, List<Question> q)
        {
            Text = text;
            questions = q;
        }
        public Test(Test test)
        {
            Text = test.Text;
            questions = test.questions;
        }
    }
}
