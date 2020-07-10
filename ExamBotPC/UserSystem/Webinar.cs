using Microsoft.VisualBasic.CompilerServices;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamBotPC.UserSystem
{
    class Webinar
    {
        public int id;
        public string name;
        public int day;
        public DateTime time;
        public DateTime date;
        public int type;

        public Webinar(int id, string name, int day, DateTime time, DateTime enddate, int type)
        {
            this.id = id;
            this.name = name;
            this.day = day;
            this.time = time;
            this.date = enddate;
            this.type = type;
        }
        public Webinar()
        {
        }
    }


}
