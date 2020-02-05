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
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        async private void Login_Button_Clicked(object sender, EventArgs e)
        {
            bool isValidLogin = TryLogin(emailForm.Text, passwordForm.Text);

            if (isValidLogin)
            {
                Navigation.InsertPageBefore(new MainPage(), this);
                await Navigation.PopAsync();
            }
            else
            {
                invalidLoginLabel.IsVisible = true;
            }
        }

        //obviously placeholder
        private bool TryLogin(string email, string pass)
        {
            if(email == null && pass == null)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        async private void Signup_Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignupPage());
        }
    }
}