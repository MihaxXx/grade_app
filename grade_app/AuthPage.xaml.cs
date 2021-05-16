using System;
using System.Collections.Generic;
using System.Web;
using Xamarin.Forms;

namespace grade_app
{
    public partial class AuthPage : ContentPage
    {
        string login;
        static string URL = "https://openid.sfedu.ru/server.php?openid.return_to=http%3A%2F%2Fgrade.sfedu.ru%2Fhandler%2Fsign%2Fopenidfinish%3Fuser_role%3Dstudent&openid.mode=checkid_setup&openid.identity=https%3A%2F%2Fopenid.sfedu.ru%2Fserver.php%2Fidpage%3Fuser%3Duseruser&openid.trust_root=http%3A%2F%2Fgrade.sfedu.ru&openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.claimed_id=https%3A%2F%2Fopenid.sfedu.ru%2Fserver.php%2Fidpage%3Fuser%3Duseruser&openid.realm=http%3A%2F%2Fgrade.sfedu.ru&openid.ns.sreg=http://openid.net/extensions/sreg/1.1&openid.sreg.optional=email%2Cnickname%2Cr61globalkey%2Cstaff%2Cstudent%2Cr61studentid&";
        Uri newUri;
        Uri newRedirectUri;
        public AuthPage(string name)
        {
            InitializeComponent();

            login = name;
            Uri OpenIDuri = new Uri(URL);
            var newuriB = new UriBuilder(OpenIDuri.Scheme, OpenIDuri.Host);
            newuriB.Path = OpenIDuri.AbsolutePath;
            var qs = HttpUtility.ParseQueryString(OpenIDuri.Query);
            qs.Set("openid.identity", qs["openid.identity"].Replace("useruser", login));
            qs.Set("openid.claimed_id", qs["openid.claimed_id"].Replace("useruser", login));
            //TODO: change redirect URLs
            newRedirectUri = new Uri(qs["openid.trust_root"]);
            qs.Set("openid.trust_root", qs["openid.trust_root"]);
            newuriB.Query = qs.ToString();
            webView.Source = newUri = newuriB.Uri;
        }
        async void webviewNavigating(object sender, WebNavigatingEventArgs e)
        {
            if (new Uri(e.Url).Host == newRedirectUri.Host)
            {
                Navigation.InsertPageBefore(new StudentIndexPage(), this);
                await Navigation.PopAsync();
            }
        }
    }
}
