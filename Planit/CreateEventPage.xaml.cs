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
        private bool isRecurring;
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
                isRecurring = true;
                eventType = Event.Type.Recurring;
                eventdate.Date = DateTime.Today;
            }
            else //event is being edited
            {
                Title = "Edit Event";
                deletebutton.IsVisible = true;

                if (Event.EventType == Event.Type.OneTime)
                {
                    isRecurring = false;
                    ReccuringSwitch.IsToggled = true;
                }
                else
                {
                    isRecurring = true;
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
            datepicker.IsVisible = !datepicker.IsVisible;
            daystag.IsVisible = !daystag.IsVisible;
            daysgrid.IsVisible = !daysgrid.IsVisible;

            if (isRecurring)
            {
                eventType = Event.Type.OneTime;
            }
            else
            {
                eventType = Event.Type.Recurring;
            }
        }

        async private void Save_Button_Clicked(object sender, EventArgs e)
        {
            var Event = (Event)BindingContext;
            Event.OnMon = MonCheck.IsChecked;
            Event.OnTue = TueCheck.IsChecked;
            Event.OnWed = WedCheck.IsChecked;
            Event.OnThu = ThuCheck.IsChecked;
            Event.OnFri = FriCheck.IsChecked;
            Event.OnSat = SatCheck.IsChecked;
            Event.OnSun = SunCheck.IsChecked;
            Event.EventType = eventType;

            await App.DB.SaveEventAsync(Event);
            App.TP.PlanTasks(true);
            await Navigation.PopAsync();
        }

        async private void Delete_Button_Clicked(object sender, EventArgs e)
        {
            var Event = (Event)BindingContext;
            await App.DB.DeleteEventAsync(Event);
            App.TP.PlanTasks(true);
            await Navigation.PopAsync();

        }
    }
}