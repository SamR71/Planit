﻿using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Planit.Data;

namespace Planit
{
    public partial class App : Application
    {
        static Database db;

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

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
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
