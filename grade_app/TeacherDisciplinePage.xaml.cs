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
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TeacherDisciplinePage : TabbedPage
	{
		public TeacherDiscipline TeacherDiscipline { get; private set; }
		public List<SubModulePickerItem> subModulePickerItems { get; private set; } = new List<SubModulePickerItem>();
		public List<StudentSubmoduleItem> studentSubmoduleItems { get; private set; } = new List<StudentSubmoduleItem>();
		public TeacherDisciplinePage(long id)
		{
			InitializeComponent();

			TeacherDiscipline = App.API.TeacherGetDiscipline(id);
			foreach (var m in TeacherDiscipline.Modules)
				foreach (var sm in m.Value.Submodules)
					subModulePickerItems.Add(new SubModulePickerItem(sm.Name, sm.ModuleId, sm.Id, sm.Rate));
			SubmodulePicker.ItemsSource = subModulePickerItems;
			SubmodulePicker.SelectedIndex = 0;

			var Students = TeacherDiscipline.Students.Values.SelectMany(item => item);
			var smi = (SubModulePickerItem)SubmodulePicker.SelectedItem;
			studentSubmoduleItems = Students.Select(s =>
			new StudentSubmoduleItem(s.ShortName(), s.Id,TeacherDiscipline.Rates.ContainsKey(s.RecordBookId)? new int? (TeacherDiscipline.Rates[s.RecordBookId][smi.ID]) : null, smi.MaxRate)).ToList();

			//Must be at the end!!!
			BindingContext = this;
		}
	}

	public class StudentSubmoduleItem
	{
		public StudentSubmoduleItem(string name, long id, int? rate, int maxRate)
		{
			Name = name;
			Id = id;
			Rate = rate;
			MaxRate = maxRate;
		}

		public string Name { get; set; }
		public long Id { get; set; }
		public int? Rate { get; set; }
		public int MaxRate { get; set; }

	}
	public class SubModulePickerItem
	{
		public SubModulePickerItem(string name, long moduleID, long iD, int maxRate)
		{
			Name = name;
			ModuleID = moduleID;
			ID = iD;
			MaxRate = maxRate;
		}

		public string Name { get; set; }
		public long ModuleID { get; set; }
		public long ID { get; set; }
		public int MaxRate { get; set; }
	}
}