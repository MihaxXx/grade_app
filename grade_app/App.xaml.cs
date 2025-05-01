using System;
using Microsoft.Maui.Controls.Xaml;
using Grade;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Networking;

namespace grade_app
{
    public partial class App : Application
    {
        public static Grade.API API;
		private static readonly string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppSettings.json");
		private static NetworkAccess NetworkAccess;
		private static IEnumerable<ConnectionProfile> ConnectionProfiles;

		public App()
		{
			InitializeComponent();
			NetworkAccess = Connectivity.NetworkAccess;
			ConnectionProfiles = Connectivity.ConnectionProfiles;
			Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

			if (IsInternetConnected())
				OpenIndexOrLoginPage();
			else
				MainPage = new NoInternetPage();
		}

		internal void OpenIndexOrLoginPage()
		{
#if LOCAL
			if (true)
			{
				InitUser("token1", Role.Teacher);
				//InitUser("token2", Role.Student);*/
			/*if (File.Exists(_fileName))
			{
				var appSettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(_fileName, System.Text.Encoding.UTF8));
				InitUser(appSettings.token, appSettings.role);*/
#elif DEV_RATING
            if (File.Exists(_fileName))
			{
				var appSettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(_fileName, System.Text.Encoding.UTF8));
                InitUser(appSettings.token, appSettings.role);
#else
			if (File.Exists(_fileName))
			{
				var appSettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(_fileName, System.Text.Encoding.UTF8));
				InitUser(appSettings.token, appSettings.role);
#endif
				if (API.role == Role.Student)
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
		void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
		{
			NetworkAccess = e.NetworkAccess;
			ConnectionProfiles = e.ConnectionProfiles;
		}

		internal static bool IsInternetConnected() => NetworkAccess == NetworkAccess.Internet &&
			ConnectionProfiles.Count(cp => cp == ConnectionProfile.Cellular || cp == ConnectionProfile.WiFi || cp == ConnectionProfile.Ethernet) > 0;

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
