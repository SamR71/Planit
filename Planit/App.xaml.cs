using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Planit.Data;

namespace Planit
{
    public partial class App : Application
    {
        static Database db;
        static TaskPlanner tp;

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


        public App()
        {
            InitializeComponent();

            NavigationPage main = new NavigationPage(new LoginPage());

            MainPage = main;
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
