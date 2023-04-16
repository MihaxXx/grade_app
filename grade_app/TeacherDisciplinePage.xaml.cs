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
	public partial class TeacherDisciplinePage : TabbedPage
	{
		public TeacherDiscipline TeacherDiscipline { get; private set; }
		public List<SubModulePickerItem> subModulePickerItems { get; private set; } = new List<SubModulePickerItem>();
		public ObservableCollection<DisGroup> GroupedStudentItems { get; private set; } = new ObservableCollection<DisGroup>();

		public bool DisciplineNotFrozen { get; private set; }

		public TeacherJournal TeacherJournal { get; private set; }
		public List<LessonPickerItem> LessonPickerItems { get; private set; } = new List<LessonPickerItem>();
		public ObservableCollection<DisJourGroup> GroupedJournalStudentItems { get; private set; } = new ObservableCollection<DisJourGroup>();


		public TeacherDisciplinePage(long id)
		{
			InitializeComponent();

			TeacherDiscipline = App.API.TeacherGetDiscipline(id);
			DisciplineNotFrozen = !TeacherDiscipline.Discipline.Frozen;
			FillSubModulePicker();
			FillStudentsList();
			if(!TeacherDiscipline.Discipline.IsMapCreated)
			{
				WarningLabel.Text = "Для дисциплины не создана учебная карта";
				WarningLabel.IsVisible = true;
			}
			if(TeacherDiscipline.Discipline.Frozen)
			{
				WarningLabel.Text = "Дисциплина подписана, выставление баллов невозможно";
				WarningLabel.IsVisible = true;
			}

			TeacherJournal = App.API.TeacherGetDisciplineJournal(id);
			FillLessonPicker();
			FillJournalStudentsList();

			//Must be at the end!!!
			BindingContext = this;
		}

		private void FillStudentsList()
		{
			var smi = (SubModulePickerItem)SubmodulePicker.SelectedItem;
			GroupedStudentItems.Clear();

			try
			{
				var studentSubmodules = new List<StudentSubmoduleItem>();
				var MaxExtraRate = TeacherDiscipline.Discipline.Type == "exam" ? 38 : 60;
				if (smi != null && TeacherDiscipline != null && TeacherDiscipline.Students != null)
				{
					foreach (var group in TeacherDiscipline.Students)
					{
						var groupInfo = TeacherDiscipline.Groups[group.Key];
						var disGroup = new DisGroup(groupInfo.Name() + " | " + groupInfo.SpecAbbr);
						foreach (var student in group.Value)
						{
							var Rate = TeacherDiscipline.Rates != null && TeacherDiscipline.Rates.ContainsKey(student.RecordBookId) ?
									TeacherDiscipline.Rates[student.RecordBookId].ContainsKey(smi.ID) ?
										new int?(TeacherDiscipline.Rates[student.RecordBookId][smi.ID]) :
										0 :
									null;
							var SemesterRate = TeacherDiscipline.Modules.Where(m => m.Value.Type == ModuleType.Regular).Sum(m =>
							TeacherDiscipline.Rates != null && TeacherDiscipline.Rates.ContainsKey(student.RecordBookId) ?
									m.Value.Submodules.Sum(sm =>
									TeacherDiscipline.Rates[student.RecordBookId].ContainsKey(sm.Id) ?
									new int?(TeacherDiscipline.Rates[student.RecordBookId][sm.Id]) : 0) : 0);
							disGroup.Add(new StudentSubmoduleItem(
								student.ShortName(),
								student.RecordBookId,
								Rate,
								//TODO: In case of ModuleType.Extra should be MaxExtraRate - SemesterRate - PreviousSubModulesFromExtraModule, current version will lie maxrate on second extra submodule
								//TODO: Also, In case of ModuleType.Extra would be better to display - instead of 0 on non-positive results (when extra rates is unnecessary) 
								smi.moduleType == ModuleType.Extra ? Math.Max(MaxExtraRate - SemesterRate.Value, 0) : smi.MaxRate));
						}
						GroupedStudentItems.Add(disGroup);
					}
				}
			}
			catch(NullReferenceException e)
			{
				Console.WriteLine("Error: " + e.Message);
			}
		}

		private void FillSubModulePicker()
		{
			try
			{
				if (TeacherDiscipline == null || TeacherDiscipline.Modules == null)
					return;
				//TODO: check module type, workaround non-regular modules submodules names
				foreach (var m in TeacherDiscipline.Modules)
					for (int i = 0; i < m.Value.Submodules.Length; i++)
					{
						SubmoduleT sm = m.Value.Submodules[i];
						switch (m.Value.Type)
						{
							case ModuleType.Exam:
								if (i == 0)
									subModulePickerItems.Add(new SubModulePickerItem($"{m.Value.Name} - Основная сдача", sm.ModuleId, sm.Id, sm.Rate, m.Value.Type));
								else
									subModulePickerItems.Add(new SubModulePickerItem($"{m.Value.Name} - Пересдача {i}", sm.ModuleId, sm.Id, sm.Rate, m.Value.Type));
								break;
							case ModuleType.Extra:
								subModulePickerItems.Add(new SubModulePickerItem($"{m.Value.Name}{(m.Value.Submodules.Length > 1 ? " " + (i + 1).ToString() : "")}", sm.ModuleId, sm.Id, sm.Rate, m.Value.Type));
								break;
							case ModuleType.Bonus:
								subModulePickerItems.Add(new SubModulePickerItem(m.Value.Name, sm.ModuleId, sm.Id, sm.Rate, m.Value.Type));
								break;
							case ModuleType.Regular:
								subModulePickerItems.Add(new SubModulePickerItem(sm.Name, sm.ModuleId, sm.Id, sm.Rate, m.Value.Type));
								break;
						}
					}

				SubmodulePicker.ItemsSource = subModulePickerItems;
				SubmodulePicker.SelectedIndex = 0;
			}
			catch (NullReferenceException e)
			{
				Console.WriteLine("Error: " + e.Message);
			}
		}

		private void SubmodulePicker_SelectedIndexChanged(object sender, EventArgs e)
		{
			FillStudentsList();
		}

		private void FillLessonPicker()
		{
			try
			{
				if (TeacherJournal.Lessons == null)
					return;
				foreach (var l in TeacherJournal.Lessons)
					LessonPickerItems.Add(new LessonPickerItem($"{l.LessonDate:d} - {TeacherJournal.LessonTypes.First(lt => lt.Id == l.LessonType).Type} ({l.LessonName})",
						TeacherJournal.LessonTypes.First(lt => lt.Id == l.LessonType).Type, l.Id, l.LessonDate));

				LessonPicker.ItemsSource = LessonPickerItems;
				LessonPicker.SelectedIndex = 0;
			}
			catch (NullReferenceException e)
			{
				Console.WriteLine("Error: " + e.Message);
			}
		}

		private void FillJournalStudentsList()
		{
			var li = (LessonPickerItem)LessonPicker.SelectedItem;
			GroupedJournalStudentItems.Clear();

			try
			{
				var studentLessons = new List<StudentJournalItem>();
				if (li != null && TeacherJournal.Students != null)
				{
					//TODO: respect subgroup information
					foreach (var group in TeacherJournal.Students)
					{
						var groupInfo = TeacherJournal.Groups[group.Key];
						var disGroup = new DisJourGroup(groupInfo.Name() + " | " + groupInfo.SpecAbbr);
						foreach (var student in group.Value)
						{
							disGroup.Add(new StudentJournalItem(student.ShortName(), student.RecordBookId,
								TeacherJournal.Attendance != null && TeacherJournal.Attendance.ContainsKey(student.RecordBookId) ?
									TeacherJournal.Attendance[student.RecordBookId].ContainsKey(li.ID) ?
										new bool?(TeacherJournal.Attendance[student.RecordBookId][li.ID] > 0) :
										false :
									null
								));
						}
						GroupedJournalStudentItems.Add(disGroup);
					}
				}
			}
			catch(NullReferenceException e)
			{
				Console.WriteLine("Error: " + e.Message);
			}
		}

		private void LessonPicker_SelectedIndexChanged(object sender, EventArgs e)
		{
			FillJournalStudentsList();
		}

		async private void ToolbarItem_Clicked(object sender, EventArgs e)
		{
			try
			{
				var item = sender as ToolbarItem;
				if (item.AutomationId == "add_lesson")
				{
					string action = await DisplayActionSheet("Выберите тип создаваемого занятия", "Отмена", null, TeacherJournal.LessonTypes.Select(lt => lt.Type).ToArray());
					var res = App.API.TeacherPostCreateLesson(TeacherJournal.Discipline.Id, DateTime.Today, TeacherJournal.LessonTypes.First(lt => lt.Type == action));
					if (res.Item1 == false)
					{
						await DisplayAlert("CreateLesson error", res.Item2, "OK");
					}
				}
			}
			catch (NullReferenceException ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}
		}

		private void Entry_Unfocused(object sender, FocusEventArgs e)
		{
			try
			{
				var student = (sender as Entry).Parent.Parent.BindingContext as StudentSubmoduleItem;
				var submodule = (SubModulePickerItem)SubmodulePicker.SelectedItem;
				if (!student.Rate.HasValue || student.Rate > student.MaxRate)
				{
					(sender as Entry).BackgroundColor = Color.FromHex("#a94442");
					return;
				}
				(sender as Entry).BackgroundColor = Color.Default;
				var res = App.API.TeacherPostSetRate(student.RecordBookId, TeacherDiscipline.Discipline.Id, submodule.ID, student.Rate.Value);
				if (res.Item1 == false)
				{
					DisplayAlert("SetRate error", res.Item2, "OK");
				}
			}
			catch(NullReferenceException ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}
		}

		private void Entry_Focused(object sender, FocusEventArgs e)
		{
			(sender as Entry).CursorPosition = (sender as Entry).Text?.Length ?? 0;
		}

		private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			try
			{
				var student = (sender as CheckBox).Parent.Parent.BindingContext as StudentJournalItem;
				if (student != null && student.Attendance.HasValue && student.Attendance.Value != e.Value)
				{
					student.Attendance = e.Value;
					var lesson = LessonPicker.SelectedItem as LessonPickerItem;
					if (lesson == null || !student.Attendance.HasValue)
						return;
					var res = App.API.TeacherPostSetAttendance(lesson.ID, student.RecordBookId, student.Attendance.Value);
					if (res.Item1 == false)
					{
						DisplayAlert("SetAttendance error", res.Item2, "OK");
					}
				}
			}
			catch (NullReferenceException ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}
		}
	}

	public class StudentSubmoduleItem
	{
		public StudentSubmoduleItem(string name, long recordBookId, int? rate, int maxRate)
		{
			Name = name;
			RecordBookId = recordBookId;
			Rate = rate;
			MaxRate = maxRate;
		}

		public string Name { get; set; }
		public long RecordBookId { get; set; }
		public int? Rate { get; set; }
		public int MaxRate { get; set; }
	}
	public class SubModulePickerItem
	{
		public SubModulePickerItem(string name, long moduleID, long iD, int maxRate, ModuleType moduleType)
		{
			Name = name;
			ModuleID = moduleID;
			ID = iD;
			MaxRate = maxRate;
			this.moduleType = moduleType;
		}

		public string Name { get; set; }
		public long ModuleID { get; set; }
		public long ID { get; set; }
		public int MaxRate { get; set; }
		public ModuleType moduleType { get; set; }
	}
	public class DisGroup : List<StudentSubmoduleItem>
	{
		public string Name { get; set; }
		public DisGroup(string name)
		{
			Name = name;
		}
		public static IList<DisGroup> All { private set; get; }
	}
	public class StudentJournalItem
	{
		public StudentJournalItem(string name, long recordBookId, bool? attendance)
		{
			Name = name;
			RecordBookId = recordBookId;
			Attendance = attendance;
		}

		public string Name { get; set; }
		public long RecordBookId { get; set; }
		public bool? Attendance { get; set; }
	}

	public class LessonPickerItem
	{
		public LessonPickerItem(string name, string type, long iD, DateTime date)
		{
			Name = name;
			Type = type;
			ID = iD;
			Date = date;
		}

		public string Name { get; set; }
		public string Type { get; set; }
		public long ID { get; set; }
		public DateTime Date { get; set; }
	}

	public class DisJourGroup : List<StudentJournalItem>
	{
		public string Name { get; set; }
		public DisJourGroup(string name)
		{
			Name = name;
		}
		public static IList<DisJourGroup> All { private set; get; }
	}
}