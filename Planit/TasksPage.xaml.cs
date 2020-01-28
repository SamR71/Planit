using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Planit.Models;
using Task = Planit.Models.Task;

namespace Planit
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TasksPage : ContentPage
    {
        public TasksPage()
        {
            InitializeComponent();
        }

        async private void OnCreateTask(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateTaskPage
            {
                BindingContext = new Models.Task()
            });

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            List<Models.Task> TaskList = await App.DB.GetTasksAsync();

            if (TaskList != null)
            {
                taskList.ItemsSource = TaskList;
                taskSuggestionLabel.IsVisible = false;
            }

        }

        async private void taskList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                await Navigation.PushAsync(new CreateTaskPage
                {
                    BindingContext = e.SelectedItem as Task
                });
            }

        }
    }
}