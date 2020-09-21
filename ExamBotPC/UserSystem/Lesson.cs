using ExamBotPC.Tests;
using System;
using static ExamBotPC.Program;

namespace ExamBotPC.UserSystem
{
    class Lesson
    {
        public int id;
        public string name;
        public DateTime datetime;
        public Group group;
        public Test test;
        public string link;
        public string[] tokens;

        public Lesson(int id, string name, DateTime datetime, int group, Test test, string link, string tokens)
        {
            this.id = id;
            this.name = name;
            this.datetime = datetime;
            this.group = Program.groups.Find(x => x.id == group);
            this.test = test; 
            this.link = link;
            this.tokens = tokens.Split(Program.delimiterChars, StringSplitOptions.RemoveEmptyEntries);
        }
        public Lesson()
        {
        }
    }
}
