namespace PrivateApp
{
	public partial class SettingsPage : ContentPage
	{
        private string _fileName = Path.Combine(FileSystem.Current.AppDataDirectory, "AppPassword.dat");
        public SettingsPage(int sessionId, string userName, string deviceId)
		{
			InitializeComponent();
			this.userName.Text = $"Логин аккаунта: {userName}";
			this.deviceId.Text = $"Уникальный ID устройства: {deviceId}";
            this.sessionId.Text = $"ID текущей сессии: {sessionId}";
        }
        private void BackButtonClicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }
        private async void ClearPasswordButtonClicked(object sender, System.EventArgs e)
        {
            File.Delete(_fileName);
            await DisplayAlert("Уведомление", "Пароль успешно сброшен", "ОK");
        }
        
    }
}

