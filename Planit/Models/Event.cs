using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Planit.Models
{
    public class Event
    {
        public enum Type
        {
            Recurring,
            OneTime
        }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public Type EventType { get; set; }
        public bool OnMon { get; set; }
        public bool OnTue { get; set; }
        public bool OnWed { get; set; }
        public bool OnThu { get; set; }
        public bool OnFri { get; set; }
        public bool OnSat { get; set; }
        public bool OnSun { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime Date { get; set; }
    }
}
