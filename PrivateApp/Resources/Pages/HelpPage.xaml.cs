namespace PrivateApp
{	
	public partial class HelpPage : ContentPage
	{
		public HelpPage()
		{
			InitializeComponent();
		}
        private void BackButtonClicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}

