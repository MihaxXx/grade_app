using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grade;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace grade_app
{
	public partial class TeacherIndexPage : ContentPage
	{
		TeacherIndex teacherIndex;
		List<Semester> SemesterList { get; set; }

		public long CurrentSemID { get; set; }
		public ObservableCollection<SubjectGroup> GroupedDisciplineItems1 { get; private set; } = new ObservableCollection<SubjectGroup>();

		public TeacherIndexPage()
		{
			InitializeComponent();

			LoadSemesters();
			LoadDisciplines(SemesterList.Max(s => s.Id));
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
					Text = sem.ToString(),
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
			teacherIndex = App.API.TeacherGetIndex(SemesterID);
			CurrentSemID = SemesterID;
			Title = $"БРС - {SemesterList.Find(s=> s.Id == CurrentSemID)}";
			GroupedDisciplineItems1.Clear();
			if (teacherIndex.Subjects != null)
			{
				EmptyListText.IsVisible = false;
				foreach (var s in teacherIndex.Subjects)
				{
					var group = new SubjectGroup($"{s.Value.SubjectName} \n{s.Value.Degree}, {s.Value.GradeNum} курс");
					foreach (var d in s.Value.Disciplines)
					{
						group.Add(new DisciplineItem(
								$"{s.Value.SubjectName}  \n{s.Value.Degree}, {s.Value.GradeNum} курс",
								d.Id,
								string.Join('\n', teacherIndex.Groups[d.Id.ToString()]),
								d.TypeToString(),
								string.Join('\n', teacherIndex.Teachers[d.Id.ToString()].Values.Select(t => t.ShortName()).Take(4))
							));
					}
					GroupedDisciplineItems1.Add(group);
				}
			}
			else
			{
				EmptyListText.IsVisible = true;
			}
		}

		private async void OnListItemTapped(object sender, ItemTappedEventArgs e)
		{
			((ListView)sender).SelectedItem = null;
			var item = (DisciplineItem)e.Item;
			await Navigation.PushAsync(new TeacherDisciplinePage(item.ID));
		}
		void OnToolbarItemClicked(object sender, EventArgs e)
		{
			ToolbarItem item = (ToolbarItem)sender;
			LoadDisciplines((long)item.CommandParameter);
		}
		public class DisciplineItem
		{
			public DisciplineItem(string name, long id, string groups, string type, string teachers)
			{
				Name = name;
				ID = id;
				Groups = groups;
				Type = type;
				Teachers = teachers;
			}

			public string Name { get; set; }
			public long ID { get; set; }
			public string Groups { get; set; }
			public string Type { get; set; }
			public string Teachers { get; set; }
		}
		public class SubjectGroup :List<DisciplineItem>
		{
			public string Name { get; set; }
			//TODO: Output as 2nd column, not row
			public string DegreeCourse;
			public SubjectGroup(string name)
			{
				Name = name;
			}
			public static IList<SubjectGroup> All { private set; get; }
		}
	}
}