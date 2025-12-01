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

			// Request notification permission for Android 13+ (API 33+) asynchronously
			// This prevents blocking the UI thread during app startup
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
			{
				if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Permission.Granted)
				{
					// Request permission asynchronously - don't block UI
					_ = Task.Run(async () =>
					{
						await Task.Delay(500); // Small delay to let UI initialize
						MainThread.BeginInvokeOnMainThread(() =>
						{
							RequestPermissions(new[] { Android.Manifest.Permission.PostNotifications }, 1);
						});
					});
				}
				else
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine("[MainActivity] POST_NOTIFICATIONS permission already granted");
#endif
				}
			}
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			
			if (requestCode == 1 && grantResults.Length > 0)
			{
				if (grantResults[0] == Permission.Granted)
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine("[MainActivity] POST_NOTIFICATIONS permission granted");
#endif
				}
				else
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine("[MainActivity] POST_NOTIFICATIONS permission denied");
#endif
				}
			}
		}
	}
}
