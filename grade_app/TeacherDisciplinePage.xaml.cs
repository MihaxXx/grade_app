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
		public long CurrentSubModuleID { get; set; }
		public List<SubModulePickerItem> subModulePickerItems { get; private set; } = new List<SubModulePickerItem>();
		public List<StudentSubmoduleItem> studentSubmoduleItems { get; private set; } = new List<StudentSubmoduleItem>();

		public ObservableCollection<DisGroup> GroupedStudentItems { get; private set; } = new ObservableCollection<DisGroup>();


		public TeacherDisciplinePage(long id)
		{
			InitializeComponent();

			TeacherDiscipline = App.API.TeacherGetDiscipline(id);
			FillSubModulePicker();
			FillStudentsList();

			//Must be at the end!!!
			BindingContext = this;
		}

		private void FillStudentsList()
		{
			var smi = (SubModulePickerItem)SubmodulePicker.SelectedItem;

			GroupedStudentItems.Clear();
			var studentSubmodules = new List<StudentSubmoduleItem>();
			if(TeacherDiscipline.Students!= null)
			{
				foreach(var group in TeacherDiscipline.Students)
				{
					var groupInfo = TeacherDiscipline.Groups[group.Key];
					var disGroup = new DisGroup(groupInfo.Name() + " | " + groupInfo.SpecAbbr);
					foreach (var student in group.Value)
						disGroup.Add(new StudentSubmoduleItem(
							student.ShortName(),
							student.Id,
							TeacherDiscipline.Rates.ContainsKey(student.RecordBookId) ?
								TeacherDiscipline.Rates[student.RecordBookId].ContainsKey(smi.ID) ?
									new int?(TeacherDiscipline.Rates[student.RecordBookId][smi.ID]) :
									0 :
								null,
							smi.MaxRate));
					GroupedStudentItems.Add(disGroup);
				}
			}
		}

		private void FillSubModulePicker()
		{
			foreach (var m in TeacherDiscipline.Modules)
				foreach (var sm in m.Value.Submodules)
					subModulePickerItems.Add(new SubModulePickerItem(sm.Name, sm.ModuleId, sm.Id, sm.Rate));
			SubmodulePicker.ItemsSource = subModulePickerItems;
			SubmodulePicker.SelectedIndex = 0;
		}

		private void SubmodulePicker_SelectedIndexChanged(object sender, EventArgs e)
		{
			FillStudentsList();
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
	public class DisGroup :List<StudentSubmoduleItem>
	{
		public string Name { get; set; }
		public DisGroup(string name)
		{
			Name = name;
		}
		public static IList<DisGroup> All { private set; get; }
	}
}