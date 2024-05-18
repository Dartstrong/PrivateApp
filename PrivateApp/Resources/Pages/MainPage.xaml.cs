using PrivateApp.Resources.Entity;
namespace PrivateApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage(int sessionId, byte[] sessionKey, byte[] sessionInitVector, AuthorizationData user)
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
