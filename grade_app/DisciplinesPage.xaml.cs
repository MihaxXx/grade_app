using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace grade_app
{
    public partial class DisciplinesPage : ContentPage
    {
        public DisciplinesPage()
        {
            InitializeComponent();

            var layout = new StackLayout { Padding = new Thickness(5, 10) };
            var label = new Label { Text = App.API.StudentGetDisciplines().ToString()};
            layout.Children.Add(label);
            this.Content = layout;
        }
    }
}
