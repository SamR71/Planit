using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        async private void Signup_Button_Clicked(object sender, EventArgs e)
        {
            if(passwordForm.Text == confirmForm.Text)
            {
                var myHttpClient = new HttpClient();
                var uri = new Uri(Constants.signupURL);

                var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"email",emailForm.Text},
                    {"password", passwordForm.Text }
                });

                var response = myHttpClient.PostAsync(uri.ToString(), formContent).Result;

                string res = await response.Content.ReadAsStringAsync();

                if(res == "Success: User Signed Up!")
                {
                    await Navigation.PopAsync();
                }
                else
                {
                    invalidSignupLabel.Text = res;
                    invalidSignupLabel.IsVisible = true;
                }
                
            }
            else
            {
                invalidSignupLabel.Text = "Error: Passwords do not Match";
                invalidSignupLabel.IsVisible = true;
            }
        }
    }
}