using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Planit.Models
{
    public class PlannedTask
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public bool UserModified { get; set; }
        public Task Parent { get; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime {get; set;}

        public PlannedTask()
        {
            UserModified = false;
        }

    }
}
