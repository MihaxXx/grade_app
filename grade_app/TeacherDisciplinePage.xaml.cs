using Grade;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace grade_app
{
    public partial class TeacherDisciplinePage : TabbedPage
    {
        public TeacherDiscipline TeacherDiscipline { get; private set; } = new();
        public ObservableCollection<SubModulePickerItem> subModulePickerItems { get; private set; } = [];
        public ObservableCollection<DisGroup> GroupedRatesStudentsList { get; private set; } = [];
        public ObservableCollection<DisGroup> FilteredGroupedRatesStudentsList { get; private set; } = [];
        public string subModuleFilter { get; private set; } = "ВСЕ";

        public DisciplineInfo disciplineInfo { get; private set; } = new DisciplineInfo();

        public TeacherJournal TeacherJournal { get; private set; } = new();
        public ObservableCollection<LessonPickerItem> LessonPickerItems { get; private set; } = [];
        public ObservableCollection<DisJourGroup> GroupedJournalStudentsList { get; private set; } = [];
        public ObservableCollection<DisJourGroup> FilteredGroupedJournalStudentsList { get; private set; } = [];

        public string lessonFilter { get; private set; } = "ВСЕ";

        public FormattedString MoreInfo { get; private set; } = new FormattedString();

        public TeacherDisciplinePage(long id, List<Teacher> teachers)
        {
            InitializeComponent();

            _ = LoadData(id, teachers);

            //Must be at the end!!!
            BindingContext = this;
        }

        private async Task LoadData(long id, List<Teacher> teachers)
        {
            activityIndicatorRates.IsRunning = activityIndicatorRates.IsVisible = true;
            activityIndicatorJournal.IsRunning = activityIndicatorJournal.IsVisible = true;
            activityIndicatorInfo.IsRunning = activityIndicatorInfo.IsVisible = true;

            TeacherDiscipline = await App.API.TeacherGetDiscipline(id);
            Title = TeacherDiscipline.Discipline.SubjectName;
            disciplineInfo.DisciplineNotFrozen = !TeacherDiscipline.Discipline.Frozen;
            disciplineInfo.IsDisciplineMapCreated = TeacherDiscipline.Discipline.IsMapCreated;
            FillSubModulePicker();
            if (!TeacherDiscipline.Discipline.IsMapCreated)
            {
                WarningLabel.Text = "Для дисциплины не создана учебная карта";
                WarningLabel.IsVisible = true;
            }
            if (TeacherDiscipline.Discipline.Frozen)
            {
                WarningLabel.Text = "Дисциплина подписана, выставление баллов невозможно";
                WarningLabel.IsVisible = true;
            }
            else if (TeacherDiscipline.Discipline.Milestone > 0 & TeacherDiscipline.Discipline.Milestone < 4)
            {
                WarningLabel.Text = "Семестр завершен, выставление баллов запрещено";
                WarningLabel.IsVisible = true;
            }
            if (TeacherDiscipline.Discipline.Milestone > 0 & TeacherDiscipline.Discipline.Milestone < 4)
            {
                MilestoneLabel.Text = $"Дисциплина находится на этапе №{TeacherDiscipline.Milestone.Id + 1} \"{TeacherDiscipline.Milestone.Name}\"";
                MilestoneLabel.IsVisible = true;
            }
            activityIndicatorRates.IsRunning = activityIndicatorRates.IsVisible = false;

            TeacherJournal = await App.API.TeacherGetDisciplineJournal(id);
            FillLessonPicker();
            if (LessonPickerItems.Count > 0)
                LessonPicker.SelectedIndex = 0;
            activityIndicatorJournal.IsRunning = activityIndicatorJournal.IsVisible = false;

            FillMoreInfo(teachers);
            activityIndicatorInfo.IsRunning = activityIndicatorInfo.IsVisible = false;
        }

        private void FillMoreInfo(List<Teacher> teachers)
        {
            if (TeacherDiscipline.Discipline == null)
                return;
            var Dis = TeacherDiscipline.Discipline;
            AddLineToInfo("Идентификатор", Dis.Id.ToString());
            AddLineToInfo("Учебное подразделение", Dis.FacultyName);
            AddLineToInfo("Предмет", Dis.SubjectName);
            AddLineToInfo("Форма контроля", Dis.LocalizedExamType);
            AddLineToInfo("Лекционных часов", Dis.Lectures.ToString());
            AddLineToInfo("Практических часов", Dis.Practice.ToString());
            AddLineToInfo("Лабораторных часов", Dis.Labs.ToString());
            AddLineToInfo("Семестр", TeacherIndexPage.SemesterList.Find(s => s.Id == Dis.SemesterId)?.ToString() ?? "");
            AddLineToInfo("Этап", TeacherDiscipline.Milestone == null ? "" : $"№{TeacherDiscipline.Milestone.Id + 1} \"{TeacherDiscipline.Milestone.Name}\"");

            AddLineToInfo("Группы", TeacherDiscipline.Groups == null ? "" : ("\n" + string.Join("\n", TeacherDiscipline.Groups.Select(g => $"Группа {g.Value.GroupNum} - {g.Value.SpecName}"))));
            AddLineToInfo("Преподаватели", "\n" + string.Join("\n", teachers.Select(t => t.FullName())));
        }
        private void AddLineToInfo(string title, string value)
        {
            MoreInfo.Spans.Add(new Span { Text = $"{title}: ", FontAttributes = FontAttributes.Bold, FontSize = (double)App.Current!.Resources["MyTitle"] });
            MoreInfo.Spans.Add(new Span { Text = $"{value}\n", FontSize = (double)App.Current!.Resources["MySubtitle"] });
        }

        private void FillSubModulePicker()
        {
            try
            {
                if (TeacherDiscipline == null || TeacherDiscipline.Modules == null)
                    return;
                var RegModules = TeacherDiscipline.Modules.ToList().Where(m => m.Value.Type == ModuleType.Regular).Select(m => m.Key).ToList();
                foreach (var m in TeacherDiscipline.Modules)
                {
                    var ModNum = RegModules.IndexOf(m.Key) + 1;
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
                                subModulePickerItems.Add(new SubModulePickerItem($"М{ModNum}.{sm.Name}", sm.ModuleId, sm.Id, sm.Rate, m.Value.Type));
                                break;
                        }
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
            var smi = (SubModulePickerItem)SubmodulePicker.SelectedItem;
            if (TeacherDiscipline.Discipline.Milestone > 0 & TeacherDiscipline.Discipline.Milestone < 4)
            {
                if (smi.moduleType == ModuleType.Regular || smi.moduleType == ModuleType.Bonus)
                    WarningLabel.IsVisible = true;
                else
                    WarningLabel.IsVisible = false;
            }
            var isInOurMilestone = (TeacherDiscipline.Milestone.Mask & TeacherDiscipline.Modules[smi.ModuleID].Submodules.First(sm => sm.Id == smi.ID).MilestoneMask) > 0;
            disciplineInfo.DisciplineNotFrozen = !TeacherDiscipline.Discipline.Frozen && isInOurMilestone;
            PrepareRatesStudentsList();
        }

        private void PrepareRatesStudentsList()
        {
            var smi = (SubModulePickerItem)SubmodulePicker.SelectedItem;
            GroupedRatesStudentsList.Clear();
            try
            {
                var studentSubmodules = new List<StudentSubmoduleItem>();
                var MaxExtraRate = TeacherDiscipline.Discipline.Type == "exam" ? 38 : 60;
                if (smi != null && TeacherDiscipline != null && TeacherDiscipline.Students != null)
                {
                    foreach (var group in TeacherDiscipline.Students)
                    {
                        var groupInfo = TeacherDiscipline.Groups[group.Key];
                        var disGroup = new DisGroup(groupInfo.Name() + " | " + (groupInfo.SpecAbbr ?? groupInfo.SpecName));
                        foreach (var student in group.Value)
                        {
                            Dictionary<long, int> studentRates =
                                TeacherDiscipline.Rates?.TryGetValue(student.RecordBookId, out var rates) is true && rates is not null ? rates : [];

                            int? Rate = studentRates?.TryGetValue(smi.ID, out int smiRate) is true ? smiRate : null;
                            int SemesterRate = TeacherDiscipline.Modules.Where(m => m.Value.Type == ModuleType.Regular)
                                .Sum(m => m.Value.Submodules.Sum(sm => studentRates?.TryGetValue(sm.Id, out int smRate) is true ? smRate : 0));
                            disGroup.Add(new StudentSubmoduleItem(
                                student.ShortName(),
                                student.RecordBookId,
                                Rate,
                                //TODO: In case of ModuleType.Extra should be MaxExtraRate - SemesterRate - PreviousSubModulesFromExtraModule, current version will lie maxrate on second extra submodule
                                //TODO: Also, In case of ModuleType.Extra would be better to display - instead of 0 on non-positive results (when extra rates is unnecessary) 
                                smi.moduleType == ModuleType.Extra ? Math.Max(MaxExtraRate - SemesterRate, 0) : smi.MaxRate));
                        }
                        GroupedRatesStudentsList.Add(disGroup);
                    }
                    FillFilteredRatesStudentsList();
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        private void FillFilteredRatesStudentsList()
        {
            FilteredGroupedRatesStudentsList.Clear();
            foreach (var g in GroupedRatesStudentsList.Where(g => g.Name == subModuleFilter || subModuleFilter == "ВСЕ"))
                FilteredGroupedRatesStudentsList.Add(g);
        }

        private void FillLessonPicker()
        {
            try
            {
                if (TeacherJournal.Lessons == null)
                    return;

                var OldLessonItems = LessonPickerItems.Select(l => l.ID).ToArray();
                foreach (var l in TeacherJournal.Lessons.Where(les => !OldLessonItems.Contains(les.Id)))
                    LessonPickerItems.Add(new LessonPickerItem($"{l.LessonDate:d} - {TeacherJournal.LessonTypes.First(lt => lt.Id == l.LessonType).Type}{(l.LessonName.Length > 0 ? "(" + l.LessonName + ")" : "")}",
                        TeacherJournal.LessonTypes.First(lt => lt.Id == l.LessonType).Type, l.Id, l.LessonDate));
                LessonPicker.ItemsSource = LessonPickerItems;
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        private void LessonPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            PrepareJournalStudentsList();
        }

        private void PrepareJournalStudentsList()
        {
            var li = (LessonPickerItem)LessonPicker.SelectedItem;
            GroupedJournalStudentsList.Clear();

            try
            {
                var studentLessons = new List<StudentJournalItem>();
                if (li != null && TeacherJournal.Students != null)
                {
                    //TODO: respect subgroup information
                    foreach (var group in TeacherJournal.Students)
                    {
                        var groupInfo = TeacherJournal.Groups[group.Key];
                        var disGroup = new DisJourGroup(groupInfo.Name() + " | " + (groupInfo.SpecAbbr ?? groupInfo.SpecName));
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
                        GroupedJournalStudentsList.Add(disGroup);
                    }
                    FillFilteredJournalStudentsList();
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        private void FillFilteredJournalStudentsList()
        {
            FilteredGroupedJournalStudentsList.Clear();
            foreach (var g in GroupedJournalStudentsList.Where(g => g.Name == lessonFilter || lessonFilter == "ВСЕ"))
                FilteredGroupedJournalStudentsList.Add(g);
        }

        async private void AddLesson_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is ToolbarItem { AutomationId: "add_lesson" })
                {
                    string action = await DisplayActionSheet("Выберите тип создаваемого занятия от сегодняшнего числа", "Отмена", null, TeacherJournal.LessonTypes.Select(lt => lt.Type).ToArray());
                    if (action != null && action != "Отмена")
                    {
                        var res = App.API.TeacherPostCreateLesson(TeacherJournal.Discipline.Id, DateTime.Today, TeacherJournal.LessonTypes.First(lt => lt.Type == action));
                        if (res.Item1 == false)
                        {
                            await DisplayAlert("CreateLesson error", res.Item2, "OK");
                        }
                        else
                        {
                            TeacherJournal = await App.API.TeacherGetDisciplineJournal(TeacherJournal.Discipline.Id);
                            FillLessonPicker();
                            LessonPicker.SelectedIndex = LessonPickerItems.Count - 1;
                        }
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        async private void FilterSubModule_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is ToolbarItem { AutomationId: "filter_submodule" })
                {
                    var listOfGroups = GroupedRatesStudentsList.Select(g => g.Name).ToList();
                    listOfGroups.Add("ВСЕ");
                    string action = await DisplayActionSheet("Фильтр: выберите группу", "Отмена", null, listOfGroups.ToArray());
                    if (action != null && action != "Отмена")
                    {
                        subModuleFilter = action;
                        FillFilteredRatesStudentsList();
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        async private void FilterLesson_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is ToolbarItem { AutomationId: "filter_lesson" })
                {
                    var listOfGroups = GroupedJournalStudentsList.Select(g => g.Name).ToList();
                    listOfGroups.Add("ВСЕ");
                    string action = await DisplayActionSheet("Фильтр: выберите группу", "Отмена", null, listOfGroups.ToArray());
                    if (action != null && action != "Отмена")
                    {
                        lessonFilter = action;
                        FillFilteredJournalStudentsList();
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        async private void DeleteLesson_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is ToolbarItem { AutomationId: "delete_lesson" })
                {
                    var li = (LessonPickerItem)LessonPicker.SelectedItem;
                    var answ = await DisplayAlert("Удалить занятие?", $"Вы действительно хотите удалить занятие \"{li.Name}\"?", "Да", "Отмена");
                    if (answ)
                    {
                        var res = App.API.TeacherPostDeleteLesson(TeacherJournal.Discipline.Id, li.ID);
                        if (res.Item1 == false)
                        {
                            await DisplayAlert("DeleteLesson error", res.Item2, "OK");
                        }
                        else
                        {
                            var prevSelectedIdx = LessonPicker.SelectedIndex;
                            LessonPickerItems.Remove(li);
                            if (LessonPickerItems.Count > 0)
                                LessonPicker.SelectedIndex = prevSelectedIdx > 0 ? prevSelectedIdx - 1 : 0;
                        }
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
            if (sender is Entry entry)
            {
                try
                {
                    var student = entry.Parent.Parent.BindingContext as StudentSubmoduleItem;
                    var submodule = (SubModulePickerItem)SubmodulePicker.SelectedItem;

                    if (student?.Rate.HasValue is not true || student.Rate > student.MaxRate)
                    {
                        entry.BackgroundColor = Color.FromArgb("#a94442");
                        return;
                    }

                    entry.BackgroundColor = null;

                    var res = App.API.TeacherPostSetRate(student.RecordBookId, TeacherDiscipline.Discipline.Id, submodule.ID, student.Rate.Value);

                    if (res.Item1 == false)
                    {
                        DisplayAlert("SetRate error", res.Item2, "OK");
                    }
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        private void Entry_Focused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                entry.CursorPosition = entry.Text?.Length ?? 0;
            }
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            try
            {
                if ((sender as CheckBox)?.Parent.Parent.BindingContext is StudentJournalItem student && (student.Attendance ?? false) != e.Value)
                {
                    student.Attendance = e.Value;
                    if (LessonPicker.SelectedItem is not LessonPickerItem lesson || !student.Attendance.HasValue)
                        return;

                    var res = App.API.TeacherPostSetAttendance(lesson.ID, student.RecordBookId, student.Attendance.Value);
                    if (res.Item1 == false)
                    {
                        DisplayAlert("SetAttendance error", res.Item2, "OK");
                    }
                    else
                    {
                        if (!TeacherJournal.Attendance.ContainsKey(student.RecordBookId))
                            TeacherJournal.Attendance[student.RecordBookId] = [];
                        TeacherJournal.Attendance[student.RecordBookId][lesson.ID] = student.Attendance.Value ? 1 : 0;
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void RatesItem_Tapped(object sender, EventArgs e)
        {
            var vs = (ViewCell)sender;
            var entry = vs.FindByName<Entry>("vsEntry");
            entry?.Focus();
        }

        private void LessonItem_Tapped(object sender, EventArgs e)
        {
            var vs = (ViewCell)sender;
            var checkBox = vs.FindByName<CheckBox>("vsCheckBox");
            if (checkBox != null)
                checkBox.IsChecked = !checkBox.IsChecked;
        }

        public class DisciplineInfo : INotifyPropertyChanged
        {
            private bool disciplineNotFrozen;
            private bool isDisciplineMapCreated;

            public event PropertyChangedEventHandler? PropertyChanged;

            public bool DisciplineNotFrozen
            {
                get => disciplineNotFrozen; set
                {
                    disciplineNotFrozen = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisciplineNotFrozen)));
                }
            }
            public bool IsDisciplineMapCreated
            {
                get => isDisciplineMapCreated; set
                {
                    isDisciplineMapCreated = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDisciplineMapCreated)));
                }
            }
        }
    }

    public class StudentSubmoduleItem(string name, long recordBookId, int? rate, int maxRate)
    {
        public string Name { get; set; } = name;
        public long RecordBookId { get; set; } = recordBookId;
        public int? Rate { get; set; } = rate;
        public int MaxRate { get; set; } = maxRate;
    }
    public class SubModulePickerItem(string name, long moduleID, long iD, int maxRate, ModuleType moduleType)
    {
        public string Name { get; set; } = name;
        public long ModuleID { get; set; } = moduleID;
        public long ID { get; set; } = iD;
        public int MaxRate { get; set; } = maxRate;
        public ModuleType moduleType { get; set; } = moduleType;
    }
    public class DisGroup(string name) : List<StudentSubmoduleItem>
    {
        public string Name { get; set; } = name;
    }
    public class StudentJournalItem(string name, long recordBookId, bool? attendance)
    {
        public string Name { get; set; } = name;
        public long RecordBookId { get; set; } = recordBookId;
        public bool? Attendance { get; set; } = attendance;
    }

    public class LessonPickerItem(string name, string type, long iD, DateTime date)
    {
        public string Name { get; set; } = name;
        public string Type { get; set; } = type;
        public long ID { get; set; } = iD;
        public DateTime Date { get; set; } = date;
    }

    public class DisJourGroup(string name) : List<StudentJournalItem>
    {
        public string Name { get; set; } = name;
    }
}
