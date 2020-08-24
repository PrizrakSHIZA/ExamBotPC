using ExamBotPC.Tests;
using Microsoft.VisualBasic.CompilerServices;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamBotPC.UserSystem
{
    public class Lesson
    {
        public int id;
        public string name;
        public DateTime datetime;
        public int group;
        public Test test;
        public int type;
        public string link;
        public string[] tokens;

        public Lesson(int id, string name, DateTime datetime, int group, Test test, int type, string link, string tokens)
        {
            this.id = id;
            this.name = name;
            this.datetime = datetime;
            this.group = group;
            this.test = test; 
            this.type = type;
            this.link = link;
            this.tokens = tokens.Split(Program.delimiterChars, StringSplitOptions.RemoveEmptyEntries);
        }
        public Lesson()
        {
        }
    }
}
