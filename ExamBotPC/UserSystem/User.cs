using ExamBotPC.Tests;
using ExamBotPC.UserSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ExamBotPC
{
    [Serializable]
    public class User
    {
        public long id;
        public string name;
        public bool subscriber, admin, ontest = false;
        public int currentquestion = 0, coins = 0, health = 5, group = 0;
        public long curator;
        public List<int> points = new List<int>();
        public List<Test> completedtests = new List<Test>();
        public List<bool[]> mistakes = new List<bool[]>();
        public DateTime nextwebinar;

        public static int currenttest = 0;
        public int currentTest_serializable { get { return currenttest; } set { currenttest = value; } }

        public User(long id, string name)
        {
            this.id = id;
            this.name = name;
            this.subscriber = false;
        }
        public User(long id, string name, bool subscriber, bool admin, string points, string tests, string mistakes, int coins, int health, int group, long curator)
        {
            this.id = id;
            this.name = name;
            this.subscriber = subscriber;
            this.admin = admin;
            if (points != "" || tests != "" || mistakes != "")
            {
                this.points = points.Replace(" ", "").Split(Program.delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToList();
                int[] temp = tests.Replace(" ", "").Split(Program.delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                for (int i = 0; i < temp.Length; i++)
                {
                    completedtests.Add(Program.alltests[temp[i] - 1]);
                }
                this.mistakes = JsonSerializer.Deserialize<List<bool[]>>(mistakes);
            }
            this.coins = coins;
            this.health = health;
            this.group = group;
            this.curator = curator;

            //get next webinar
            List<Webinar> shedule = new List<Webinar>();
            string[] webinarsIds = Program.groups[this.group - 1].Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in webinarsIds)
            {
                shedule.Add(Program.webinars[Convert.ToInt32(s) - 1]);
            }
            shedule = shedule.OrderBy(x => x.date).ToList();
            nextwebinar = shedule[0].date;
        }
    }
}
