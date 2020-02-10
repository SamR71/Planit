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

            App.TP.PlanTasks(false);
            App.Current.Properties["needsRefresh"] = true;
            await Navigation.PopAsync();

        }
    }
}