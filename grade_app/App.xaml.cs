using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Grade;

namespace grade_app
{
    public partial class App : Application
    {
        public static bool _isLoggedIn = true;
        public static string Token;
        public static Grade.API API;

        public App()
        {
            InitializeComponent();

            if (_isLoggedIn)
            {
                Token = "5a99d0da-de38-4c7c-99ad-1d749ee50eb8"; //Student
                //Token = "39se9832fh3e78fl23ois33mhfdff34gbuj34897"; //Teacher

                API = new API(Token);
                MainPage = new NavigationPage(new DisciplinesPage());
            }
            else
                MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
