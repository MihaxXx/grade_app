using Grade;

namespace grade_app
{
    public partial class AuthPage : ContentPage
    {
        int PassedRedirect = 0;
        string state = "";
        bool changedRole = false;
        readonly string Host;

        public AuthPage(string name, string userrole)
        {
            InitializeComponent();
#if DEV_RATING 
            Host = "dev.rating.mmcs.sfedu.ru";
            WebView.Source = $"https://login.microsoftonline.com/sfedu.ru/oauth2/v2.0/authorize?response_type=code&client_id=b88a652d-a4f2-4fb2-a953-05e6ee024a59&redirect_uri=https%3A%2F%2Fdev.rating.mmcs.sfedu.ru%2F%7Edev_rating%2Fhandler%2Fsign%2Foauthfinish&scope=offline_access+email+profile+openid+User.Read+Directory.Read.All&state={userrole}&login_hint={name}%40sfedu.ru";
#else
            Host = "grade.sfedu.ru";
            WebView.Source = $"https://login.microsoftonline.com/sfedu.ru/oauth2/v2.0/authorize?response_type=code&client_id=413637be-3e0d-48a0-a257-5e01c3d6e5f1&redirect_uri=https%3A%2F%2Fgrade.sfedu.ru%2Fhandler%2Fsign%2Foauthfinish&scope=offline_access+email+profile+openid+User.Read+Directory.Read.All&state={userrole}&login_hint={name}%40sfedu.ru";
#endif
        }
        void WebviewNavigating(object sender, WebNavigatingEventArgs e)
        {
#if LOCAL
            if (e.Url.StartsWith($"https://{Host}"))
                ((WebView)sender).Source = e.Url.Replace($"https://{Host}", $"http://{Host}/~dev_rating");
#else
            if (e.Url.StartsWith($"http://{Host}"))
                ((WebView)sender).Source = e.Url.Replace("http://", "https://");
#endif
#if LOCAL
            if (e.Url.StartsWith($"http://{Host}"))
#else
            if (e.Url.StartsWith($"https://{Host}"))
#endif
            {
                if (e.Url.Contains("oauthfinish"))
                {
                    var stateIdx = e.Url.IndexOf("state=") + 6;
                    state = e.Url[stateIdx..e.Url.IndexOf('&', stateIdx)];
                    PassedRedirect++;
                }
                else if (PassedRedirect > 0 && !e.Url.EndsWith("authtokenget"))
                {
#if DEV_RATING
                    ((WebView)sender).Source = $"https://{Host}/~dev_rating/{(state == "student" ? "student" : "teacher")}/authtokenget";
#elif LOCAL
                    ((WebView)sender).Source = $"http://{Host}/~dev_rating/{(state == "student" ? "student" : "teacher")}/authtokenget";
#else
                    ((WebView)sender).Source = $"https://{Host}/{(state == "student" ? "student" : "teacher")}/authtokenget";
#endif
                }
            }
        }

        async void WebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Url.EndsWith("authtokenget") && e.Result != WebNavigationResult.Failure)
            {
                var token = await ((WebView)sender).EvaluateJavaScriptAsync("document.body.getElementsByTagName('p')[0].innerText");
                if (token != null && token.Length == 40)
                {
                    if (changedRole) //if got token after role change then save changed role
                        state = (state == "student") ? "staff" : "student";
                    App.InitUser(token, state == "student" ? Role.Student : Role.Teacher);
                    if (App.API.role == Role.Student)
                        Navigation.InsertPageBefore(new StudentIndexPage(), Navigation.NavigationStack[0]);
                    else
                        Navigation.InsertPageBefore(new TeacherIndexPage(), Navigation.NavigationStack[0]);
                    await Navigation.PopToRootAsync();
                }
                else //if token is broken, try another role
                {
                    if (!changedRole)
                    {
                        changedRole = true;
                        await App.DisplayToastAsync($"Ваша роль была изменена с {state} на {(state == "student" ? "teacher" : "student")}");
#if DEV_RATING
                        ((WebView)sender).Source = $"https://{Host}/~dev_rating/{ (state == "student" ? "teacher" : "student") }/authtokenget";
#elif LOCAL
                        ((WebView)sender).Source = $"http://{Host}/~dev_rating/{ (state == "student" ? "teacher" : "student") }/authtokenget";
#else
                        ((WebView)sender).Source = $"https://{Host}/{(state == "student" ? "teacher" : "student")}/authtokenget";
#endif                        
                    }
                    else //if alreday tried another role, return to MainPage
                    {
                        await DisplayAlert("Ошибка", "Не удалось получить токен доступа. Проверьте верность введёных данных: Email, пароль, роль.", "ОК");
                        await Navigation.PopToRootAsync();
                    }

                }
            }
        }
    }
}
