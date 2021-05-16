using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace grade_app
{
    public partial class StudentIndexPage : ContentPage
    {
        public StudentIndexPage()
        {
            InitializeComponent();

            var layout = new StackLayout { Padding = new Thickness(5, 10) };
            var label = new Label { Text = App.API.StudentGetIndex().ToString()};
            //var label = new Label { Text = App.API.StudentGetDiscipline(30962).ToString()};
            layout.Children.Add(label);
            this.Content = layout;
        }
    }
}
