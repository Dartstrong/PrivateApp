namespace PrivateApp
{
	public partial class OutcomingRequestsPage : ContentPage
	{
		public OutcomingRequestsPage()
		{
			InitializeComponent();
		}
        private void BackButtonClicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }
        private async void PlusButtonClicked(object sender, System.EventArgs e)
        {
            var receiverLogin = await DisplayPromptAsync("Отправка заявки", "Введите логин пользователя с которым хотите начать  диалог:", "OK", "Отмена");
        }
    }
}

