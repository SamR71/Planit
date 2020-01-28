using Planit.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace Planit.Data
{
    class TaskPlanner
    {
        //calculates (or recalculates) the positioning of PlannedTasks
        async public void PlanTasks(bool recalcAll)
        {
            //get tasks, events, and planned tasks
            List<Event> EventsList = await App.DB.GetEventsAsync();
            List<Task> TasksList = await App.DB.GetTasksAsync();
            List<PlannedTask> PlannedList = await App.DB.GetPlannedAsync();

            //get hours in day (THIS CALCULATION REALLY OUGHT TO BE REFACTORED SOMEHOW)
            long defaultBedTime = 0;
            var sleep = Preferences.Get("bedtime", defaultBedTime);

            long defaultWakeTime = 288000000000; //8 am in ticks
            var wake = Preferences.Get("waketime", defaultWakeTime);

            int wakeUpTime = (int)((long)wake / 36000000000);
            int bedTime = (int)Math.Ceiling((double)sleep / (double)36000000000);

            int hoursInDay;
            if (wakeUpTime > bedTime)
            {
                hoursInDay = (24 + bedTime) - wakeUpTime;
            }
            else
            {
                hoursInDay = bedTime - wakeUpTime;
            }

            //get farthest out Deadline
            DateTime today = DateTime.Today;

            DateTime farthestOut = DateTime.Today;
            TimeSpan largestSpan = new TimeSpan(0);
            foreach(Task t in TasksList)
            {
                TimeSpan diff = t.Due.Subtract(today);
                if(diff.TotalDays > largestSpan.TotalDays)
                {
                    farthestOut = t.Due;
                    largestSpan = diff;
                }
            }

            int daysToDoAll = largestSpan.Days + 1;

            //create calendar object as int array for a "working platform"
            int[,] calendar = new int[(4*hoursInDay),daysToDoAll];
            DayOfWeek[] days = GetDaysOfWeek(today,daysToDoAll);
            DateTime[] dates = GetDates(today,daysToDoAll);

            //go through all tasks and events, place in calendar
            foreach(Event e in EventsList)
            {
                //go through each day, see if we should put task in
                for(int i = 0; i < daysToDoAll; i++)
                {
                    if (dates[i] == e.Date || OnToday(days[i], e))
                    {
                        //TODO: PLACE THE EVENT IN THE CALENDAR FOR THIS DAY
                    }

                }

            }
            foreach(PlannedTask pt in PlannedList)
            {
                for(int i = 0; i < daysToDoAll; i++)
                {
                    if(pt.Date == dates[i])
                    {
                        ////TODO: PLACE THE PLANNEDTASK IN THE CALENDAR FOR THIS DAY
                    }
                }
                
            }

            //Calculate blocks needed for each task

            //not sure if best to use dictionary here, but eh
            Dictionary<Task, int> blocksLeft = new Dictionary<Task, int>();

            foreach(Task t in TasksList)
            {
                blocksLeft.Add(t, (int)t.HoursLeft * 4);
            }


        }

        //THIS BETTER ALSO BE REFACTORED SOMEHOW
        private bool OnToday(DayOfWeek dayOfWeek, Event e)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return e.OnMon;
                case DayOfWeek.Tuesday:
                    return e.OnTue;
                case DayOfWeek.Wednesday:
                    return e.OnWed;
                case DayOfWeek.Thursday:
                    return e.OnThu;
                case DayOfWeek.Friday:
                    return e.OnFri;
                case DayOfWeek.Saturday:
                    return e.OnSat;
                case DayOfWeek.Sunday:
                    return e.OnSun;
                default:
                    return false;
            }
        }

        private DayOfWeek[] GetDaysOfWeek(DateTime firstDay, int totalDays)
        {
            DayOfWeek[] days = new DayOfWeek[totalDays];
            days[0] = firstDay.Date.DayOfWeek;
            for(int i = 1; i < totalDays; i++)
            {
                days[i] = days[i - 1] + 1;
            }

            return days;
        }

        private DateTime[] GetDates(DateTime firstDay, int totalDays)
        {
            DateTime[] dates = new DateTime[totalDays];
            dates[0] = firstDay;
            for(int i = 1; i < totalDays; i++)
            {
                dates[i] = dates[i - 1].AddDays(1);
            }

            return dates;
        }
        
        async public void UpdateTasks()
        {

        }

    }
}
