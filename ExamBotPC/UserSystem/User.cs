using ExamBotPC.Tests;
using ExamBotPC.UserSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ExamBotPC
{
    public class User
    {
        public long id;
        public string name;
        public string username;
        public bool ontest = false;
        public string[] subscriber = { "0", "0", "0", "0", "0", "0", "0", "0" }, health = { "5", "5", "5", "5", "5", "5", "5", "5" };
        public int currentquestion = 0, coins = 0, group = 0;
        public string curator;
        public int points = 0;
        public int mistakes = 0;
        public string subjects;
        public string statistic;
        public Lesson currentlesson = new Lesson();
        public string[] state;

        public User(long id, string name)
        {
            this.id = id;
            this.name = name;
        }
        public User(long id, string name, string username, string subscriber, string health, int coins,  int group, string curator, string subjects, string statistic, string statestr)
        {
            this.id = id;
            this.name = name;
            this.username = username;
            this.subscriber = subscriber.Split(";", StringSplitOptions.RemoveEmptyEntries);
            this.coins = coins;
            this.health = health.Split(";", StringSplitOptions.RemoveEmptyEntries);
            this.group = group;
            this.curator = curator;
            this.subjects = subjects;
            this.statistic = statistic;
            // string like "ontest;lessonid;questionnumber;points;mistakes"
            this.state = statestr.Split("|", StringSplitOptions.RemoveEmptyEntries);
            string[] tmpstate = state[Program.Type - 1].Substring(1).Split(";");
            if (tmpstate[0] == "1")
            {
                ontest = true;
                currentlesson = Program.lessons.Find(x => x.id == Int32.Parse(tmpstate[1]));
                currentquestion = Int32.Parse(tmpstate[2]);
                points = Int32.Parse(tmpstate[3]);
                mistakes = Int32.Parse(tmpstate[4]); 
            }
        }
    }
}
