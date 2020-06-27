using ExamBotPC.Tests.Questions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamBotPC.Tests
{
    [Serializable]
    public class Test
    {
        /// <summary>
        /// 1 - test
        /// 2 - homework
        /// </summary>
        public int id;
        public string Text;
        public List<Question> questions;

        public Test(int id, string text, List<Question> q)
        {
            this.id = id;
            Text = text;
            questions = q;
        }
        public Test(Test test)
        {
            id = test.id;
            Text = test.Text;
            questions = test.questions;
        }
    }
}
