using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace Planit
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SleepPage : ContentPage
    {
        public SleepPage()
        {
            InitializeComponent();

            //load bedtime from preferences
            long defaultBedtime = 0;
            var bed = Preferences.Get("bedtime", defaultBedtime);
            bedtimesetter.Time = new TimeSpan((long)bed);

            //load waketime from preferences
            long defaultWaketime = 288000000000;
            var wake = Preferences.Get("waketime", defaultWaketime); //the big number is 8 hrs in Ticks

            wakeupsetter.Time = new TimeSpan((long)wake);

        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            long saveBedTime = bedtimesetter.Time.Ticks;
            long saveWakeTime = wakeupsetter.Time.Ticks;

            Preferences.Set("bedtime", saveBedTime);
            Preferences.Set("waketime", saveWakeTime);

            App.TP.PlanTasks(true);
            App.Current.MainPage = new MainPage();

        }

        async private void Logout_Button_Clicked(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}