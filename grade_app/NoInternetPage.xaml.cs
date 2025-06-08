
namespace grade_app
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NoInternetPage : ContentPage
    {
        public NoInternetPage()
        {
            InitializeComponent();
        }

        private async void Button_Pressed(object sender, EventArgs e)
        {
            if (App.IsInternetConnected())
                App.Current.Windows[0].Page = App.GetStartPage();
            else
                await App.DisplayToastAsync("Соединение с интернетом всё ещё не обнаружено");
        }
    }
}
