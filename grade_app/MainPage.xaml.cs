using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace grade_app
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async private void Enter_btnClicked(object sender, EventArgs e)
        {
            if (login.Text != null && (login.Text.Contains("@sfedu.ru") || !login.Text.Contains('@')))
                await Navigation.PushAsync(new AuthPage(login.Text.Contains(@"@sfedu.ru") ? login.Text.Substring(0, login.Text.IndexOf('@')) : login.Text, UserRole.SelectedItem.ToString() == "Student" ? "student" : "staff"));
            else
                await DisplayAlert("Wrong email", "Email must be @sfedu.ru", "OK");
        }
    }


}
