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

		public List<DisciplineItem> DisciplineItems { get; private set; }
		public StudentIndexPage()
		{
			InitializeComponent();

			studentIndex = App.API.StudentGetIndex();
			Title = $"БРС - {SemesterFromDiscipline(studentIndex.Disciplines.First())}";
			DisciplineItems = new List<DisciplineItem>();
			foreach (var d in studentIndex.Disciplines)
			{
				var percent = (d.MaxCurrentRate != 0 ? ((d.Rate == null) ? 0 : d.Rate) / (double)d.MaxCurrentRate : 0).Value.ToString("P0");
				DisciplineItems.Add(new DisciplineItem(d.Id, percent, d.SubjectName, $"{((d.Rate == null) ? 0 : d.Rate)}/{d.MaxCurrentRate}/100"));
			}

			//Must be at the end!!!
			BindingContext = this;
		}

		private string SemesterFromDiscipline(Discipline discipline) => $"{(discipline.SemesterNum == 1 ? "Осень" : "Весна")} {discipline.SemesterYear}";

		private async void OnListItemTapped(object sender, ItemTappedEventArgs e)
		{
			((ListView)sender).SelectedItem = null;
			var item = (DisciplineItem)e.Item;
			await Navigation.PushAsync(new StudentDisciplinePage(item.ID));
			//Navigation.InsertPageBefore(new StudentDisciplinePage(item.ID), this);
		}
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
