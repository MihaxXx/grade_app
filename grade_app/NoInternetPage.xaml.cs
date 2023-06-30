using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace grade_app
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NoInternetPage : ContentPage
	{
		public NoInternetPage()
		{
			InitializeComponent();
		}

		private void Button_Pressed(object sender, EventArgs e)
		{
			if (App.IsInternetConnected())
				((App)App.Current).OpenIndexOrLoginPage();
        }
    }
}