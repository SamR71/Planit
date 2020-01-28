using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Planit.Models
{
    public class Task
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Due { get; set; }
        public TimeSpan DueTime { get; set; }
        public float HoursLeft { get; set; }

    }
}
