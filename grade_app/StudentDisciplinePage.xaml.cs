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
	//[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StudentDisciplinePage : ContentPage
	{
		public StudentDiscipline studentDiscipline { get; private set; }

		public List<SubModuleItem> SubModuleItems { get; private set; }

		public DisciplineInfo disciplineInfo { get; private set; }

		public IEnumerable<IGrouping<string, SubModuleItem>> GroupedSubModules { get; private set; }
		public StudentDisciplinePage(long id)
		{
			InitializeComponent();

			int? SemesterRate = 0;
			int? SemesterMaxRate = 0;

			studentDiscipline = App.API.StudentGetDiscipline(id);
			//Title = studentDiscipline.Discipline.SubjectName;
			SubModuleItems = new List<SubModuleItem>();
			foreach (var m in studentDiscipline.DisciplineMap.Modules)
			{
				foreach (var smID in m.Value.Submodules)
				{
					var smVal = studentDiscipline.Submodules[smID.ToString()];
					SubModuleItems.Add(new SubModuleItem(smID, long.Parse(m.Key), m.Value.Title, smVal.Title, smVal.MaxRate, smVal.Rate, smVal.Date));

					SemesterRate += (smVal.Rate == null? 0 : smVal.Rate);
					SemesterMaxRate += (smVal.MaxRate == null? 0: smVal.MaxRate);
				}
			}
			GroupedSubModules = SubModuleItems.GroupBy(sm => sm.ModuleTitle);

			disciplineInfo = new DisciplineInfo();
			{
				disciplineInfo.Type = studentDiscipline.Discipline.Type switch
				{
					"exam" => "Экзамен",
					"credit" => "Зачет",
					"grading_credit" => "Дифф. зачет",
					_ => studentDiscipline.Discipline.Type,
				};
				var year = studentDiscipline.Discipline.SemesterYear;
				disciplineInfo.SemesterName = $"{(studentDiscipline.Discipline.SemesterNum == 1 ? "Осенний" : "Весенний")} семестр {year}/{year + 1} учебного года";
				disciplineInfo.Teachers = string.Join('\n', studentDiscipline.Teachers.Select(t => t.Name));
				disciplineInfo.StudyLoad = DisciplineInfo.StudyLoadToText(studentDiscipline.Discipline.Lectures, studentDiscipline.Discipline.Practice, studentDiscipline.Discipline.Labs);

				var IsExam = studentDiscipline.Discipline.Type == "exam";
				if (IsExam)
				{
					disciplineInfo.ResultHeader1 = "Допуск к экзамену";
					var Admission = 38 - (SemesterRate + studentDiscipline.ExtraRate);
					//TODO: Fix num ending
					disciplineInfo.ResultText = Admission > 0 ? 
						$"Для допуска к экзамену Вам необходимо получить еще { Admission } баллов.":
						"Поздравляем, заработанных Вами баллов достаточно для получения допуска к экзамену!";
					//((StackLayout)this.Content).Children;
					disciplineInfo.ExtraRate = new SubModuleItem(-1, -1, "", "Добор баллов", 38, studentDiscipline.ExtraRate, null);
					disciplineInfo.MiddleTotalRate = $"Промежуточный итог: { SemesterRate + studentDiscipline.ExtraRate } / { SemesterMaxRate }";
					disciplineInfo.ResultHeader2 = "Экзамен";
					disciplineInfo.ResultSubHeader2 = $"Экзамен по курсу «{ studentDiscipline.Discipline.SubjectName }»";

					//TODO: Fix Bonus might be null
						var BonusID = studentDiscipline.DisciplineMap.Bonus;
						var Bonus = studentDiscipline.Submodules[BonusID.ToString()];
						disciplineInfo.Bonus = new SubModuleItem(BonusID, -1, "", "Бонусные баллы",Bonus.MaxRate,Bonus.Rate,Bonus.Date);

					var ExamID = studentDiscipline.DisciplineMap.Exam;
					var Exam = studentDiscipline.Submodules[ExamID.ToString()];
					disciplineInfo.Exam = new SubModuleItem(ExamID, -1, "", $"Экзамен по курсу «{ studentDiscipline.Discipline.SubjectName }»", Exam.MaxRate, Exam.Rate, Exam.Date);
					var Rating = SemesterRate + studentDiscipline.ExtraRate + (studentDiscipline.Discipline.IsBonus && Bonus.Rate != null ? Bonus.Rate:0) + (Exam.Rate!=null? Exam.Rate:0);
					disciplineInfo.FinalTotalRate = $"Итоговый рейтинг: { Math.Min(Rating.Value, 100) } / 100";
				}
				else
				{

				}
				
			};

			//Must be at the end!!!
			BindingContext = this;
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
			Rate = $"{(rate.HasValue ? rate.Value : 0)}/{(maxRate.HasValue ? maxRate.Value : 0)}";
			Percent = ((double)(rate.HasValue ? rate.Value : 0) / (maxRate.HasValue ? maxRate.Value : 0)).ToString("P0");
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