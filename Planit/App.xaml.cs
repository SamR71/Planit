using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Planit.Data;
using System.Collections.Generic;
using Planit.Models;

namespace Planit
{
    public partial class App : Application
    {
        static Database db;
        static TaskPlanner tp;
        static List<Event> myevents;
        static List<Task> mytasks;
        static List<PlannedTask> myplanned;

        public static Database DB
        {
            get
            {
                if(db == null)
                {
                    db = new Database(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "maindb.db3"));
                }
                return db;
            }
        }

        public static TaskPlanner TP
        {
            get
            {
                if(tp == null)
                {
                    tp = new TaskPlanner();
                }
                return tp;
            }
        }

        public static List<Event> MyEvents
        {
            get
            {
                return myevents;
            }
        }

        public static List<Task> MyTasks
        {
            get
            {
                return mytasks;
            }
        }

        public static List<PlannedTask> MyPlanned
        {
            get
            {
                return myplanned;
            }
        }

        public App()
        {
            InitializeComponent();

            NavigationPage main = new NavigationPage(new LoginPage());

            MainPage = main;
        }

        protected override async void OnStart()
        {
            myevents = await DB.GetEventsAsync();
            mytasks = await DB.GetTasksAsync();
            myplanned = await DB.GetPlannedAsync();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            TP.UpdateTasks();
        }
    }
}
