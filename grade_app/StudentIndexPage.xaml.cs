using System;
using System.Collections.Generic;
using System.Linq;
using Grade;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace grade_app
{
	public partial class StudentIndexPage : ContentPage
	{
		StudentIndex studentIndex;
		List<Semester> SemesterList { get; set; }

		public long CurrentSemID { get; set; }

		public ObservableCollection<DisciplineItem> DisciplineItems { get; private set; } = new ObservableCollection<DisciplineItem>();
		public StudentIndexPage()
		{
			InitializeComponent();

			LoadDisciplines(-1);
			LoadSemesters();
			//Must be at the end!!!
			BindingContext = this;
		}

		private void LoadSemesters()
		{
			SemesterList = App.API.GetSemesterList();
			foreach (var sem in SemesterList)
			{
				ToolbarItem item = new ToolbarItem
				{
					Text = $"{(sem.Num == 1 ? "Осень" : "Весна")} {(sem.Num==1?sem.Year:sem.Year+1)}",
					Order = ToolbarItemOrder.Secondary,
					Priority = (int)(SemesterList.Count - sem.Id + 1),
					CommandParameter = sem.Id,
				};
				item.Clicked += OnToolbarItemClicked;
				this.ToolbarItems.Add(item);
			}
		}

		private void LoadDisciplines(long SemesterID)
		{
			studentIndex = App.API.StudentGetIndex(SemesterID);
			//TODO: Refactor
			CurrentSemID = studentIndex.Disciplines.First().SemesterId;
			Title = $"БРС - {SemesterFromDiscipline(studentIndex.Disciplines.First())}";
			DisciplineItems.Clear();
			foreach (var d in studentIndex.Disciplines)
			{
				var percent = (d.MaxCurrentRate != 0 ? ((d.Rate == null) ? 0 : d.Rate) / (double)d.MaxCurrentRate : 0).Value.ToString("P0");
				DisciplineItems.Add(new DisciplineItem(d.Id, percent, d.SubjectName, $"{((d.Rate == null) ? 0 : d.Rate)}/{d.MaxCurrentRate}/100"));
			}
		}

		private string SemesterFromDiscipline(Discipline discipline) => $"{(discipline.SemesterNum == 1 ? "Осень" : "Весна")} {(discipline.SemesterNum == 1 ? discipline.SemesterYear : discipline.SemesterYear + 1)}";

		private async void OnListItemTapped(object sender, ItemTappedEventArgs e)
		{
			((ListView)sender).SelectedItem = null;
			var item = (DisciplineItem)e.Item;
			await Navigation.PushAsync(new StudentDisciplinePage(item.ID));
			//Navigation.InsertPageBefore(new StudentDisciplinePage(item.ID), this);
		}
		void OnToolbarItemClicked(object sender, EventArgs e)
		{
			ToolbarItem item = (ToolbarItem)sender;
			LoadDisciplines((long)item.CommandParameter);
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
