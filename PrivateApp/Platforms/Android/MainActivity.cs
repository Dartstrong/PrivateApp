using Android.App;
using Android.Content.PM;
using Android.OS;

namespace PrivateApp
{
    [Activity(Theme = "@style/SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity{    
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#282828"));
            Window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#282828"));

            base.OnCreate(savedInstanceState);
        }
    }

}
