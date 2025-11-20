using Android.App;
using Android.Runtime;
using Android.OS;

namespace StudentProgressTracker
{
	[Application]
	public class MainApplication : MauiApplication
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}

		public override void OnCreate()
		{
			base.OnCreate();
			CreateNotificationChannel();
		}

		private void CreateNotificationChannel()
		{
			// Create notification channel for Android 8.0+ (API 26+)
			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				var channelId = "general";
				var channelName = "General Notifications";
				var channelDescription = "Notifications for courses and assessments";
				var importance = NotificationImportance.High;

				var channel = new NotificationChannel(channelId, channelName, importance)
				{
					Description = channelDescription
				};
				channel.EnableVibration(true);
				channel.EnableLights(true);

				var notificationManager = (NotificationManager?)GetSystemService(NotificationService);
				notificationManager?.CreateNotificationChannel(channel);

#if DEBUG
				System.Diagnostics.Debug.WriteLine("[MainApplication] Notification channel created");
#endif
			}
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
