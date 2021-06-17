using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Grade;

namespace grade_app
{
    public partial class App : Application
    {
        public static bool _isLoggedIn;// = true;
        public static string Token;
        public static Grade.API API;
        public static Role role;

        public App()
        {
            InitializeComponent();

            if (_isLoggedIn)
            {
                //InitUser("b89c7c3a-d570-42bc-8b49-285655133a1f", Role.Student);
                //InitUser("a506c94a-d28d-48fb-9361-96bcd3b8356d", Role.Teacher);
                if(role == Role.Student)
                    MainPage = new NavigationPage(new StudentIndexPage());
                else
                    MainPage = new NavigationPage(new TeacherIndexPage());
            }
            else
                MainPage = new NavigationPage(new MainPage());
        }
        public static void InitUser(string _token,Role _role)
		{
            Token = _token;
            role = _role;
            API = new API(Token, role);
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
