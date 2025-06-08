using Grade;
using System.Collections.ObjectModel;

namespace grade_app
{
    public partial class StudentIndexPage : ContentPage
    {
        StudentIndex studentIndex = new();
        List<Semester> SemesterList { get; set; } = [];

        public long CurrentSemID { get; set; }

        public ObservableCollection<DisciplineItem> DisciplineItems { get; private set; } = [];
        public StudentIndexPage()
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
            SemesterList.Clear();
            SemesterList.AddRange(await App.API.GetSemesterList());
        }

        private async Task LoadDisciplines(long SemesterID)
        {
            DisciplineItems.Clear();
            EmptyListText.IsVisible = false;
            activityIndicator.IsRunning = activityIndicator.IsVisible = true;
            studentIndex = await App.API.StudentGetIndex(SemesterID);
            CurrentSemID = SemesterID;
            Title = $"БРС - {SemesterList.Find(s => s.Id == CurrentSemID)}";
            if (studentIndex.Disciplines.Length != 0)
            {
                foreach (var d in studentIndex.Disciplines)
                {
                    var percent = (d.MaxCurrentRate != 0 ? ((d.Rate == null) ? 0 : Math.Min((int)d.Rate, 100)) / (double)d.MaxCurrentRate : 0).
                        ToString("P0");
                    var repeatCount = Math.Max(1.ToString("P0").Length - percent.Length, 0);
                    /// Ugly fix to make column width equal for every row regardless of UI scale and screen size, center allined text
                    percent = string.Concat(Enumerable.Repeat(" ", repeatCount)) + percent + string.Concat(Enumerable.Repeat(" ", repeatCount));
                    var rates = $"{((d.Rate == null) ? 0 : d.Rate)}/{d.MaxCurrentRate}/100";
                    /// Ugly fix to make column width equal for every row regardless of UI scale and screen size, end allind text
                    rates = string.Concat(Enumerable.Repeat(" ", ((3 * 3 + 2) - rates.Length) * 2)) + rates;
                    DisciplineItems.Add(new DisciplineItem(d.Id, percent, d.SubjectName, rates));
                }
            }
            else
            {
                EmptyListText.IsVisible = true;
            }
            activityIndicator.IsRunning = activityIndicator.IsVisible = false;
        }
        private async void OnListItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
            if (!App.IsInternetConnected())
            {
                await DisplayAlert("Нет доступа к интернету!", "Попробуйте снова позднее.", "ОК");
                return;
            }
            var item = (DisciplineItem)e.Item;
            await Navigation.PushAsync(new StudentDisciplinePage(item.ID));
        }
        async void OnToolbarItemClicked(object sender, EventArgs e)
        {
            try
            {
                var item = sender as ToolbarItem;
                if (item?.AutomationId == "change_semester")
                {
                    if (!App.IsInternetConnected())
                    {
                        await DisplayAlert("Нет доступа к интернету!", "Попробуйте снова позднее.", "ОК");
                        return;
                    }
                    var semlist = SemesterList.Select(sem => sem.ToString()).ToList();
                    string action = await DisplayActionSheet("Выберите семестр", "Отмена", null, [.. semlist]);
                    if (action != null && action != "Отмена")
                    {
                        var res = SemesterList[semlist.FindIndex(sem => sem == action)];
                        if (res.Id != CurrentSemID)
                            _ = LoadDisciplines(res.Id);
                    }
                }
                else if (item?.AutomationId == "logout")
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
    }
    public class DisciplineItem(long id, string percent, string name, string rate)
    {
        public long ID { get; set; } = id;
        public string Percent { get; set; } = percent;
        public string Name { get; set; } = name;
        public string Rate { get; set; } = rate;
    }
}
