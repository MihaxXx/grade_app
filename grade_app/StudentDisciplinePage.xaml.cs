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
		StudentDiscipline studentDiscipline;
		
		public List<SubModuleItem> SubModuleItems { get; private set; }

		public IEnumerable<IGrouping<string, SubModuleItem>> GroupedSubModules { get; private set; }
		public StudentDisciplinePage(long id)
		{
			InitializeComponent();

			studentDiscipline = App.API.StudentGetDiscipline(id);
			Title = studentDiscipline.Discipline.SubjectName;
			SubModuleItems = new List<SubModuleItem>();
			foreach(var m in studentDiscipline.DisciplineMap.Modules)
			{
				foreach (var smID in m.Value.Submodules)
				{
					var smVal = studentDiscipline.Submodules[smID.ToString()];
					SubModuleItems.Add(new SubModuleItem(smID, long.Parse(m.Key), m.Value.Title,smVal.Title,smVal.MaxRate,smVal.Rate,smVal.Date));
				}
			}
			GroupedSubModules = SubModuleItems.GroupBy(sm => sm.ModuleTitle);

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
			Rate = $"{(rate.HasValue? rate.Value : 0)}/{(maxRate.HasValue ? maxRate.Value : 0)}";
			Percent = ((double)(rate.HasValue ? rate.Value : 0)/(maxRate.HasValue ? maxRate.Value : 0)).ToString("P0");
			Date = date.HasValue? date.Value.ToString("d") : "—";
		}

		public long ID { get; set; }
		public long ModuleID { get; set; }
		public string ModuleTitle { get; set; }
		public string Title { get; set; }
		public string Rate { get; set; }
		public string Percent { get; set; }
		public string Date { get; set; }

	}
}