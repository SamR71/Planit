using Planit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Planit
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EventsPage : ContentPage
    {
        public EventsPage()
        {
            InitializeComponent();
        }

        async private void OnCreateEvent(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateEventPage
            {
                BindingContext = new Event()
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            List<Event> EventsList = await App.DB.GetEventsAsync();

            if(EventsList != null)
            {
                eventsList.ItemsSource = EventsList;
                eventSuggestionLabel.IsVisible = false;
            }

        }

        async private void eventsList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                await Navigation.PushAsync(new CreateEventPage
                {
                   BindingContext = e.SelectedItem as Event
                });
            }
        }
    }
}