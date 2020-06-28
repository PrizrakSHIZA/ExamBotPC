using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamBotPC.UserSystem
{
    class Webinar
    {
        public int id;
        public string name;
        public DateTime date;
        public int type;

        public Webinar(int id, string name, DateTime date, int type)
        {
            this.id = id;
            this.name = name;
            this.date = date;
            this.type = type;
        }
        public Webinar()
        {
        }
    }


}
