using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grade;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace grade_app
{
	public partial class StudentDisciplinePage : TabbedPage
	{
		public StudentDiscipline StudentDiscipline { get; private set; }

		public List<Journal> StudentJournal { get; private set; }

		public DisciplineInfo DisciplineInfo { get; private set; }

		public IEnumerable<IGrouping<string, SubModuleItem>> GroupedSubModules { get; private set; }


		public StudentDisciplinePage(long id)
		{
			InitializeComponent();

			int? SemesterRate = 0;
			int? SemesterMaxRate = 0;

			LoadModules(id, ref SemesterRate, ref SemesterMaxRate);
			if (!StudentDiscipline.Discipline.IsMapCreated)
			{
				WarningLabel.Text = "Для дисциплины не создана учебная карта";
				WarningLabel.IsVisible = true;
			}

			StudentJournal = App.API.StudentGetDisciplineJournal(id).Journal.ToList();

			FillDisciplineInfo(SemesterRate, SemesterMaxRate);

			//Must be at the end!!!
			BindingContext = this;
		}

		private void FillDisciplineInfo(int? SemesterRate, int? SemesterMaxRate)
		{
			DisciplineInfo = new DisciplineInfo();
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

			};
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

		private void LoadModules(long id, ref int? SemesterRate, ref int? SemesterMaxRate)
		{
			StudentDiscipline = App.API.StudentGetDiscipline(id);
			var SubModuleItems = new List<SubModuleItem>();
			if (StudentDiscipline.DisciplineMap == null)
				return;
			foreach (var m in StudentDiscipline.DisciplineMap.Modules)
			{
				foreach (var smID in m.Value.Submodules)
				{
					var smVal = StudentDiscipline.Submodules[smID.ToString()];
					SubModuleItems.Add(new SubModuleItem(smID, long.Parse(m.Key), m.Value.Title, smVal.Title, smVal.MaxRate, smVal.Rate, smVal.Date));

					SemesterRate += (smVal.Rate == null ? 0 : smVal.Rate);
					SemesterMaxRate += (smVal.MaxRate == null ? 0 : smVal.MaxRate);
				}
			}
			GroupedSubModules = SubModuleItems.GroupBy(sm => sm.ModuleTitle);
		}

		private void FillCreditInfo(int? SemesterRate)
		{
			DisciplineInfo.ResultHeader1 = "Зачет";
			var Admission = 60 - (SemesterRate + StudentDiscipline.ExtraRate);
			//TODO: Fix num ending
			DisciplineInfo.ResultText = Admission > 0 ?
				$"Для получения зачета необходимо набрать ещё { Admission } баллов." :
				$"Поздравляем, Вы получили зачет по курсу «{ StudentDiscipline.Discipline.SubjectName }»!";
			DisciplineInfo.ExtraRate = new SubModuleItem(-1, -1, "", "Добор баллов", 38, StudentDiscipline.ExtraRate, null);
			DisciplineInfo.ResultSubHeader2 = $"Зачет по курсу «{ StudentDiscipline.Discipline.SubjectName }»";

			long BonusID = -1;
			Submodule Bonus = null;
			if (DisciplineInfo.IsBonus)
			{
				BonusID = StudentDiscipline.DisciplineMap.Bonus;
				Bonus = StudentDiscipline.Submodules[BonusID.ToString()];
				DisciplineInfo.Bonus = new SubModuleItem(BonusID, -1, "", "Бонусные баллы", Bonus.MaxRate, Bonus.Rate, Bonus.Date);
			}
			var Rating = SemesterRate + StudentDiscipline.ExtraRate + (StudentDiscipline.Discipline.IsBonus && Bonus != null && Bonus.Rate != null ? Bonus.Rate : 0);
			DisciplineInfo.FinalTotalRate = $"Итоговый рейтинг: { Math.Min(Rating.Value, 100) } / 100";
		}

		private void FillExamInfo(int? SemesterRate, int? SemesterMaxRate)
		{
			DisciplineInfo.ResultHeader1 = "Допуск к экзамену";
			var Admission = 38 - (SemesterRate + StudentDiscipline.ExtraRate);
			//TODO: Fix num ending
			DisciplineInfo.ResultText = Admission > 0 ?
				$"Для допуска к экзамену Вам необходимо получить еще { Admission } баллов." :
				"Поздравляем, заработанных Вами баллов достаточно для получения допуска к экзамену!";
			DisciplineInfo.ExtraRate = new SubModuleItem(-1, -1, "", "Добор баллов", 38, StudentDiscipline.ExtraRate, null);
			DisciplineInfo.MiddleTotalRate = $"Промежуточный итог: { SemesterRate + StudentDiscipline.ExtraRate } / { SemesterMaxRate }";
			DisciplineInfo.ResultHeader2 = "Экзамен";
			DisciplineInfo.ResultSubHeader2 = $"Экзамен по курсу «{ StudentDiscipline.Discipline.SubjectName }»";
			long BonusID = -1;
			Submodule Bonus = null;
			if (DisciplineInfo.IsBonus)
			{
				BonusID = StudentDiscipline.DisciplineMap.Bonus;
				Bonus = StudentDiscipline.Submodules[BonusID.ToString()];
				DisciplineInfo.Bonus = new SubModuleItem(BonusID, -1, "", "Бонусные баллы", Bonus.MaxRate, Bonus.Rate, Bonus.Date);
			}
			var ExamID = StudentDiscipline.DisciplineMap.Exam;
			var Exam = StudentDiscipline.Submodules[ExamID.ToString()];
			DisciplineInfo.Exam = new SubModuleItem(ExamID, -1, "", $"Экзамен по курсу «{ StudentDiscipline.Discipline.SubjectName }»", Exam.MaxRate, Exam.Rate, Exam.Date);
			var Rating = SemesterRate + StudentDiscipline.ExtraRate + (StudentDiscipline.Discipline.IsBonus && Bonus != null && Bonus.Rate != null ? Bonus.Rate : 0) + (Exam.Rate != null ? Exam.Rate : 0);
			DisciplineInfo.FinalTotalRate = $"Итоговый рейтинг: { Math.Min(Rating.Value, 100) } / 100";
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
	public class DisciplineInfo
	{
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
		public bool IsExam { get; set; }
		public bool IsBonus { get; set; }
		public bool IsExtraRate { get; set; }
		public bool IsExamOrBonusOrExtraRate { get; set; }
		/// <summary>
		/// List Header
		/// </summary>
		public string Type { get; set; }
		public string SemesterName { get; set; }
		public string Teachers { get; set; }
		public string StudyLoad { get; set; }

		/// <summary>
		/// List Footer
		/// </summary>
		public string ResultHeader1 { get; set; }
		public string ResultText { get; set; }
		public SubModuleItem ExtraRate { get; set; }
		public string MiddleTotalRate { get; set; }
		public string ResultHeader2 { get; set; }
		public string ResultSubHeader2 { get; set; }
		public SubModuleItem Bonus { get; set; }
		public SubModuleItem Exam { get; set; }
		public string FinalTotalRate { get; set; }
	}
}