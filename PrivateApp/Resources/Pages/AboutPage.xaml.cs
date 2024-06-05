namespace PrivateApp
{    
    public partial class AboutPage : ContentPage
    {
	    public AboutPage()
	    {
		    InitializeComponent();
	    }
        private void BackButtonClicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}

