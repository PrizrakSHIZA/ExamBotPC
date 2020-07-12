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
        public int[] points = new int[1000];
        public string subjects;
        public string completedtests;
        public bool[][] mistakes = new bool[1000][];

        public static int currenttest = 0;
        public int currentTest_serializable { get { return currenttest; } set { currenttest = value; } }

        public User(long id, string name)
        {
            this.id = id;
            this.name = name;
            this.subscriber = false;
        }
        public User(long id, string name, bool subscriber, bool admin, string points, string tests, string mistakes, int coins, int health, int group, long curator, string subjects)
        {
            this.id = id;
            this.name = name;
            this.subscriber = subscriber;
            this.admin = admin;
            if (points != "" || tests != "" || mistakes != "")
            {
                this.points = JsonSerializer.Deserialize<int[]>(points);
                //int[] temp = tests.Replace(" ", "").Split(Program.delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                completedtests = tests;
                this.mistakes = JsonSerializer.Deserialize<bool[][]>(mistakes);
            }
            this.coins = coins;
            this.health = health;
            this.group = group;
            this.curator = curator;
            this.subjects = subjects;
        }
    }
}
