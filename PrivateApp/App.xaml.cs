using Microsoft.VisualBasic;
using PrivateApp.Resources.Entity;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace PrivateApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new AppLoginPage());
        }
    }
}
