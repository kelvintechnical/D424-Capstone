using Android.App;
using Android.Runtime;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

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

			// Let the plugin create and manage the notification channel
			LocalNotificationCenter.CreateNotificationChannel(new NotificationChannelRequest
			{
				Id = "general",
				Name = "General Notifications",
				Description = "Notifications for courses and assessments",
				Importance = NotificationImportance.High
			});

			// CRITICAL: Initialize the plugin to register receivers
			LocalNotificationCenter.Current.Initialize();

#if DEBUG
			System.Diagnostics.Debug.WriteLine("[MainApplication] LocalNotificationCenter initialized and channel created");
#endif
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
