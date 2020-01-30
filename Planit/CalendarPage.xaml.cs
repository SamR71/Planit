using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Planit.Models;

namespace Planit
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarPage : ContentPage
    {
        private DateTime shownDate = DateTime.Today;
        private Label dateLabel;
        private Grid dayPlan;
        private List<View> loadedEvents;
        private int wakeHour; //the first time in the calendar (mod 24)
        private int sleepHour; // time the calendar goes to

        public CalendarPage()
        {
            this.Title = "My Plan";

            StackLayout mainStack = new StackLayout
            {
                Orientation = StackOrientation.Vertical
            };

            ///////
            //  Everything Pertaining to the DateBar
            ///////

            Grid dateBar = new Grid
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(2,GridUnitType.Star)},
                    new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star)},
                    new ColumnDefinition { Width = new GridLength(4,GridUnitType.Auto)},
                    new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star)},
                    new ColumnDefinition { Width = new GridLength(2,GridUnitType.Star)}
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(50, GridUnitType.Absolute)}
                }
            };

            dateLabel = new Label 
            { 
                Text = shownDate.ToShortDateString(),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center 
            };
            dateBar.Children.Add(dateLabel, 2, 0);

            Button forwardButton = new Button
            {
                Text = ">",
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            forwardButton.Clicked += ForwardButton_Clicked;
            Button backwardButton = new Button
            {
                Text = "<",
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            backwardButton.Clicked += BackwardButton_Clicked;
            Button todayButton = new Button
            {
                Text = "Today",
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            todayButton.Clicked += TodayButton_Clicked;

            dateBar.Children.Add(todayButton, 0, 0);
            dateBar.Children.Add(backwardButton, 1,0);
            dateBar.Children.Add(forwardButton, 3, 0);

            ///////
            //  Everything Pertaining to the main Grid
            ///////
            ScrollView scroller = new ScrollView();


            //get wake up time and bedtime
            // use that to get hours in day
            long defaultBedTime = 0;
            var sleep = Preferences.Get("bedtime", defaultBedTime);

            long defaultWakeTime = 288000000000; //8 am in ticks
            var wake = Preferences.Get("waketime", defaultWakeTime);

            int wakeUpTime = (int)((long)wake/ 36000000000);
            int bedTime = (int)Math.Ceiling((double)sleep / (double)36000000000);

            int hoursInDay;
            if(wakeUpTime > bedTime)
            {
                hoursInDay = (24 + bedTime) - wakeUpTime;
            }
            else
            {
                hoursInDay = bedTime - wakeUpTime;
            }

            //set first and last hour in calendar
            wakeHour = wakeUpTime;
            sleepHour = bedTime;

            Grid grid = new Grid
            {
                //VerticalOptions = LayoutOptions.Fill,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(9, GridUnitType.Star) }
                },
                RowSpacing = 0,
                ColumnSpacing = 0
            };

            //initialize rows and borders for hour in day
            for (int i = 0; i < hoursInDay; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Absolute) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12, GridUnitType.Absolute) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12, GridUnitType.Absolute) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Absolute) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12, GridUnitType.Absolute) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12, GridUnitType.Absolute) });
            }

            //create 1 pixel wide borders as required, and add times
            grid.Children.Add(new BoxView { Color = Color.Silver }, 1, 2, 0, (hoursInDay) * 6);

            for (int i = 0; i < hoursInDay; i++)
            {
                //figure out which hour planner represents
                int currHour = (wakeUpTime + i) % 12;
                if (currHour == 0)
                    currHour = 12;

                grid.Children.Add(new Label {Text = currHour.ToString(), HorizontalOptions = LayoutOptions.Center }, 0, 1, (6 * i) + 1, (6 * i) + 3);

                grid.Children.Add(new BoxView { Color = Color.Silver }, 0, 3, i * 6, (i * 6) + 1);
                grid.Children.Add(new BoxView { Color = Color.Silver }, 1, 3, (i * 6)+3, (i * 6) + 4);
            }

            dayPlan = grid;

            ///////
            //  Putting Everything Together
            ///////

            mainStack.Children.Add(dateBar);

            scroller.Content = grid;

            mainStack.Children.Add(scroller);
            Content = mainStack;

            loadedEvents = new List<View>();

            LoadDayEvents(shownDate);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        //load all Events and PlannedTasks, and add them to the calendar
        private async void LoadDayEvents(DateTime day)
        {
            DayOfWeek dayOfWeek = day.DayOfWeek;
            List<Event> EventsList = await App.DB.GetEventsAsync();
            List<PlannedTask> PlannedList = await App.DB.GetPlannedAsync();

            //remove all previously loaded events
            foreach(View v in loadedEvents)
            {
                dayPlan.Children.Remove(v);
            }
            loadedEvents.Clear();

            foreach(Event e in EventsList)
            {
                //check if event applies to current day
                if(e.EventType == Event.Type.OneTime && e.Date == day)
                {
                    AddEntry(e.StartTime, e.EndTime, e.Name, Color.PowderBlue);
                }
                else
                {
                    if (OnToday(dayOfWeek, e))
                    {
                        AddEntry(e.StartTime,e.EndTime,e.Name,Color.PowderBlue);
                    }
                }
            }

            foreach(PlannedTask pt in PlannedList)
            {
                if(pt.Date == day)
                {
                    AddEntry(pt.StartTime,pt.EndTime,pt.Name,Color.CornflowerBlue);
                }
            }
        }


        private void AddEntry(TimeSpan startTime, TimeSpan endTime, String name, Color color)
        {
            //make sure Event actually fits in the given sleep range
            double taskStart = startTime.TotalHours;
            double taskEnd = endTime.TotalHours;

            int tempSleepHour = sleepHour;
            if (wakeHour > sleepHour)
            {
                tempSleepHour += 24;
            }
            double tempEventEnd = taskEnd;
            if (taskEnd < taskStart)
            {
                tempEventEnd += 24;
            }

            if (taskStart >= wakeHour && tempEventEnd <= tempSleepHour) //falls within calendar range
            {
                //create the button
                Button placedEvent = new Button
                {
                    BackgroundColor = color,
                    TextColor = Color.Black,
                    CornerRadius = 15,
                    Text = name,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(2, 2, 2, 2),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand
                };

                //get appropriate indicies to schedule
                int startIndex = (int)Math.Floor((taskStart - wakeHour) / 0.25) + 1 + (int)Math.Floor(2 * (taskStart - wakeHour));
                int endIndex = (int)Math.Ceiling((tempEventEnd - wakeHour) / 0.25) + (int)Math.Ceiling(2 * (tempEventEnd - wakeHour));

                dayPlan.Children.Add(placedEvent, 2, 3, startIndex, endIndex);
                loadedEvents.Add(placedEvent);
            }
        }

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

        private void TodayButton_Clicked(object sender, EventArgs e)
        {
            shownDate = DateTime.Today;
            dateLabel.Text = shownDate.ToShortDateString();
            LoadDayEvents(shownDate);
        }

        private void ForwardButton_Clicked(object sender, EventArgs e)
        {
            shownDate = shownDate.AddDays(1);
            dateLabel.Text = shownDate.ToShortDateString();
            LoadDayEvents(shownDate);
        }

        private void BackwardButton_Clicked(object sender, EventArgs e)
        {
            shownDate = shownDate.AddDays(-1);
            dateLabel.Text = shownDate.ToShortDateString();
            LoadDayEvents(shownDate);
        }
    }
}