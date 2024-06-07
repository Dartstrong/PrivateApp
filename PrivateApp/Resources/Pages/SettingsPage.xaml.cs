namespace PrivateApp
{
	public partial class SettingsPage : ContentPage
	{
        private string _fileName = Path.Combine(FileSystem.Current.AppDataDirectory, "AppPassword.dat");
        public SettingsPage(int sessionId, string userName, string deviceId)
		{
			InitializeComponent();
			this.userName.Text = $"����� ��������: {userName}";
			this.deviceId.Text = $"���������� ID ����������: {deviceId}";
            this.sessionId.Text = $"ID ������� ������: {sessionId}";
        }
        private void BackButtonClicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }
        private async void ClearPasswordButtonClicked(object sender, System.EventArgs e)
        {
            File.Delete(_fileName);
            await DisplayAlert("�����������", "������ ������� �������", "�K");
        }
        
    }
}

