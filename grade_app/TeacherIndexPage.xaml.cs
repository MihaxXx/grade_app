using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Grade;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace grade_app
{
	public partial class TeacherIndexPage : ContentPage
	{
		TeacherIndex teacherIndex;
		public static List<Semester> SemesterList { get; set; }

		public long CurrentSemID { get; set; }
		public ObservableCollection<SubjectGroup> GroupedDisciplineItems1 { get; private set; } = new ObservableCollection<SubjectGroup>();

		public TeacherIndexPage()
		{
			InitializeComponent();

			_ = LoadData();
			//Must be at the end!!!
			BindingContext = this;
		}

		private async Task LoadData()
		{
			activityIndicator.IsRunning = activityIndicator.IsVisible = true;
			await LoadSemesters();
			await LoadDisciplines(SemesterList.Max(s => s.Id));
			activityIndicator.IsRunning = activityIndicator.IsVisible = false;
		}

		private async Task LoadSemesters()
		{
			SemesterList = await App.API.GetSemesterList();
		}

		private async Task LoadDisciplines(long SemesterID)
		{
			CurrentSemID = SemesterID;
			Title = $"БРС - {SemesterList.Find(s => s.Id == CurrentSemID)}";
			teacherIndex = await App.API.TeacherGetIndex(SemesterID);
			GroupedDisciplineItems1.Clear();
			if (teacherIndex.Subjects != null)
			{
				///Find the longest line in groups column
				var GlobalDiscsIDs = teacherIndex.Subjects.Values.SelectMany(s => s.Disciplines).Where(d => d.IsGlobal).Select(d => d.Id.ToString()).ToHashSet();
				var GlobalDiscText = "Межфакультетская дисциплина";
                var MaxGroupNameLenght = teacherIndex.Groups.Where( kv => !GlobalDiscsIDs.Contains(kv.Key)).Select(kv => kv.Value).Max(groups => groups.Max(group => group.Length));
				if (GlobalDiscsIDs.Count > 0)
					MaxGroupNameLenght = MaxGroupNameLenght > GlobalDiscText.Length ? MaxGroupNameLenght : GlobalDiscText.Length;

                EmptyListText.IsVisible = false;
				foreach (var s in teacherIndex.Subjects)
				{
					var group = new SubjectGroup($"{s.Value.SubjectName}",s.Value.GradeNum == null ? "" : $"{s.Value.ShortDegree()}\n{s.Value.GradeNum} курс");
					foreach (var d in s.Value.Disciplines)
					{
						/// Ugly fix to make column width equal for every row regardless of UI scale and screen size, center allined text
						/// Prepare 3 variants of groups column contents depending on set of students (no groups/non-global disc/global disc)
						var NoStudText = "Нет студентов";
						var NormolizedNoStudText = 
							string.Concat(Enumerable.Repeat(" ", (int)((MaxGroupNameLenght - NoStudText.Length)*0.8))) + NoStudText + string.Concat(Enumerable.Repeat(" ", (int)((MaxGroupNameLenght - NoStudText.Length) * 0.8)));
                        string[] NormolizedGroupNames = new string[0];
                        if (teacherIndex.Groups.ContainsKey(d.Id.ToString()))
							NormolizedGroupNames = teacherIndex.Groups[d.Id.ToString()].
								Select(g => string.Concat(Enumerable.Repeat(" ", MaxGroupNameLenght - g.Length)) + g + string.Concat(Enumerable.Repeat(" ", MaxGroupNameLenght - g.Length))).ToArray();
                        string NormolizedGlobalName = "";
                        if (d.IsGlobal)
							NormolizedGlobalName = Regex.Replace(d.GlobalName, $".{{{(int)(MaxGroupNameLenght*0.8)}}}", "$0\n");

                        group.Add(new DisciplineItem(
                                s.Value.GradeNum == null ? $"{s.Value.SubjectName}" : $"{s.Value.SubjectName} \n{s.Value.ShortDegree()}, {s.Value.GradeNum} курс",
								d.Id,
								d.IsGlobal?
                                    GlobalDiscText + "\n" + NormolizedGlobalName :
									teacherIndex.Groups.ContainsKey(d.Id.ToString()) ? string.Join('\n', NormolizedGroupNames) : NormolizedNoStudText,
								d.TypeToString() + (d.Frozen ? "\n подписано" : "\n"),
								teacherIndex.Teachers.ContainsKey(d.Id.ToString()) ? string.Join('\n', teacherIndex.Teachers[d.Id.ToString()].Values.Select(t => t.ShortName()).Take(4)) : "Нет преподавателей"
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
			await Navigation.PushAsync(new TeacherDisciplinePage(item.ID, teacherIndex.Teachers[item.ID.ToString()].Values.ToList()));
		}
		async void OnToolbarItemClicked(object sender, EventArgs e)
		{
			try
			{
				var item = sender as ToolbarItem;
				if (item.AutomationId == "change_semester")
				{
					var semlist = SemesterList.Select(sem => sem.ToString()).ToList();
					string action = await DisplayActionSheet("Выберите семестр", "Отмена", null, semlist.ToArray());
					if (action != null && action != "Отмена")
					{
						var res = SemesterList[semlist.FindIndex(sem => sem == action)];
						if (res.Id != CurrentSemID)
							_ = LoadDisciplines(res.Id);
					}
				}
				else if (item.AutomationId == "logout")
				{
					App.WipeUser();
					Navigation.InsertPageBefore(new MainPage(), this);
					await Navigation.PopAsync();
				}
			}
			catch (NullReferenceException ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}
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
			public string DegreeCourse { get; set; }

            public SubjectGroup(string name, string degreeCourse)
            {
                Name = name;
                DegreeCourse = degreeCourse;
            }

            public static IList<SubjectGroup> All { private set; get; }
		}
	}
}