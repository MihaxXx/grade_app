using System;
using System.Collections.Generic;
using System.Web;
using Xamarin.Forms;
using Grade;

namespace grade_app
{
    public partial class AuthPage : ContentPage
    {
        int PassedRedirect = 0;
        string state;
        public AuthPage(string name, string userrole)
        {
            InitializeComponent();
            webView.Source = $"https://login.microsoftonline.com/sfedu.ru/oauth2/v2.0/authorize?response_type=code&client_id=413637be-3e0d-48a0-a257-5e01c3d6e5f1&redirect_uri=https%3A%2F%2Fgrade.sfedu.ru%2Fhandler%2Fsign%2Foauthfinish&scope=offline_access+email+profile+openid+User.Read+Directory.Read.All&state={userrole}&login_hint={name}%40sfedu.ru";
        }
        async void webviewNavigating(object sender, WebNavigatingEventArgs e)
        {
#if DEBUG
            if (e.Url.StartsWith("https://grade.sfedu.ru"))
                ((WebView)sender).Source = e.Url.Replace("https://grade.sfedu.ru", "http://grade.sfedu.ru/~dev_rating");
#endif
            if (e.Url.StartsWith("http://grade.sfedu.ru"))
            {
                Title = "Grade.sfedu.ru Auth";
                if (PassedRedirect == 0)
                {
                    var stateIdx = e.Url.IndexOf("state=")+6;
                    state = e.Url.Substring(stateIdx, e.Url.IndexOf("&", stateIdx) - stateIdx);
                }
                else if (PassedRedirect == 1)
                {
#if DEBUG
                    ((WebView)sender).Source = "http://grade.sfedu.ru/~dev_rating/student/authtokenget";
#else
                    ((WebView)sender).Source = "https://grade.sfedu.ru/student/authtokenget";
#endif
                }

                PassedRedirect++;
            }

        }

		async void webView_Navigated(object sender, WebNavigatedEventArgs e)
		{
            if (e.Url.EndsWith("authtokenget"))
            {
                var token = await((WebView)sender).EvaluateJavaScriptAsync("document.body.getElementsByTagName('p')[0].innerText");
                App.InitUser(token, state == "student" ? Role.Student : Role.Teacher);
                //await Navigation.PushAsync(new StudentIndexPage());
                Navigation.InsertPageBefore(new StudentIndexPage(), this);
                await Navigation.PopAsync();
            }
        }
	}
}
