using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace grade_app
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StudentDisciplinePage : ContentPage
	{
		public StudentDisciplinePage()
		{
			InitializeComponent();

			var layout = new StackLayout { Padding = new Thickness(5, 10) };
			//var label = new Label { Text = App.API.StudentGetIndex().ToString()};
			var label = new Label { Text = App.API.StudentGetDiscipline(30962).ToString() };
			layout.Children.Add(label);
			this.Content = layout;
		}
	}
}