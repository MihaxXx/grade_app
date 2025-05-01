using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

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