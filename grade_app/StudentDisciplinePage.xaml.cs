using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grade;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace grade_app
{
	public partial class StudentDisciplinePage : TabbedPage
	{
		public StudentDiscipline StudentDiscipline { get; private set; }
		public StudentJournal studentJour { get; private set; }

		public ObservableCollection<Journal> StudentJournal { get; private set; } = new ObservableCollection<Journal>();

		public DisciplineInfo DisciplineInfo { get; private set; } = new DisciplineInfo();

		public ObservableCollection<IGrouping<string, SubModuleItem>> GroupedSubModules { get; private set; } = new ObservableCollection<IGrouping<string, SubModuleItem>>();


		public StudentDisciplinePage(long id)
		{
			InitializeComponent();
			_ = LoadData(id);
			DisciplineInfo.Type = "";

			//Must be at the end!!!
			BindingContext = this;
		}

		private async Task LoadData(long id)
		{
			activityIndicator.IsRunning = activityIndicator.IsVisible = true;
			activityIndicatorJour.IsRunning = activityIndicatorJour.IsVisible = true;

			(int SemesterRate, int SemesterMaxRate) = await LoadModules(id);
			Title = StudentDiscipline.Discipline.SubjectName;
			if (!StudentDiscipline.Discipline.IsMapCreated)
			{
				WarningLabel.Text = "Для дисциплины не создана учебная карта";
				WarningLabel.IsVisible = true;
			}
			FillDisciplineInfo(SemesterRate, SemesterMaxRate);
			activityIndicator.IsRunning = activityIndicator.IsVisible = false;

			studentJour = await App.API.StudentGetDisciplineJournal(id);
			foreach (var lesson in studentJour.Journal)
				StudentJournal.Add(lesson);
			DisciplineInfo.GymInfo = studentJour.GymAttendanceInfo != null ?
				$"{studentJour.GymAttendanceInfo.DebtCount} в счет задолженности\n" +
				$"{studentJour.GymAttendanceInfo.SemesterCount} в баллах текущего семестра\n" +
				$"{studentJour.GymAttendanceInfo.TotalAttendance} посещений учтено\n" +
				$"{studentJour.GymAttendanceInfo.Uncounted} еще не учтено" :
				"";
			DisciplineInfo.IsNewGym = studentJour.IsGym && studentJour.GymAttendanceInfo != null && studentJour.Discipline.SemesterId >= 17;

			activityIndicatorJour.IsRunning = activityIndicatorJour.IsVisible = false;
		}

		private void FillDisciplineInfo(int SemesterRate, int SemesterMaxRate)
		{
			FillBaseDisInfo();
			if (DisciplineInfo.IsExam)
			{
				FillExamInfo(SemesterRate, SemesterMaxRate);
			}
			else
			{
				FillCreditInfo(SemesterRate);
			}
		}

		private void FillBaseDisInfo()
		{
			DisciplineInfo.Type = StudentDiscipline.Discipline.Type switch
			{
				"exam" => "Экзамен",
				"credit" => "Зачет",
				"grading_credit" => "Дифф. зачет",
				_ => StudentDiscipline.Discipline.Type,
			};
			var year = StudentDiscipline.Discipline.SemesterYear;
			DisciplineInfo.SemesterName = $"{(StudentDiscipline.Discipline.SemesterNum == 1 ? "Осенний" : "Весенний")} семестр {year}/{year + 1} учебного года";
			DisciplineInfo.Teachers = string.Join('\n', StudentDiscipline.Teachers.Select(t => t.Name));
			DisciplineInfo.StudyLoad = DisciplineInfo.StudyLoadToText(StudentDiscipline.Discipline.Lectures, StudentDiscipline.Discipline.Practice, StudentDiscipline.Discipline.Labs);

			DisciplineInfo.IsExam = StudentDiscipline.Discipline.Type == "exam";
			DisciplineInfo.IsBonus = StudentDiscipline.Discipline.IsBonus;
			DisciplineInfo.IsExtraRate = StudentDiscipline.ExtraRate > 0;
			DisciplineInfo.IsExamOrBonusOrExtraRate = DisciplineInfo.IsExam || DisciplineInfo.IsBonus || DisciplineInfo.IsExtraRate;
		}

		private async Task<(int, int)> LoadModules(long id)
		{
			int SemesterRate = 0;
			int SemesterMaxRate = 0;

			StudentDiscipline = await App.API.StudentGetDiscipline(id);
			var SubModuleItems = new List<SubModuleItem>();
			if (StudentDiscipline.DisciplineMap == null)
				return (SemesterRate, SemesterMaxRate);
			foreach (var m in StudentDiscipline.DisciplineMap.Modules)
			{
				foreach (var smID in m.Value.Submodules)
				{
					var smVal = StudentDiscipline.Submodules[smID];
					SubModuleItems.Add(new SubModuleItem(smID, m.Key, m.Value.Title, smVal.Title, smVal.MaxRate, smVal.Rate, smVal.Date));

					SemesterRate += smVal.Rate ?? 0;
					SemesterMaxRate += smVal.MaxRate ?? 0;
				}
			}
			foreach (var m in SubModuleItems.GroupBy(sm => sm.ModuleTitle))
				GroupedSubModules.Add(m);
			return (SemesterRate, SemesterMaxRate);
		}

		private void FillCreditInfo(int SemesterRate)
		{
			DisciplineInfo.ResultHeader1 = "Зачет";
			var Admission = 60 - (SemesterRate + StudentDiscipline.ExtraRate);
			//TODO: Fix num ending
			DisciplineInfo.ResultText = Admission > 0 ?
				$"Для получения зачета необходимо набрать ещё {Admission} баллов." :
				$"Поздравляем, Вы получили зачет по курсу «{StudentDiscipline.Discipline.SubjectName}»!";
			DisciplineInfo.ExtraRate = new SubModuleItem(-1, -1, "", "Добор баллов", 38, StudentDiscipline.ExtraRate, null);
			DisciplineInfo.ResultSubHeader2 = $"Зачет по курсу «{StudentDiscipline.Discipline.SubjectName}»";

			long BonusID = -1;
			Submodule Bonus = null;
			if (DisciplineInfo.IsBonus)
			{
				BonusID = StudentDiscipline.DisciplineMap.Bonus;
				Bonus = StudentDiscipline.Submodules[BonusID];
				DisciplineInfo.Bonus = new SubModuleItem(BonusID, -1, "", "Бонусные баллы", Bonus.MaxRate, Bonus.Rate, Bonus.Date);
			}
			var Rating = SemesterRate + StudentDiscipline.ExtraRate + (StudentDiscipline.Discipline.IsBonus && Bonus != null && Bonus.Rate != null ? Bonus.Rate : 0);
			DisciplineInfo.FinalTotalRate = $"Итоговый рейтинг: {Math.Min(Rating.Value, 100)} / 100";
		}

		private void FillExamInfo(int SemesterRate, int SemesterMaxRate)
		{
			DisciplineInfo.ResultHeader1 = "Допуск к экзамену";
			var Admission = 38 - (SemesterRate + StudentDiscipline.ExtraRate);
			//TODO: Fix num ending
			DisciplineInfo.ResultText = Admission > 0 ?
				$"Для допуска к экзамену Вам необходимо получить еще {Admission} баллов." :
				"Поздравляем, заработанных Вами баллов достаточно для получения допуска к экзамену!";
			DisciplineInfo.ExtraRate = new SubModuleItem(-1, -1, "", "Добор баллов", 38, StudentDiscipline.ExtraRate, null);
			DisciplineInfo.MiddleTotalRate = $"Промежуточный итог: {SemesterRate + StudentDiscipline.ExtraRate} / {SemesterMaxRate}";
			DisciplineInfo.ResultHeader2 = "Экзамен";
			DisciplineInfo.ResultSubHeader2 = $"Экзамен по курсу «{StudentDiscipline.Discipline.SubjectName}»";
			long BonusID = -1;
			Submodule Bonus = null;
			if (DisciplineInfo.IsBonus && StudentDiscipline.DisciplineMap != null)
			{
				BonusID = StudentDiscipline.DisciplineMap.Bonus;
				Bonus = StudentDiscipline.Submodules[BonusID];
				DisciplineInfo.Bonus = new SubModuleItem(BonusID, -1, "", "Бонусные баллы", Bonus.MaxRate, Bonus.Rate, Bonus.Date);
			}
			var ExamID = (StudentDiscipline.Discipline.IsMapCreated && StudentDiscipline.DisciplineMap != null) ? StudentDiscipline.DisciplineMap.Exam : -1;
			var Exam = ExamID != -1 ? StudentDiscipline.Submodules[ExamID] : StudentDiscipline.Submodules.First().Value;
			DisciplineInfo.Exam = new SubModuleItem(ExamID, -1, "", $"Экзамен по курсу «{StudentDiscipline.Discipline.SubjectName}»", Exam.MaxRate, Exam.Rate, Exam.Date);
			var Rating = SemesterRate + StudentDiscipline.ExtraRate + (StudentDiscipline.Discipline.IsBonus && Bonus != null && Bonus.Rate != null ? Bonus.Rate : 0) + (Exam.Rate != null ? Exam.Rate : 0);
			DisciplineInfo.FinalTotalRate = $"Итоговый рейтинг: {Math.Min(Rating.Value, 100)} / 100";
		}
	}
	public class SubModuleItem
	{
		public SubModuleItem(long id, long moduleID, string moduleTitle, string title, int? maxRate, int? rate, DateTime? date)
		{
			ID = id;
			ModuleID = moduleID;
			ModuleTitle = moduleTitle;
			Title = title;
			Rate = $"{(rate ?? 0)}/{(maxRate ?? 0)}";
			Percent = ((double)(rate ?? 0) / (maxRate ?? 0)).ToString("P0");
			Date = date.HasValue ? date.Value.ToString("d") : "—";
		}

		public long ID { get; set; }
		public long ModuleID { get; set; }
		public string ModuleTitle { get; set; }
		public string Title { get; set; }
		public string Rate { get; set; }
		public string Percent { get; set; }
		public string Date { get; set; }

	}
	public class DisciplineInfo : INotifyPropertyChanged
	{
		private bool isExam;
		private bool isBonus;
		private bool isExtraRate;
		private bool isExamOrBonusOrExtraRate;
		private string type;
		private string semesterName;
		private string teachers;
		private string studyLoad;
		private string resultHeader1;
		private string resultText;
		private SubModuleItem extraRate;
		private string middleTotalRate;
		private string resultHeader2;
		private string resultSubHeader2;
		private SubModuleItem bonus;
		private SubModuleItem exam;
		private string finalTotalRate;
		private bool isNewGym;
		private string gymInfo;

		public event PropertyChangedEventHandler PropertyChanged;

		private static string HoursToText(int hours)
		{
			if (hours >= 10 && hours <= 20 || hours % 10 >= 5)
				return hours + " часов";
			else if (hours % 10 != 1)
				return hours + " часа";
			else return hours + " час";
		}
		public static string StudyLoadToText(int Lectures, int Practice, int Labs)
		{
			string res = string.Empty;
			if (Lectures != 0)
			{
				res += $"{HoursToText(Lectures)} теории";
				if (Practice != 0)
					if (Labs != 0)
						res += $", {HoursToText(Practice)} практики и {HoursToText(Labs)} лабораторных занятий";
					else
						res += $" и {HoursToText(Practice)} практики";
				else if (Labs != 0)
					res += $" и {HoursToText(Labs)} лабораторных занятий";
			}
			else
			{
				if (Practice != 0)
					if (Labs != 0)
						res += $"{HoursToText(Practice)} практики и {HoursToText(Labs)} лабораторных занятий";
					else
						res += $"{HoursToText(Practice)} практики";
				else if (Labs != 0)
					res += $"{HoursToText(Labs)} лабораторных занятий";
			}
			return res;
		}
		public bool IsExam
		{
			get => isExam; set
			{
				isExam = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExam)));
			}
		}
		public bool IsBonus
		{
			get => isBonus; set
			{
				isBonus = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBonus)));
			}
		}
		public bool IsExtraRate
		{
			get => isExtraRate; set
			{
				isExtraRate = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isExtraRate)));
			}
		}
		public bool IsExamOrBonusOrExtraRate
		{
			get => isExamOrBonusOrExtraRate; set
			{
				isExamOrBonusOrExtraRate = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExamOrBonusOrExtraRate)));
			}
		}
		/// <summary>
		/// List Header
		/// </summary>
		public string Type
		{
			get => type; set
			{
				type = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
			}
		}
		public string SemesterName
		{
			get => semesterName; set
			{
				semesterName = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SemesterName)));
			}
		}
		public string Teachers
		{
			get => teachers; set
			{
				teachers = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Teachers)));
			}
		}
		public string StudyLoad
		{
			get => studyLoad; set
			{
				studyLoad = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudyLoad)));
			}
		}

		/// <summary>
		/// List Footer
		/// </summary>
		public string ResultHeader1
		{
			get => resultHeader1; set
			{
				resultHeader1 = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultHeader1)));
			}
		}
		public string ResultText
		{
			get => resultText; set
			{
				resultText = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultText)));
			}
		}
		public SubModuleItem ExtraRate
		{
			get => extraRate; set
			{
				extraRate = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExtraRate)));
			}
		}
		public string MiddleTotalRate
		{
			get => middleTotalRate; set
			{
				middleTotalRate = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MiddleTotalRate)));
			}
		}
		public string ResultHeader2
		{
			get => resultHeader2; set
			{
				resultHeader2 = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultHeader2)));
			}
		}
		public string ResultSubHeader2
		{
			get => resultSubHeader2; set
			{
				resultSubHeader2 = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultSubHeader2)));
			}
		}
		public SubModuleItem Bonus
		{
			get => bonus; set
			{
				bonus = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Bonus)));
			}
		}
		public SubModuleItem Exam
		{
			get => exam; set
			{
				exam = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Exam)));
			}
		}
		public string FinalTotalRate
		{
			get => finalTotalRate; set
			{
				finalTotalRate = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinalTotalRate)));
			}
		}

		public string GymInfo
		{
			get => gymInfo; set
			{
				gymInfo = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GymInfo)));
			}
		}
		public bool IsNewGym
		{
			get => isNewGym; set
			{
				isNewGym = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNewGym)));
			}
		}
	}
}