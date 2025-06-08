using Grade;

namespace grade_app
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async private void Enter_btnClicked(object sender, EventArgs e)
        {
            if (!App.IsInternetConnected())
            {
                await DisplayAlert("Нет доступа к интернету!", "Попробуйте снова позднее.", "ОК");
                return;
            }

            if (!pass.IsVisible)
            {
                if (login.Text != null && (login.Text.Contains("@sfedu.ru") || !login.Text.Contains('@')))
                    await Navigation.PushAsync(new AuthPage(login.Text.Contains(@"@sfedu.ru") ? login.Text[..login.Text.IndexOf('@')] : login.Text, UserRole.SelectedItem.ToString() == "Студент" ? "student" : "staff"));
                else
                    await DisplayAlert("Неверный Email", "Email должен быть в домене @sfedu.ru", "ОК");
            }
            else
            {
                if (login.Text != null && pass.Text != null)
                {
                    var res = API.PostGetToken(login.Text, pass.Text);
                    if (res.Item1)
                    {
                        var token = res.Item2;
                        App.InitUser(token, UserRole.SelectedItem.ToString() == "Студент" ? Role.Student : Role.Teacher);
                        try
                        {
                            var _ = await App.API.GetSemesterList();
                        }
                        catch (Exception)
                        {
                            App.WipeUser();
                            App.InitUser(token, UserRole.SelectedItem.ToString() == "Студент" ? Role.Teacher : Role.Student);
                        }
                        finally
                        {
                            if (App.API.role == Role.Student)
                                Navigation.InsertPageBefore(new StudentIndexPage(), Navigation.NavigationStack[0]);
                            else
                                Navigation.InsertPageBefore(new TeacherIndexPage(), Navigation.NavigationStack[0]);
                        }
                        await Navigation.PopToRootAsync();
                    }
                    else
                        await DisplayAlert("Ошибка", res.Item2, "ОК");
                }
                else
                    await DisplayAlert($"Неверный {(login.Text == null ? "логин" : "пароль")}", $"Поле \"{(login.Text == null ? "Логин" : "Пароль")}\" не может быть пустым", "ОК");
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (SignInLocalLabel.Text == "Вход через локальную учетную запись")
            {
                SignInLocalLabel.Text = "Вход через Microsoft";
                EmailLabel.Text = "Логин или Email";
                login.Placeholder = "";
                Enter_btn.Text = "Войти";
                passLabel.IsVisible = true;
                pass.IsVisible = true;
            }
            else
            {
                SignInLocalLabel.Text = "Вход через локальную учетную запись";
                EmailLabel.Text = "Email";
                login.Placeholder = "Логин@sfedu.ru";
                Enter_btn.Text = "Войти через сервис Microsoft";
                passLabel.IsVisible = false;
                pass.IsVisible = false;

            }
        }
    }


}
