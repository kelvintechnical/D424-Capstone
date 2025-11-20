using Android.App;
using Android.Content.PM;
using Android.OS;

namespace StudentProgressTracker
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Request notification permission for Android 13+ (API 33+)
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
			{
				if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Permission.Granted)
				{
					RequestPermissions(new[] { Android.Manifest.Permission.PostNotifications }, 1);
				}
			}
		}
	}
}
