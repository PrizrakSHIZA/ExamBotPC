using ExamTrainBot.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ExamTrainBot
{
    [Serializable]
    public class User
    {
        public long id;
        public string name;
        public bool subscriber,isadmin, ontest = false;
        public int currentquestion = 0;
        public int coins = 0;
        public int health = 5;

        public static int currenttest = 0;
        public int currentTest_serializable { get { return currenttest; } set { currenttest = value; } }

        public User(long id, string name)
        {
            this.id = id;
            this.name = name;
            this.subscriber = false;
            this.isadmin = false;
        }
        public User(long id, string name, bool subscriber, bool isadmin, int coins, int health)
        {
            this.id = id;
            this.name = name;
            this.subscriber = subscriber;
            this.isadmin = isadmin;
            this.coins = coins;
            this.health = health;
        }
    }
}
