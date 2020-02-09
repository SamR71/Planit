using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using Xamarin.Essentials;

namespace Planit
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            App.Current.Properties["SessionID"] = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var savedEmail = Preferences.Get("savedEmail", null);
            var savedPassword = Preferences.Get("savedPassword", null);

            if (savedEmail != null && savedPassword != null)
            {
                System.Diagnostics.Debug.WriteLine("Auto logging in");
                TryLogin(savedEmail, savedPassword);
            }
        }

        private void Login_Button_Clicked(object sender, EventArgs e)
        {
            TryLogin(emailForm.Text, passwordForm.Text);
        }

        async private void TryLogin(string email, string password)
        {
            var myHttpClient = new HttpClient();
            var uri = new Uri(Constants.loginURL);

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"email",email},
                    {"password", password }
                });

            var response = myHttpClient.PostAsync(uri.ToString(), formContent).Result;

            string res = await response.Content.ReadAsStringAsync();


            if (res != "Failure: Invalid Email or Password")
            {
                App.Current.Properties["SessionID"] = res;
                Preferences.Set("savedEmail", emailForm.Text);
                Preferences.Set("savedPassword", passwordForm.Text);
                Navigation.InsertPageBefore(new MainPage(), this);
                await Navigation.PopAsync();
            }
            else
            {
                invalidLoginLabel.IsVisible = true;
            }
        }


        async private void Signup_Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignupPage());
        }
    }
}