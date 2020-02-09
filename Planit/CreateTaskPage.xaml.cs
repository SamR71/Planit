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

    public partial class CreateTaskPage : ContentPage
    {
        public CreateTaskPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var Task = (Task)BindingContext;

            if (Task.Name != null) //task is being edited
            {
                numberpicker.Text = Task.HoursLeft.ToString();
                deletebutton.IsVisible = true;
                Title = "Edit Task";
            }
            else
            {
                duedate.Date = DateTime.Today;
            }
        }

        async private void Save_Button_Clicked(object sender, EventArgs e)
        {
            var Task = (Task)BindingContext;

            if(numberpicker.Text != null)
            {
                Task.HoursLeft = float.Parse(numberpicker.Text);
            }

            await App.DB.SaveTaskAsync(Task);
            App.TP.PlanTasks(true);
            App.Current.Properties["needsRefresh"] = true;
            await Navigation.PopAsync();
        }

        async private void Delete_Button_Clicked(object sender, EventArgs e)
        {
            var Task = (Task)BindingContext;
            await App.DB.DeleteTaskAsync(Task);
            App.TP.PlanTasks(true);
            App.Current.Properties["needsRefresh"] = true;
            await Navigation.PopAsync();
        }
    }
}