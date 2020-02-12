using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Planit.Models;

namespace Planit
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateEventPage : ContentPage
    {
        private Event.Type eventType;
        public CreateEventPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var Event = (Event)BindingContext;

            if (Event.Name == null) //Event has not yet been created
            {
                eventType = Event.Type.Recurring;
                eventdate.Date = DateTime.Today;
            }
            else //event is being edited
            {
                Title = "Edit Event";
                deletebutton.IsVisible = true;

                if (Event.EventType == Event.Type.OneTime)
                {
                    ReccuringSwitch.IsToggled = true;
                }
                else
                {
                    MonCheck.IsChecked = Event.OnMon;
                    TueCheck.IsChecked = Event.OnTue;
                    WedCheck.IsChecked = Event.OnWed;
                    ThuCheck.IsChecked = Event.OnThu;
                    FriCheck.IsChecked = Event.OnFri;
                    SatCheck.IsChecked = Event.OnSat;
                    SunCheck.IsChecked = Event.OnSun;


                }
            }

        }

        private void ReccuringSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("why we doin this");
            datepicker.IsVisible = !datepicker.IsVisible;
            daystag.IsVisible = !daystag.IsVisible;
            daysgrid.IsVisible = !daysgrid.IsVisible;
        }

        async private void Save_Button_Clicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(eventType);
            var Event = (Event)BindingContext;
            Event.OnMon = MonCheck.IsChecked;
            Event.OnTue = TueCheck.IsChecked;
            Event.OnWed = WedCheck.IsChecked;
            Event.OnThu = ThuCheck.IsChecked;
            Event.OnFri = FriCheck.IsChecked;
            Event.OnSat = SatCheck.IsChecked;
            Event.OnSun = SunCheck.IsChecked;

            if (ReccuringSwitch.IsToggled)
            {
                eventType = Event.Type.OneTime;
            }
            else
            {
                eventType = Event.Type.Recurring;
            }


            Event.EventType = eventType;
            System.Diagnostics.Debug.WriteLine(Event.EventType);

            await App.DB.SaveEventAsync(Event);

            App.TP.PlanTasks(true);
            App.Current.Properties["needsRefresh"] = true;
            await Navigation.PopAsync();
        }

        async private void Delete_Button_Clicked(object sender, EventArgs e)
        {
            var Event = (Event)BindingContext;
            await App.DB.DeleteEventAsync(Event);

            App.TP.PlanTasks(true);
            App.Current.Properties["needsRefresh"] = true;
            await Navigation.PopAsync();

        }
    }
}