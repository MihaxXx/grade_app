using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Grade;
using System.IO;
using Newtonsoft.Json;


namespace grade_app
{
    public partial class App : Application
    {
        public static Grade.API API;
        private static readonly string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppSettings.json");

        public App()
        {
            InitializeComponent();

            if (File.Exists(_fileName))
            {
                var appSettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(_fileName, System.Text.Encoding.UTF8));
                InitUser(appSettings.token, appSettings.role);

                if(API.role == Role.Student)
                    MainPage = new NavigationPage(new StudentIndexPage());
                else
                    MainPage = new NavigationPage(new TeacherIndexPage());
            }
            else
                MainPage = new NavigationPage(new MainPage());
        }
        public static void InitUser(string _token,Role _role)
		{
            API = new API(_token, _role);
            var appSettings = new AppSettings(_token, _role);
            File.WriteAllText(_fileName, JsonConvert.SerializeObject(appSettings, Formatting.Indented), System.Text.Encoding.UTF8);
        }

        public static void WipeUser()
		{
            API = null;
            File.Delete(_fileName);
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
