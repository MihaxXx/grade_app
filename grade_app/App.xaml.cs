using CommunityToolkit.Maui.Alerts;
using Grade;
using Newtonsoft.Json;

namespace grade_app
{
    public partial class App : Application
    {
        public static API API { get; private set; }
        private static readonly string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppSettings.json");
        private static NetworkAccess NetworkAccess = Connectivity.NetworkAccess;
        private static IEnumerable<ConnectionProfile> ConnectionProfiles = Connectivity.ConnectionProfiles;

        public App()
        {
            InitializeComponent();
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        ~App() => Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;

        internal static Page GetStartPage()
        {
            if (!IsInternetConnected())
                return new NoInternetPage();
#if LOCAL
			if (true)
			{
				InitUser("token1", Role.Teacher);
				//InitUser("token2", Role.Student);
			/*if (File.Exists(_fileName) && JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(_fileName, System.Text.Encoding.UTF8)) is { } appSettings)
			{
				InitUser(appSettings.token, appSettings.role);*/
#elif DEV_RATING
            if (File.Exists(_fileName) && JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(_fileName, System.Text.Encoding.UTF8)) is { } appSettings)
			{
                InitUser(appSettings.token, appSettings.role);
#else
            if (File.Exists(_fileName) && JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(_fileName, System.Text.Encoding.UTF8)) is { } appSettings)
            {
                InitUser(appSettings.token, appSettings.role);
#endif
                if (API.role == Role.Student)
                    return new NavigationPage(new StudentIndexPage());
                else
                    return new NavigationPage(new TeacherIndexPage());
            }
            else
                return new NavigationPage(new MainPage());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(GetStartPage());
        }

        public static void InitUser(string _token, Role _role)
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
        void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            NetworkAccess = e.NetworkAccess;
            ConnectionProfiles = e.ConnectionProfiles;
        }

        internal static bool IsInternetConnected() => NetworkAccess == NetworkAccess.Internet &&
            ConnectionProfiles.Any(cp => cp is ConnectionProfile.Cellular or ConnectionProfile.WiFi or ConnectionProfile.Ethernet);

        public static async Task DisplayToastAsync(string message)
        {
            var toast = Toast.Make(message, textSize: 18);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await toast.Show(cts.Token);
        }

    }
}
