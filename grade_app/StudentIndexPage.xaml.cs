using System;
using System.Collections.Generic;
using System.Linq;
using Grade;

using Xamarin.Forms;

namespace grade_app
{
	public partial class StudentIndexPage : ContentPage
	{
		StudentIndex studentIndex;

		public List<DisciplineItem> Ditems { get; private set; }
		public StudentIndexPage()
		{
			InitializeComponent();

			studentIndex = App.API.StudentGetIndex();
			Title = $"БРС - {SemesterFromDiscipline(studentIndex.Disciplines.First())}";
			Ditems = new List<DisciplineItem>();
			foreach (var d in studentIndex.Disciplines)
			{
				Ditems.Add(new DisciplineItem(d.Id, (d.MaxCurrentRate != 0 ? ((d.Rate==null)? 0: d.Rate) / d.MaxCurrentRate : 0).ToString() + "%", d.SubjectName, $"{((d.Rate == null) ? 0 : d.Rate)}/{d.MaxCurrentRate}/100"));
			}

			/*var layout = new StackLayout { Padding = new Thickness(5, 10) };
            var label = new Label { Text = App.API.StudentGetIndex().ToString()};
            //var label = new Label { Text = App.API.StudentGetDiscipline(30962).ToString()};
            layout.Children.Add(label);
            this.Content = layout;*/

			//Must be at the end!!!
			BindingContext = this;
		}

		private string SemesterFromDiscipline(Discipline discipline) => $"{(discipline.SemesterNum == 1 ? "Осень" : "Весна")} {discipline.SemesterYear}";
	}
	public class DisciplineItem
	{
		public long ID { get; set; }
		public string Percent { get; set; }
		public string Name { get; set; }
		public string Rate { get; set; }
		public DisciplineItem(long id, string percent, string name, string rate)
		{
			ID = id;
			Percent = percent;
			Name = name;
			Rate = rate;
		}
	}
}
