using Planit.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace Planit.Data
{
    public class TaskPlanner
    {
        private int wakeHour;
        private int sleepHour;


        public TaskPlanner()
        {

        }

        //calculates (or recalculates) the positioning of PlannedTasks
        //NOTE: a block of time here is a 15 minute interval
        //-->synchs up with how the calendar is currently implemented
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

            wakeHour = wakeUpTime;
            sleepHour = bedTime;

            //get farthest out Deadline
            DateTime today = DateTime.Today;

            TimeSpan largestSpan = new TimeSpan(0);
            foreach(Task t in TasksList)
            {
                TimeSpan diff = t.Due.Subtract(today);
                if(diff.TotalDays > largestSpan.TotalDays)
                {
                    largestSpan = diff;
                }
            }

            int daysToDoAll = largestSpan.Days + 1;
            int blocksInDay = (4 * hoursInDay);

            //create calendar object as int array for a "working platform"
            int[,] calendar = new int[blocksInDay, daysToDoAll];
            DayOfWeek[] days = GetDaysOfWeek(today,daysToDoAll);
            DateTime[] dates = GetDates(today,daysToDoAll);

            //go through all tasks and events, place in calendar
            //by placing in calendar, we mean putting a -1 there to signify -- its blocked

            System.Diagnostics.Debug.WriteLine("Days to do all: " + daysToDoAll);

            int[] blocksPlaced = new int[daysToDoAll];
            int totalBlocksPlaced = 0;

            foreach(Event e in EventsList)
            {
                //go through each day, see if we should put event in
                for(int i = 0; i < daysToDoAll; i++)
                {
                    if(e.EventType == Event.Type.Recurring)
                    {
                        if (OnToday(days[i], e))
                        {
                            System.Diagnostics.Debug.WriteLine("Placing block for " + e.Name + ". From: " + e.StartTime.TotalHours + " to: " + e.EndTime.TotalHours + ", on:" + dates[i]);
                            int placed = AddBlock(e.StartTime.TotalHours, e.EndTime.TotalHours, calendar, i);
                            blocksPlaced[i] += placed;
                            totalBlocksPlaced += placed;
                        }
                    }
                    else
                    {
                        if (dates[i] == e.Date)
                        {
                            System.Diagnostics.Debug.WriteLine("Placing block for " + e.Name + ". From: " + e.StartTime.TotalHours + " to: " + e.EndTime.TotalHours + ", on:" + dates[i]);
                            int placed = AddBlock(e.StartTime.TotalHours, e.EndTime.TotalHours, calendar, i);
                            blocksPlaced[i] += placed;
                            totalBlocksPlaced += placed;
                        }
                    }
                    

                }

            }

            //only do if we recalculating all
            if (!recalcAll)
            {
                foreach (PlannedTask pt in PlannedList)
                {
                    for (int i = 0; i < daysToDoAll; i++)
                    {
                        //go through each day, see if we should put task in
                        if (pt.Date == dates[i] && pt.UserModified)
                        {
                            int placed = AddBlock(pt.StartTime.TotalHours, pt.EndTime.TotalHours, calendar, i);
                            int prevPlaced = AddBlock(pt.PrevStartTime.TotalHours, pt.PrevEndTime.TotalHours, calendar, i);
                            blocksPlaced[i] += placed;
                            blocksPlaced[i] += prevPlaced;
                            totalBlocksPlaced += placed;
                            totalBlocksPlaced += prevPlaced;
                        }
                    }

                }
            }

            //Calculate blocks needed for each task

            //not sure if best to use dictionary here, but eh
            Dictionary<Task, int> blocksLeft = new Dictionary<Task, int>();
            
            Dictionary<int, Task> IDtoTask = new Dictionary<int, Task>();
            int blocksToSchedule = 0;

            foreach(Task t in TasksList)
            {

                System.Diagnostics.Debug.WriteLine("MY NAME IS: " + t.Name + " WITH ID:" + t.ID);
                int blocksForTask = (int)t.HoursLeft * 4;

                blocksLeft.Add(t, blocksForTask);
                blocksToSchedule += blocksForTask;

                IDtoTask.Add(t.ID, t);
            }

            //go through and subtract the blocks used by the "planned" parts of the task
            if (!recalcAll)
            {
                foreach(PlannedTask pt in PlannedList)
                {
                    if (pt.UserModified)
                    {
                        int blocksUsed = (int)Math.Ceiling(pt.EndTime.Subtract(pt.StartTime).TotalHours * 4);
                        Task parent = IDtoTask[pt.ParentID];

                        System.Diagnostics.Debug.WriteLine("MY NAME IS: "+pt.Name+". MY PARENTS NAME IS: "+parent.Name+" WITH ID:"+parent.ID);

                        if (blocksLeft[parent] - blocksUsed <= 0)
                        {
                            blocksToSchedule -= blocksLeft[parent];
                            blocksLeft[parent] = 0;
                        }
                        else
                        {
                            blocksToSchedule -= blocksUsed;
                            blocksLeft[parent] = blocksLeft[parent] - blocksUsed;
                        }

                    }

                }
            }

            //calculate number of free blocks in the schedule
            //allowing an hour after/before bed for -- nothing
            //note we double count anything in the hour before/after bed (who knows if this is a bug or feature yet)

            Dictionary<Task, int> blocksFreeForEach = new Dictionary<Task, int>();

            int c = 0;
            foreach(Task t in TasksList)
            {
                TimeSpan diff = t.Due.Subtract(today);
                int daysToDo = diff.Days + 1;
                blocksFreeForEach.Add(t, (((blocksInDay - 8) * daysToDo) - blocksPlaced[c]));
                c++;
            }

            int blocksFree = ((blocksInDay - 8) * daysToDoAll) - totalBlocksPlaced;

            //these variable determine where the for loop starts
            int lastDayPlaced = 0;

            int lastBlockPlaced = (DateTime.Now.Hour * 4) + (DateTime.Now.Minute / 15) - (wakeHour*4);

            //task blocks consecutively added in one go
            int consecutiveAdded = 0;

            while (blocksFree > 0 && blocksToSchedule > 0)
            {
                Task t = MostPressing(blocksFreeForEach, blocksLeft);

                for(int i = lastDayPlaced; i < daysToDoAll; i++)
                {
                    //reset after we scan to next day
                    if (i != lastDayPlaced)
                        lastBlockPlaced = 4;

                    for(int j = lastBlockPlaced; j < blocksInDay-4; j++)
                    {
                        


                        //if block is free
                        System.Diagnostics.Debug.WriteLine("Looking at row: " + j + ", col:" + i +": "+calendar[j,i]);
                        if (calendar[j,i] == 0 && calendar[j-1,i] != -1 && calendar[j + 1, i] != -1)
                        {
                            //if we need break (consecutiveAdded > 8) and we can take one (after break we have 45 min to next event), take break
                            if (consecutiveAdded >= 8 && calendar[j + 1, i] == 0 && calendar[j + 2, i] == 0 && calendar[j + 3, i] == 0)
                            {
                                //block out current block
                                calendar[j, i] = -1;
                                blocksFree--;

                                foreach(Task ta in TasksList)
                                {
                                    blocksFreeForEach[ta]--;
                                }

                                //move to next one (we know we have room for two!)
                                j++;
                            }
                            //add task block to calendar there, and update representation
                            int id = t.ID;
                            calendar[j, i] = id;
                            blocksFree--;

                            foreach (Task ta in TasksList)
                            {
                                blocksFreeForEach[ta]--;
                            }

                            blocksToSchedule--;
                            blocksLeft[t] = blocksLeft[t] - 1;
                            System.Diagnostics.Debug.WriteLine("Just placed " + t.Name + ". At row: " + j + ", col:" + i);

                            lastBlockPlaced = j;
                            lastDayPlaced = i;

                            //check consecutive added
                            if(calendar[j-1,i] > 0)
                            {
                                consecutiveAdded++;
                            }
                            else
                            {
                                consecutiveAdded = 1;
                            }

                            //break
                            j = blocksInDay;
                            i = daysToDoAll;

                            
                        }

                    }
                }
            }

            //clear existing PlannedTasks that need to be pulled
            await App.DB.DeleteAllPlannedAsync(recalcAll);
            App.MyPlanned.Clear();


            //create plannedTasks for all of the things in the working calendar
            int currTask = -1;
            for(int i = 0; i < daysToDoAll; i++)
            {
                int taskStartBlock = 0;
                Dictionary<int, int> blocksPerTask = new Dictionary<int, int>(); ;
                for (int j = 0; j < blocksInDay; j++)
                {
                    //start of task block
                    if (calendar[j,i] > 0 && currTask == -1)
                    {
                        currTask = calendar[j, i];
                        taskStartBlock = j;

                        blocksPerTask.Clear();

                    }

                    //inside block -- update task
                    if(calendar[j,i] > 0) 
                    {
                        if (blocksPerTask.ContainsKey(calendar[j, i]))
                        {
                            blocksPerTask[calendar[j, i]] = blocksPerTask[calendar[j, i]] + 1;
                        }
                        else
                        {
                            blocksPerTask[calendar[j, i]] = 1;
                        }

                    }
                    

                    //end of task -- PLAN IT!
                    if(calendar[j,i] <= 0 && currTask != -1)
                    {
                        foreach(int t in blocksPerTask.Keys)
                        {
                            int blocksReq = blocksPerTask[t];

                            Task futurePlanned = IDtoTask[t];
                            DateTime dayOfTask = dates[i];
                            TimeSpan start = IndexToTimeSpan(taskStartBlock);
                            TimeSpan end = IndexToTimeSpan(taskStartBlock+blocksReq);
                            System.Diagnostics.Debug.WriteLine("Just Planned: " + futurePlanned.Name + ". From: " + start + " to " + end);

                            CreatePlannedTask(futurePlanned, start, end, dayOfTask);

                            taskStartBlock += blocksReq;
                        }

                        currTask = -1;
                        taskStartBlock = 0;
                    }
                }
            }

            App.Current.Properties["needsRefresh"] = true;
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

        //adds a block with given start/end times into the calendar
        private int AddBlock(double startTime, double endTime, int[,] cal, int dayIndex)
        {
            //make sure block actually fits in the given sleep range
            int tempSleepHour = sleepHour;
            if (wakeHour > sleepHour)
            {
                tempSleepHour += 24;
            }
            double tempEndTime = endTime;
            if (endTime < startTime)
            {
                tempEndTime += 24;
            }

            int blocksPlaced = 0;
            if (startTime >= wakeHour && tempEndTime <= tempSleepHour) //falls within calendar range
            {
                //get appropriate indicies to schedule
                int startIndex = (int)Math.Floor((startTime - wakeHour) / 0.25);
                int endIndex = (int)Math.Ceiling((tempEndTime - wakeHour) / 0.25);

                for(int i = startIndex; i < endIndex; i++)
                {
                    cal[i, dayIndex] = -1;
                    blocksPlaced++;
                }
            }
            return blocksPlaced;
        }

        //returns an array corresponding to the days of the week for the planning calendar
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

        //returns an array corresponding to the dates of the planning calendar
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

        //returns which task is most pressing to do at the current junction based on:
        // most pressing task = task for which blocksleft[task]/blocksFree is largest
        private Task MostPressing(Dictionary<Task, int> blocksFree, Dictionary<Task,int> blocksLeft)
        {
            float worstRatio = 0;
            Task mostPressing = null;

            foreach(Task t in blocksLeft.Keys)
            {
                float currRatio = (float)blocksLeft[t] / (float)blocksFree[t];

                if(currRatio > worstRatio)
                {
                    worstRatio = currRatio;
                    mostPressing = t;
                }
            }

            return mostPressing;
        }

        //from the wakeup time and the calendar index, get the eqivalent timespan
        private TimeSpan IndexToTimeSpan(int calendarIndex)
        {
            int additonalHour = calendarIndex/4;
            int additionalMin = (calendarIndex % 4) * 15;
            TimeSpan toReturn = new TimeSpan(wakeHour + additonalHour, additionalMin, 0);

            return toReturn;
        }

        //creates a planned task given the information, and adds it to the database
        async private void CreatePlannedTask(Task parent, TimeSpan startTime, TimeSpan endTime, DateTime date)
        {
            PlannedTask toAdd = new PlannedTask();
            toAdd.ParentID = parent.ID;
            toAdd.UserModified = false;
            toAdd.Name = "DO: " + parent.Name;
            toAdd.StartTime = startTime;
            toAdd.PrevStartTime = startTime;
            toAdd.EndTime = endTime;
            toAdd.PrevEndTime = endTime;
            toAdd.Date = date;

            await App.DB.SavePlannedAsync(toAdd);
            App.MyPlanned.Add(toAdd);
        }

        //when called, looks through all planned tasks, deletes any which have passed, and updates hoursLeft on its parent task accordingly
        async public void UpdateTasks()
        {
            //get all Planned Tasks
            List<PlannedTask> allPlanned = await App.DB.GetPlannedAsync();

            //go through each planned Task, check if we have gone past it
            foreach(PlannedTask pt in allPlanned)
            {
                //checks if now is later than the end of the plannedTask
                DateTime plannedTaskEnd = pt.Date.Add(pt.EndTime);
                if (DateTime.Compare(DateTime.Now,plannedTaskEnd) > 0)
                {
                    Task parent = await App.DB.GetTaskAsync(pt.ParentID);

                    if (parent != null)
                    {
                        parent.HoursLeft = parent.HoursLeft - (float)pt.EndTime.Subtract(pt.StartTime).TotalHours;

                        if(parent.HoursLeft <= 0)
                        {
                            await App.DB.DeleteTaskAsync(parent);
                        }
                        else
                        {
                            await App.DB.SaveTaskAsync(parent);
                        }
                    }
                    
                    await App.DB.DeletePlannedAsync(pt);
                }
            }
            System.Diagnostics.Debug.WriteLine("BRO IM KINDA FUCKIN DONE");
        }

    }
}
