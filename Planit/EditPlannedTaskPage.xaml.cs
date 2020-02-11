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
    public partial class EditPlannedTaskPage : ContentPage
    {
        public EditPlannedTaskPage()
        {
            InitializeComponent();
        }

        async private void Save_Button_Clicked(object sender, EventArgs e)
        {
            var PlannedTask = (PlannedTask)BindingContext;
            PlannedTask.UserModified = true;

            await App.DB.SavePlannedAsync(PlannedTask);

            foreach(PlannedTask pt in App.MyPlanned)
            {
                if(pt.ID == PlannedTask.ID)
                {
                    App.MyPlanned.Remove(pt);
                    App.MyPlanned.Add(PlannedTask);
                    break;
                }
            }

            App.TP.PlanTasks(false);
            App.Current.Properties["needsRefresh"] = true;
            await Navigation.PopAsync();

        }

        async private void Delete_Button_Clicked(object sender, EventArgs e)
        {
            var PlannedTask = (PlannedTask)BindingContext;
            PlannedTask.UserModified = true;

            //set both start and end time to 0
            PlannedTask.StartTime = new TimeSpan(0);
            PlannedTask.EndTime = new TimeSpan(0);

            await App.DB.SavePlannedAsync(PlannedTask);

            foreach (PlannedTask pt in App.MyPlanned)
            {
                if (pt.ID == PlannedTask.ID)
                {
                    App.MyPlanned.Remove(pt);
                    App.MyPlanned.Add(PlannedTask);
                    break;
                }
            }

            App.TP.PlanTasks(false);
            App.Current.Properties["needsRefresh"] = true;
            await Navigation.PopAsync();
        }
    }
}