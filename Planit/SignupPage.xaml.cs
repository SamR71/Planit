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
    public partial class SignupPage : ContentPage
    {
        public SignupPage()
        {
            InitializeComponent();
        }

        //placeholder obviously, more stuff needed
        async private void Signup_Button_Clicked(object sender, EventArgs e)
        {
            if(passwordForm.Text == confirmForm.Text)
            {
                await Navigation.PopAsync();
            }
            else
            {
                invalidSignupLabel.IsVisible = true;
            }
        }
    }
}